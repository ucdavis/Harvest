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
    public interface IExpenseService
    {
        Task CreateYearlyAcreageExpense(Project project);
        Task CreateChangeRequestAdjustment(Project project, decimal extraAcres);
    }

    public class ExpenseService : IExpenseService
    {
        private readonly AppDbContext _dbContext;
        private readonly IProjectHistoryService _historyService;

        public ExpenseService(AppDbContext dbContext, IProjectHistoryService historyService)
        {
            _dbContext = dbContext;
            _historyService = historyService;
        }


        public async Task CreateYearlyAcreageExpense(Project project)
        {
            if (project.Acres <= 0)
            {
                return;
            }

            //Look for an acreage expense in the last year or so.
            var compareDate = DateTime.UtcNow.Date.AddYears(-1).AddDays(-1);
            if (await _dbContext.Expenses.AnyAsync(a => a.ProjectId == project.Id && a.CreatedOn >= compareDate && a.Rate.Type == Rate.Types.Acreage))
            {
                Log.Information("Project {projectId} found an acreage expense within the last year, skipping", project.Id);
                return;
            }

            var amountToCharge = Math.Round((decimal)project.Acres * project.AcreageRate.Price, 2, MidpointRounding.ToZero); //TODO: How to round this?
            if (amountToCharge < 0.01m)
            {
                Log.Error("Project {projectId} would have an acreage amount less than 0.01. Skipping.", project.Id);
                return;
            }

            var expense = CreateExpense(project, amountToCharge, (decimal)project.Acres);

            await _dbContext.Expenses.AddAsync(expense);
            await _historyService.AcreageExpenseCreated(project.Id, expense);
            await _dbContext.SaveChangesAsync();
        }

        public async Task CreateChangeRequestAdjustment(Project project, decimal extraAcres)
        {
            if (extraAcres <= 0)
            {
                return;
            }

            if (project.AcreageRate == null) //I guess this is ok. Might want to call it later in the change request when it should have it?
            {
                var project1 = project;
                project = await _dbContext.Projects.Include(a => a.AcreageRate).SingleAsync(a => a.Id == project1.Id);
            }

            var amountToCharge = Math.Round(extraAcres * project.AcreageRate.Price, 2, MidpointRounding.ToZero); //TODO: How to round this?
            if (amountToCharge < 0.01m)
            {
                Log.Error("Adjustment Project {projectId} would have an acreage amount less than 0.01. Skipping.", project.Id);
                return;
            }

            var expense = CreateExpense(project, amountToCharge, extraAcres);

            await _dbContext.Expenses.AddAsync(expense);
            await _historyService.AcreageExpenseCreated(project.Id, expense);
            await _dbContext.SaveChangesAsync();
        }


        private Expense CreateExpense(Project project, decimal amountToCharge, decimal acres)
        {
            //If we populate the user, then this will have the PI's user because it is done in the request approval for change requests.


            var expense = new Expense
            {
                Type = project.AcreageRate.Type,
                Description = project.AcreageRate.Description,
                Price = project.AcreageRate.Price,
                Quantity = acres,
                Total = Math.Round(amountToCharge,2, MidpointRounding.ToZero),
                ProjectId = project.Id,
                RateId = project.AcreageRate.Id,
                InvoiceId = null,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = null,
                Account = project.AcreageRate.Account,
            };
            return expense;
        }
    }
}
