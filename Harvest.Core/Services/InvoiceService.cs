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
        Task<Result<int>> CreateInvoice(int projectId, bool isCloseout = false);
        Task<int> CreateInvoices(bool manualOverride = false);
        Task<List<int>> GetCreatedInvoiceIds();
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _dbContext;
        private readonly IProjectHistoryService _historyService;
        private readonly IEmailService _emailService;
        private readonly IExpenseService _expenseService;
        private readonly DevSettings _devSettings;

        public InvoiceService(AppDbContext dbContext, IProjectHistoryService historyService,
            IEmailService emailService, IExpenseService expenseService, IOptions<DevSettings> devSettings)
        {
            _dbContext = dbContext;
            _historyService = historyService;
            _emailService = emailService;
            _expenseService = expenseService;
            _devSettings = devSettings.Value;
        }
        public async Task<Result<int>> CreateInvoice(int projectId, bool isCloseout = false)
        {
            var now = DateTime.UtcNow;

            //Look for an active project
            var project = await _dbContext.Projects.Include(a => a.AcreageRate)
                .Where(a =>
                    a.IsActive &&
                    (a.Status == Project.Statuses.Active ||
                        (isCloseout && a.Status == Project.Statuses.AwaitingCloseout)) &&
                    a.Id == projectId).SingleOrDefaultAsync();
            if (project == null)
            {
                return Result.Error("No active project found for given projectId: {projectId}", projectId);
            }

            if (!isCloseout && !_devSettings.NightlyInvoices)
            {
                if (!now.IsBusinessDay())
                {
                    return Result.Error("Invoices can only be created Monday through Friday: {projectId}", projectId);
                }

                //Check to see if there is an invoice within current month
                if (await _dbContext.Invoices.AnyAsync(a => a.ProjectId == projectId && a.CreatedOn.Year == DateTime.UtcNow.Year && a.CreatedOn.Month == DateTime.UtcNow.Month))
                {
                    return Result.Error("An invoice already exists for current month: {projectId}", projectId);
                }

                if (project.Start.AddMonths(1) > now) //Start doing invoices 1 month after the project starts
                {
                    return Result.Error("Project has not yet started: {projectId}", projectId);
                }

                //Don't create an invoice for project past its end date, and just mark it as AwaitingCloseout
                if (project.End < now)
                {
                    project.Status = Project.Statuses.AwaitingCloseout;
                    await _dbContext.SaveChangesAsync();
                    return Result.Error("Can't create invoice for project past its end date: {projectId}", projectId);
                }

                //Acreage fees are ignored for manually created invoices
                await _expenseService.CreateMonthlyAcreageExpense(project);
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

            if (totalExpenses < project.QuoteTotal)
            {
                Log.Warning("Expenses: {totalExpenses} do not match quote: ({quoteTotal}) for project: {projectId}",
                    totalExpenses, project.QuoteTotal, project.Id);
            }


            var unbilledExpenses = await _dbContext.Expenses.Where(e => e.Invoice == null && e.Approved && e.ProjectId == projectId).ToArrayAsync();

            if (unbilledExpenses.Length == 0 && !isCloseout)
            {
                return Result.Error("No unbilled expenses found for project: {projectId}", projectId);
            }

            var newInvoice = new Invoice
            {
                CreatedOn = DateTime.UtcNow,
                ProjectId = projectId,
                // empty invoice won't be processed by SlothService
                Status = unbilledExpenses.Length > 0 ? Invoice.Statuses.Created : Invoice.Statuses.Completed,
                Total = unbilledExpenses.Sum(x => x.Total)
            };
            newInvoice.Expenses = new List<Expense>(unbilledExpenses);
            _dbContext.Invoices.Add(newInvoice);

            await _historyService.InvoiceCreated(project.Id, newInvoice);

            var resultMessage = "Invlice Created";

            if (isCloseout)
            {
                if (unbilledExpenses.Length > 0)
                {
                    project.Status = Project.Statuses.FinalInvoicePending;
                    await _historyService.FinalInvoicePending(project.Id, newInvoice);
                    resultMessage = "Final invoice created. Project will be marked 'Completed' upon money movement.";
                }
                else
                {
                    project.Status = Project.Statuses.Completed;
                    await _historyService.ProjectCompleted(project.Id, project);
                    resultMessage = "Final invoice created. Project has been marked 'Completed'.";
                }
            }

            await _dbContext.SaveChangesAsync();
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
    }
}
