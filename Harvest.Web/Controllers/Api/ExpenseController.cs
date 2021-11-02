using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers.Api
{
    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IProjectHistoryService _historyService;
        private readonly IExpenseService _expenseService;

        public ExpenseController(AppDbContext dbContext, IUserService userService, IProjectHistoryService historyService, IExpenseService expenseService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _historyService = historyService;
            _expenseService = expenseService;
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.WorkerAccess)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> Create(int projectId, [FromBody] Expense[] expenses)
        {
            var project = await _dbContext.Projects.SingleAsync(p => p.Id == projectId);
            if (project.Status != Project.Statuses.Active
                && project.Status != Project.Statuses.AwaitingCloseout
                && project.Status != Project.Statuses.PendingCloseoutApproval)
            {
                return BadRequest($"Expenses cannot be created for project with status of {project.Status}");
            }

            var user = await _userService.GetCurrentUser();
            var allRates = await _dbContext.Rates.Where(a => a.IsActive).ToListAsync();
            foreach (var expense in expenses)
            {
                expense.CreatedBy = user;
                expense.CreatedOn = DateTime.UtcNow;
                expense.ProjectId = projectId;
                expense.InvoiceId = null;
                expense.Account = allRates.Single(a => a.Id == expense.RateId).Account;
                expense.IsPassthrough = allRates.Single(a => a.Id == expense.RateId).IsPassthrough;
            }

            _dbContext.Expenses.AddRange(expenses);

            await _historyService.ExpensesCreated(projectId, expenses);

            await _dbContext.SaveChangesAsync();

            return Ok(expenses);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> Delete(int expenseId)
        {
            var expense = await _dbContext.Expenses.FindAsync(expenseId);

            if (expense == null)
            {
                return NotFound();
            }

            _dbContext.Expenses.Remove(expense);
            await _historyService.ExpenseDeleted(expense.ProjectId, expense);
            await _dbContext.SaveChangesAsync();

            return Ok(null);
        }

        // Get all unbilled expenses for the given project
        [HttpGet]
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public async Task<ActionResult> GetUnbilled(int projectId)
        {
            return Ok(await _dbContext.Expenses.Include(e => e.CreatedBy).Where(e => e.InvoiceId == null && e.ProjectId == projectId).ToArrayAsync());
        }

        // Get just the total of unbilled expenses for the current project
        [HttpGet]
        public async Task<ActionResult> GetUnbilledTotal(int projectId)
        {
            return Ok(await _dbContext.Expenses.Where(e => e.InvoiceId == null && e.ProjectId == projectId).SumAsync(e => e.Total));
        }

        // Get just the total of unbilled expenses for the current project
        [Authorize(Policy = AccessCodes.WorkerAccess)]
        [HttpGet]
        public async Task<ActionResult> GetRecentExpensedProjects()
        {
            var user = await _userService.GetCurrentUser();

            // get projects where user has entered an expense in the last month
            var projects = await _dbContext.Projects.AsNoTracking()
                .Where(p => p.IsActive && p.Expenses.Any(e => e.CreatedById == user.Id && e.CreatedOn > DateTime.UtcNow.AddMonths(-1)))
                .Select(p => new { p.Id, p.Status, p.Name })
                .Take(4) // limit to 4 projects
                .ToArrayAsync();

            return Ok(projects);
        }
    }
}
