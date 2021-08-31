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
        Task CreateMonthlyAcreageExpense(Project project);
        Task CreateAcreageExpense(int projectId, decimal amount);
    }

    public class ExpenseService : IExpenseService
    {
        private readonly AppDbContext _dbContext;
        private readonly IProjectHistoryService _historyService;
        private readonly IUserService _userService;

        public ExpenseService(AppDbContext dbContext, IProjectHistoryService historyService, IUserService userService)
        {
            _dbContext = dbContext;
            _historyService = historyService;
            _userService = userService;
        }

        public async Task CreateAcreageExpense(int projectId, decimal amount)
        {
            var project = await _dbContext.Projects
                .Include(p => p.AcreageRate)
                .SingleOrDefaultAsync(p => p.Id == projectId);

            var expense = await CreateExpense(project, amount);

            await _dbContext.Expenses.AddAsync(expense);
            await _historyService.AcreageExpenseCreated(project.Id, expense);
            await _dbContext.SaveChangesAsync();
        }

        public async Task CreateMonthlyAcreageExpense(Project project)
        {
            // Don't want to continue running if there are no acres
            if (project.Acres <= 0)
            {
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

            var expense = await CreateExpense(project, amountToCharge);

            await _dbContext.Expenses.AddAsync(expense);
            await _historyService.AcreageExpenseCreated(project.Id, expense);
            await _dbContext.SaveChangesAsync();

        }

        private async Task<Expense> CreateExpense(Project project, decimal amountToCharge)
        {
            var user = await _userService.GetCurrentUser();

            var expense = new Expense
            {
                Type = project.AcreageRate.Type,
                Description = project.AcreageRate.Description,
                Price = project.AcreageRate.Price / 12, //This can be more than 2 decimals
                Quantity = (decimal) project.Acres,
                Total = amountToCharge,
                ProjectId = project.Id,
                RateId = project.AcreageRate.Id,
                InvoiceId = null,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = user,
                Account = project.AcreageRate.Account,
            };
            return expense;
        }
    }
}
