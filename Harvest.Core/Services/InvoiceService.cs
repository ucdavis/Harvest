﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Harvest.Core.Services
{
    public interface IInvoiceService
    {
        Task<bool> CreateInvoice(int id);
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
        public async Task<bool> CreateInvoice(int id)
        {
            //Look for an active project
            var project = await _dbContext.Projects.Include(a => a.AcreageRate)
                .Where(a => a.IsActive && a.Status == Project.Statuses.Active && a.Id == id).SingleOrDefaultAsync();
            if (project == null)
            {
                return false;
            }
            //Check to see if there is an invoice within the last 28 days
            if (await _dbContext.Invoices.AnyAsync(a => a.ProjectId == id && a.CreatedOn >= DateTime.UtcNow.AddDays(-28)))
            {
                //Already invoice within the last 28 days.
                return false;
            }

            var now = DateTime.UtcNow;
            if (project.Start.AddMonths(1) > now) //Start doing invoices 1 month after the project starts
            {
                //Project hasn't started yet. (Is Start UTC? if not, it should be)
                return false;
            }

            //TODO: Review this later
            if (project.End < now)
            {
                //Ok, so the project has ended, we should probably create the last invoice, and close it out. Setting the project to completed.
                project.Status = Project.Statuses.Completed;
            }

            //TODO: Create the acreage expense with correct amount 

            await CreateMonthlyAcreageExpense(project);

            var unbilledExpenses = await _dbContext.Expenses.Where(e => e.Invoice == null && e.ProjectId == id).ToArrayAsync();

            //Shouldn't happen once we create the acreage expense 
            if (unbilledExpenses.Length == 0)
            {
                return false;
            }

            var newInvoice = new Invoice { CreatedOn = DateTime.UtcNow, ProjectId = id, Status = Invoice.Statuses.Created, Total = unbilledExpenses.Sum(x => x.Total) };

            newInvoice.Expenses = new System.Collections.Generic.List<Expense>(unbilledExpenses);

            _dbContext.Invoices.Add(newInvoice);

            await _historyService.AddProjectHistory(project, nameof(CreateInvoice), "Invoice created", newInvoice);

            await _dbContext.SaveChangesAsync(); //Do one save outside of this?
            return true;

        }

        private async Task CreateMonthlyAcreageExpense(Project project)
        {
            var amountToCharge = Math.Round((decimal)project.Acres * (project.AcreageRate.Price / 12), 2);

            //Check for an unbilled acreage expense
            if (await _dbContext.Expenses.AnyAsync(a =>
                a.ProjectId == project.Id && a.InvoiceId == null && a.Rate.Type == Rate.Types.Acreage))
            {
                Log.Information("Project {project.Id} Unbilled acreage expense already found. Skipping.", project.Id);
                return;
            }

            var expense = new Expense
            {
                Type        = project.AcreageRate.Type,
                Description = project.AcreageRate.Description,
                Price       = project.AcreageRate.Price / 12, //This can be more than 2 decimals
                Quantity    = (decimal)project.Acres,
                Total       = amountToCharge,
                ProjectId   = project.Id,
                RateId      = project.AcreageRate.Id,
                InvoiceId   = null,
                CreatedOn   = DateTime.UtcNow,
                CreatedById = null,
                Account     = project.AcreageRate.Account,
            };

            await _dbContext.Expenses.AddAsync(expense);
            await _historyService.AddProjectHistory(project, nameof(CreateMonthlyAcreageExpense), "Monthly acreage expense created", expense);
            await _dbContext.SaveChangesAsync();

        }

        public async Task<int> CreateInvoices()
        {
            var activeProjects = await _dbContext.Projects.Where(a => a.IsActive && a.Status == Project.Statuses.Active)
                .ToListAsync();
            var counter = 0;
            foreach (var activeProject in activeProjects)
            {
                if (await CreateInvoice(activeProject.Id))
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
