using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Models;
using Harvest.Core.Models.InvoiceModels;
using Harvest.Core.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace Harvest.Core.Services
{
    public interface IInvoiceService
    {
        Task<Result<bool>> InitiateCloseout(int projectId);
        Task<Result<int>> CreateInvoice(int projectId, bool isCloseout = false, bool isAutoCloseout = false);
        Task<int> CreateInvoices(bool manualOverride = false);
        Task<List<int>> GetCreatedInvoiceIds();

        Task<int> AutoCloseoutProjects(int daysBeforeAutoCloseout);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _dbContext;
        private readonly IProjectHistoryService _historyService;
        private readonly IEmailService _emailService;
        private readonly IExpenseService _expenseService;
        private readonly IDateTimeService _dateTimeService;
        private readonly DevSettings _devSettings;
        private readonly AggieEnterpriseOptions _aeSettings;

        public InvoiceService(AppDbContext dbContext, IProjectHistoryService historyService,
            IEmailService emailService, IExpenseService expenseService, IOptions<DevSettings> devSettings, IDateTimeService dateTimeService, IOptions<AggieEnterpriseOptions> options)
        {
            _dbContext = dbContext;
            _historyService = historyService;
            _emailService = emailService;
            _expenseService = expenseService;
            _dateTimeService = dateTimeService;
            _devSettings = devSettings.Value;
            _aeSettings = options.Value;
        }

        public async Task<Result<bool>> InitiateCloseout(int projectId)
        {
            var project = await _dbContext.Projects.Include(p => p.PrincipalInvestigator).SingleAsync(p => p.Id == projectId);

            if (project.IsActive == false)
            {
                return Result.Error("Project is not Active");
            }

            if (project.Status != Project.Statuses.Active && project.Status != Project.Statuses.AwaitingCloseout)
            {
                return Result.Error("Project status is not Active or AwaitingCloseout");
            }

            var unbilledExpenses = await _dbContext.Expenses.Where(
                e => e.InvoiceId == null && e.ProjectId == projectId).SumAsync(e => e.Total);
            if (unbilledExpenses + project.ChargedTotal > project.QuoteTotal)
            {
                return Result.Error("Project has unbilled expenses that exceed the quote total");
            }

            if (!await _emailService.CloseoutConfirmation(project))
            {
                return Result.Error("Failed to send confirmation email");
            }

            project.UpdateStatus(Project.Statuses.PendingCloseoutApproval);

            await _historyService.ProjectCloseoutInitiated(project.Id, project);

            await _dbContext.SaveChangesAsync();

            return Result.Value(true, "Closeout initiated. An approval request has been sent to project's PI.");
        }

        public async Task<Result<int>> CreateInvoice(int projectId, bool isCloseout = false, bool isAutoCloseout = false)
        {
            var now = _dateTimeService.DateTimeUtcNow();// DateTime.UtcNow;

            //Look for an active project
            var project = await _dbContext.Projects.Include(a => a.AcreageRate)
                .Where(a =>
                    a.IsActive &&
                    (a.Status == Project.Statuses.Active ||
                        (isCloseout && a.Status == Project.Statuses.PendingCloseoutApproval)) &&
                    a.Id == projectId).SingleOrDefaultAsync();
            if (project == null)
            {
                return Result.Error("No active project found for given projectId: {projectId}", projectId);
            }

            if(isAutoCloseout)
            {
                await _historyService.ProjectAutoCloseout(projectId, project); 
            }

            if (isCloseout && project.Status != Project.Statuses.PendingCloseoutApproval)
            {
                return Result.Error("Closeout can only happen for projects with the Pending Closeout Approval status projectId: {projectId}", projectId);
            }

            if (!isCloseout && !_devSettings.NightlyInvoices)
            {
                if (!now.IsBusinessDay())
                {
                    return Result.Error("Invoices can only be created Monday through Friday: {projectId}", projectId);
                }

                //Check to see if there is an invoice within current month
                if (await _dbContext.Invoices.AnyAsync(a => a.ProjectId == projectId && a.CreatedOn.Year == now.Year && a.CreatedOn.Month == now.Month))
                {
                    return Result.Error("An invoice already exists for current month: {projectId}", projectId);
                }

                var prjStartFirstOfMonth = new DateTime(project.Start.Year, project.Start.Month, 1).Date.AddMonths(1).FromPacificTime();
                if (prjStartFirstOfMonth > now) //Start doing invoices on the first month after the project starts
                {
                    return Result.Error("Project has not yet started in following month: {projectId}", projectId);
                }

                //Don't create an invoice for project past its end date, and just mark it as AwaitingCloseout
                if (project.End < now)
                {
                    project.UpdateStatus(Project.Statuses.AwaitingCloseout);
                    await _dbContext.SaveChangesAsync();
                    return Result.Error("Can't create invoice for project past its end date: {projectId}", projectId);
                }

                //Acreage fees are ignored for manually created invoices
                await _expenseService.CreateYearlyAcreageExpense(project); //I don't think we want to call the acreage expense on closeout. It will be called on the yearly anniversary 
            }
            else if (!isCloseout)
            {
                await _expenseService.CreateYearlyAcreageExpense(project); //Call it for the nightly invoices though.
            }

            //Don't exceed quoted amount
            var allExpenses = await _dbContext.Expenses.Where(e => e.Approved && e.ProjectId == projectId).ToArrayAsync();
            var totalExpenses = allExpenses.Sum(e => e.Total);
            if (totalExpenses > project.QuoteTotal)
            {
                var billedTotal = allExpenses.Where(e => e.InvoiceId != null).Sum(e => e.Total);
                var invoiceAmount = allExpenses.Where(e => e.InvoiceId == null).Sum(e => e.Total);
                var quoteRemaining = project.QuoteTotal - billedTotal;
                //Only send notification if it is first business day of month
                if (now.IsFirstBusinessDayOfMonth())
                {
                    await _emailService.InvoiceExceedsQuote(project, invoiceAmount, quoteRemaining);
                }
                return Result.Error("Project expenses exceed quote: {projectId}, invoiceAmount: {invoiceAmount}, quoteRemaining: {quoteRemaining}",
                    projectId, invoiceAmount, quoteRemaining);
            }

            if (isCloseout && totalExpenses < project.QuoteTotal)
            {
                Log.Warning("Expenses: {totalExpenses} do not match quote: ({quoteTotal}) for project: {projectId}",
                    totalExpenses, project.QuoteTotal, project.Id);
            }


            var unbilledExpenses = await _dbContext.Expenses.Where(e => e.Invoice == null && e.Approved && e.ProjectId == projectId).ToArrayAsync();

            if (!unbilledExpenses.Any() && !isCloseout)
            {
                return Result.Error("No unbilled expenses found for project: {projectId}", projectId);
            }

            var newInvoice = new Invoice
            {
                CreatedOn = now,
                ProjectId = projectId,
                // empty invoice won't be processed by SlothService
                Status = unbilledExpenses.Any() ? Invoice.Statuses.Created : Invoice.Statuses.Completed,
                Total = unbilledExpenses.Sum(x => x.Total)
            };
            newInvoice.Expenses = new List<Expense>(unbilledExpenses);
            _dbContext.Invoices.Add(newInvoice);

            await _historyService.InvoiceCreated(project.Id, newInvoice);

            var resultMessage = "Invoice Created";

            if (isCloseout)
            {
                if (unbilledExpenses.Length > 0)
                {
                    project.UpdateStatus(Project.Statuses.FinalInvoicePending);
                    await _historyService.FinalInvoicePending(project.Id, newInvoice);
                    resultMessage = "Final invoice created. Project will be marked 'Completed' upon money movement.";
                }
                else
                {
                    project.UpdateStatus(Project.Statuses.Completed);
                    await _historyService.ProjectCompleted(project.Id, project);
                    resultMessage = "Final invoice created. Project has been marked 'Completed'.";
                }

                //Email FM that PI has confirmed the closeout
                await _emailService.ProjectClosed(project, isAutoCloseout);
            }

            await _dbContext.SaveChangesAsync();

            await _emailService.InvoiceCreated(newInvoice);

            return Result.Value(newInvoice.Id, resultMessage);
        }

        public async Task<int> CreateInvoices(bool manualOverride = false)
        {
            var activeProjects = await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Where(a => a.IsActive && a.Status == Project.Statuses.Active && a.Accounts.Any())
                .ToListAsync();
            var counter = 0;
            foreach (var activeProject in activeProjects)
            {
                if (!(await CreateInvoice(activeProject.Id, manualOverride)).IsError)
                {
                    //Log something if invoice created?
                    counter++;
                }
            }

            return counter;
        }

        public async Task<List<int>> GetCreatedInvoiceIds()
        {
            //TODO: Check project status?
            return await _dbContext.Invoices.Where(a => a.Status == Invoice.Statuses.Created).Select(a => a.Id)
                .ToListAsync();
        }

        /// <summary>
        /// Automatically closeout projects that are 18 days or older since closeout was initiated.
        /// </summary>
        /// <param name="daysBeforeAutoCloseout">Negative Value</param>
        /// <returns></returns>
        public async Task<int> AutoCloseoutProjects(int daysBeforeAutoCloseout)
        {
            var dateCutoff = DateTime.UtcNow.AddDays(daysBeforeAutoCloseout);

            var count = 0;
            var projectIds = await _dbContext.Projects.Where(a => a.IsActive && a.Status == Project.Statuses.PendingCloseoutApproval && a.LastStatusUpdatedOn < dateCutoff).Select(a => a.Id).ToArrayAsync();

            foreach (var projectId in projectIds)
            {
                Log.Information($"Starting auto closeout of project: {projectId}");
                var result = await CreateInvoice(projectId, true, true);
                if (result.IsError)
                { 
                    Log.Error($"Error creating invoice for AutoCloseout. Error: {result.Message}");
                    Log.Information($"Auto closeout of project {projectId} failed.");
                }
                else
                {
                    count++;
                    Log.Information($"Auto closeout of project {projectId} succeeded.");
                }
            }
            return count;
        }
    }
}
