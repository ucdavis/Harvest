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
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers.Api
{
    [Authorize]
    public class ExpenseController : SuperController
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
            var project = await _dbContext.Projects.SingleAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);
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
            //var expense = await _dbContext.Expenses.FindAsync(expenseId);
            var expense = await _dbContext.Expenses.SingleOrDefaultAsync(a => a.Id == expenseId && a.Project.Team.Slug == TeamSlug);

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
        [Authorize(Policy = AccessCodes.InvoiceAccess)]
        //`/api/${team}/Expense/GetUnbilled/${projectId}/${shareId}`
        [Route("/api/{team}/Expense/GetUnbilled/{projectId}/{shareId?}")]
        public async Task<ActionResult> GetUnbilled(int projectId, Guid? shareId)
        {
            var query = _dbContext.Expenses
                .Include(e => e.CreatedBy)
                .Include(e => e.ApprovedBy)
                .Where(e => e.InvoiceId == null && e.ProjectId == projectId && e.Project.Team.Slug == TeamSlug);

            if (shareId.HasValue)
            {
                query = query.Where(e => e.Project.ShareId == shareId.Value);
            }

            return Ok(await query.ToArrayAsync());

            //return Ok(await _dbContext.Expenses.Include(e => e.CreatedBy).Where(e => e.InvoiceId == null && e.ProjectId == projectId && e.Project.Team.Slug == TeamSlug).ToArrayAsync());

        }

        // Get just the total of unbilled expenses for the current project
        [HttpGet]
        public async Task<ActionResult> GetUnbilledTotal(int projectId)
        {
            return Ok(await _dbContext.Expenses.Where(e => e.InvoiceId == null && e.ProjectId == projectId && e.Project.Team.Slug == TeamSlug).SumAsync(e => e.Total));
        }

        // Get just the total of unbilled expenses for the current project
        [Authorize(Policy = AccessCodes.WorkerAccess)]
        [HttpGet]
        public async Task<ActionResult> GetRecentExpensedProjects()
        {
            var user = await _userService.GetCurrentUser();

            // get projects where user has entered an expense in the last month
            var projects = await _dbContext.Projects.AsNoTracking()
                .Where(p => p.IsActive && p.Team.Slug == TeamSlug && p.Expenses.Any(e => e.CreatedById == user.Id && e.CreatedOn > DateTime.UtcNow.AddMonths(-1)))
                .OrderByDescending(a => a.CreatedOn)
                .Select(p => new { p.Id, p.Status, p.Name })
                .Take(4) // limit to 4 projects
                .ToArrayAsync();

            return Ok(projects);
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> GetMyPendingExpenses()
        {
            var user = await _userService.GetCurrentUser();
            var myWorkers = await _dbContext.Permissions.Where(a => a.UserId == user.Id && a.Team.Slug == TeamSlug).Include(a => a.Children).ThenInclude(a => a.User)
                .SelectMany(a => a.Children).Select(a => a.User).Select(a => a.Id).ToListAsync();

            //TODO: Use a projection? 
            var pendingExpenses = await _dbContext.Expenses
                .Include(a => a.CreatedBy)
                .Include(a => a.ApprovedBy)
                .Include(a => a.Project)
                .Where(a => !a.Approved && a.CreatedBy != null && myWorkers.Contains(a.CreatedById.Value)) //Created by can be null for auto generated expenses
                .ToArrayAsync();

            return Ok(pendingExpenses);
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> GetAllPendingExpenses()
        {
            var pendingExpenses = await _dbContext.Expenses
                .Include(a => a.CreatedBy)
                .Include(a => a.ApprovedBy)
                .Include(a => a.Project)
                .Where(a => !a.Approved && a.CreatedBy != null) //Created by can be null for auto generated expenses
                .ToArrayAsync();

            return Ok(pendingExpenses);

        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> ApproveMyWorkerExpense(int expenseId)
        {
            var user = await _userService.GetCurrentUser();
            var myWorkers = await _dbContext.Permissions.Where(a => a.UserId == user.Id && a.Team.Slug == TeamSlug).Include(a => a.Children).ThenInclude(a => a.User)
                .SelectMany(a => a.Children).Select(a => a.User).Select(a => a.Id).ToListAsync();

            var expense = await _dbContext.Expenses.SingleOrDefaultAsync(a => a.Id == expenseId && a.Project.Team.Slug == TeamSlug && myWorkers.Contains(a.CreatedById.Value));
            if (expense == null)
            {
                return NotFound();
            }
            if (expense.Approved)
            {
                return BadRequest("Expense is already approved");
            }
            expense.ApprovedBy = user;
            expense.ApprovedOn = DateTime.UtcNow;
            expense.Approved = true;
            //await _historyService.ExpenseApproved(expense.ProjectId, expense); //TODO: Add this
            await _dbContext.SaveChangesAsync();
            return Ok(expense);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> ApproveMyWorkerExpenses(int[] expenseIds)
        {
            var user = await _userService.GetCurrentUser();
            var myWorkers = await _dbContext.Permissions.Where(a => a.UserId == user.Id && a.Team.Slug == TeamSlug).Include(a => a.Children).ThenInclude(a => a.User)
                .SelectMany(a => a.Children).Select(a => a.User).Select(a => a.Id).ToListAsync();

            var expenses = await _dbContext.Expenses
                .Where(a => expenseIds.Contains(a.Id) && a.Project.Team.Slug == TeamSlug && myWorkers.Contains(a.CreatedById.Value) && !a.Approved)
                .ToListAsync();
            if (expenses.Count == 0)
            {
                return NotFound();
            }
            foreach (var expense in expenses)
            {
                expense.ApprovedBy = user;
                expense.ApprovedOn = DateTime.UtcNow;
                expense.Approved = true;
                //await _historyService.ExpenseApproved(expense.projectId, expenses); //TODO: Add this
            }

            await _dbContext.SaveChangesAsync();
            return Ok(expenses);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> ApproveExpense(int expenseId)
        {
            var expense = await _dbContext.Expenses.SingleOrDefaultAsync(a => a.Id == expenseId && a.Project.Team.Slug == TeamSlug);
            if (expense == null)
            {
                return NotFound();
            }
            if (expense.Approved)
            {
                return BadRequest("Expense is already approved");
            }
            var user = await _userService.GetCurrentUser();
            expense.ApprovedBy = user;
            expense.ApprovedOn = DateTime.UtcNow;
            expense.Approved = true;
            //await _historyService.ExpenseApproved(expense.ProjectId, expense); //TODO: Add this
            await _dbContext.SaveChangesAsync();
            return Ok(expense);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> ApproveExpenses(int[] expenseIds)
        {
            var expenses = await _dbContext.Expenses
                .Where(a => expenseIds.Contains(a.Id) && a.Project.Team.Slug == TeamSlug && !a.Approved)
                .ToListAsync();
            if (expenses.Count == 0)
            {
                return NotFound();
            }
            var user = await _userService.GetCurrentUser();
            foreach (var expense in expenses)
            {
                expense.ApprovedBy = user;
                expense.ApprovedOn = DateTime.UtcNow;
                expense.Approved = true;
                //await _historyService.ExpenseApproved(expense.ProjectId, expenses); //TODO: Add this
            }
            await _dbContext.SaveChangesAsync();
            return Ok(expenses);
        }

    }
}
