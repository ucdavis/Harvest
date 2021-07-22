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
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Harvest.Core.Services
{
    public interface IInvoiceService
    {
        Task<Result<int>> CreateInvoice(int projectId, bool manualOverride = false);
        Task<int> CreateInvoices();
        Task<List<int>> GetCreatedInvoiceIds();
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _dbContext;
        private readonly IProjectHistoryService _historyService;

        public InvoiceService(AppDbContext dbContext, IProjectHistoryService historyService)
        {
            _dbContext = dbContext;
            _historyService = historyService;
        }
        public async Task<Result<int>> CreateInvoice(int projectId, bool manualOverride = false)
        {
            var now = DateTime.UtcNow;

            //Look for an active project
            var project = await _dbContext.Projects.Include(a => a.AcreageRate)
                .Where(a => a.IsActive && a.Status == Project.Statuses.Active && a.Id == projectId).SingleOrDefaultAsync();
            if (project == null)
            {
                return Result.Error("No active project found for given projectId");
            }

            if (!manualOverride)
            {
                //Make sure we are running on a business day
                var day = now.ToPacificTime().DayOfWeek;
                if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday)
                {
                    return Result.Error("Invoices can only be created Monday through Friday");
                }

                //Check to see if there is an invoice within current month
                if (await _dbContext.Invoices.AnyAsync(a => a.ProjectId == projectId && a.CreatedOn.Year == DateTime.UtcNow.Year && a.CreatedOn.Month == DateTime.UtcNow.Month))
                {
                    return Result.Error("An invoice already exists for current month");
                }

                if (project.Start.AddMonths(1) > now) //Start doing invoices 1 month after the project starts
                {
                    return Result.Error("Project has not yet started");
                }

            }

            //TODO: Review this later
            if (project.End < now)
            {
                //Ok, so the project has ended, we should probably create the last invoice, and close it out. Setting the project to completed.
                project.Status = Project.Statuses.Completed;
            }

            //Acreage fees are ignored for manually created invoices
            //TODO: Create the acreage expense with correct amount 
            await CreateMonthlyAcreageExpense(project);

            var unbilledExpenses = await _dbContext.Expenses.Where(e => e.Invoice == null && e.Approved && e.ProjectId == projectId).ToArrayAsync();

            //Shouldn't happen once we create the acreage expense 
            if (unbilledExpenses.Length == 0)
            {
                return Result.Error("No existing unbilled expenses found");
            }

            var newInvoice = new Invoice { CreatedOn = DateTime.UtcNow, ProjectId = projectId, Status = Invoice.Statuses.Created, Total = unbilledExpenses.Sum(x => x.Total) };

            newInvoice.Expenses = new System.Collections.Generic.List<Expense>(unbilledExpenses);

            _dbContext.Invoices.Add(newInvoice);

            await _historyService.InvoiceCreated(project.Id, newInvoice);
            if (project.Status == Project.Statuses.Completed)
            {
                await _historyService.ProjectCompleted(project.Id, project);
            }

            await _dbContext.SaveChangesAsync(); //Do one save outside of this?
            return Result.Value(newInvoice.Id);

        }

        private async Task CreateMonthlyAcreageExpense(Project project)
        {
            // Don't want to continue running if there are no acres
            if (project.Acres == 0 ) {
                return;
            }

            var amountToCharge = Math.Round((decimal)project.Acres * (project.AcreageRate.Price / 12), 2);

            //Check for an unbilled acreage expense
            if (await _dbContext.Expenses.AnyAsync(a =>
                a.ProjectId == project.Id && a.InvoiceId == null && a.Rate.Type == Rate.Types.Acreage))
            {
                Log.Information("Project {projectId} Unbilled acreage expense already found. Skipping.", project.Id);
                return;
            }

            var expense = new Expense
            {
                Type = project.AcreageRate.Type,
                Description = project.AcreageRate.Description,
                Price = project.AcreageRate.Price / 12, //This can be more than 2 decimals
                Quantity = (decimal)project.Acres,
                Total = amountToCharge,
                ProjectId = project.Id,
                RateId = project.AcreageRate.Id,
                InvoiceId = null,
                CreatedOn = DateTime.UtcNow,
                CreatedById = null,
                Account = project.AcreageRate.Account,
            };

            await _dbContext.Expenses.AddAsync(expense);
            await _historyService.AcreageExpenseCreated(project.Id, expense);
            await _dbContext.SaveChangesAsync();

        }

        public async Task<int> CreateInvoices()
        {
            var activeProjects = await _dbContext.Projects.Where(a => a.IsActive && a.Status == Project.Statuses.Active)
                .ToListAsync();
            var counter = 0;
            foreach (var activeProject in activeProjects)
            {
                if (!(await CreateInvoice(activeProject.Id)).IsError)
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
