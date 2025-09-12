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
            var autoApprove = await _userService.HasAnyTeamRoles(TeamSlug, new[] { Role.Codes.FieldManager, Role.Codes.Supervisor });
            var allRates = await _dbContext.Rates.Where(a => a.IsActive).ToListAsync();
            foreach (var expense in expenses)
            {
                var rate = allRates.Single(a => a.Id == expense.RateId);

                expense.CreatedBy = user;
                expense.CreatedOn = DateTime.UtcNow;
                expense.ProjectId = projectId;
                expense.InvoiceId = null;
                expense.Account = rate.Account;
                expense.Price = rate.Price;
                expense.Type = rate.Type;
                expense.Total = Math.Round(rate.Price * expense.Quantity, 2, MidpointRounding.ToZero);
                expense.IsPassthrough = rate.IsPassthrough;
                if (autoApprove)
                {
                    expense.Approved = true;
                    expense.ApprovedBy = user;
                    expense.ApprovedOn = DateTime.UtcNow;
                }
            }

            _dbContext.Expenses.AddRange(expenses);

            await _historyService.ExpensesCreated(projectId, expenses);

            await _dbContext.SaveChangesAsync();

            return Ok(expenses);
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        [Route("/api/{team}/Expense/Get/{expenseId}")]
        public async Task<ActionResult> Get(int expenseId)
        {
            var user = await _userService.GetCurrentUser();
            var isFieldManager = await _userService.HasAnyTeamRoles(TeamSlug, new[] { Role.Codes.FieldManager, Role.Codes.System });

            var expense = await _dbContext.Expenses
                .Include(e => e.Project)
                .Include(e => e.Rate)
                .Include(e => e.CreatedBy)
                .Include(e => e.ApprovedBy)
                .SingleOrDefaultAsync(e => e.Id == expenseId && e.Project.Team.Slug == TeamSlug);

            if (expense == null)
            {
                return NotFound();
            }

            if (!isFieldManager)
            {
                // Check if the worker belongs to the supervisor
                var myWorkers = await _dbContext.Permissions
                    .Where(a => a.UserId == user.Id && a.Team.Slug == TeamSlug)
                    .SelectMany(a => a.Children)
                    .Select(c => c.UserId)
                    .ToListAsync();

                if (expense.CreatedById == null || !myWorkers.Contains(expense.CreatedById.Value))
                {
                    return Forbid("You can only view expenses created by your workers.");
                }
            }

            return Ok(expense);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> Edit(int projectId, [FromBody] Expense[] expenses)
        {
            //When editing, new expenses can be added, but at least one must already exist.
            var project = await _dbContext.Projects.SingleAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);
            if (project.Status != Project.Statuses.Active
                && project.Status != Project.Statuses.AwaitingCloseout
                && project.Status != Project.Statuses.PendingCloseoutApproval)
            {
                return BadRequest($"Expenses cannot be created for project with status of {project.Status}");
            }

            var user = await _userService.GetCurrentUser();
            var isFieldManager = await _userService.HasAnyTeamRoles(TeamSlug, new[] { Role.Codes.FieldManager, Role.Codes.System });

            //Only unbilled and unapproved expenses can be edited
            var expenseIds = expenses.Where(e => e.Id != 0).Select(e => e.Id).Distinct().ToArray();
            if (expenseIds.Length == 0)
            {
                return BadRequest("At least one expense must already exist to do an edit. (You can't completely remove the type (Labor, Expense, Other) without replacing that.)");
            }

            if (expenseIds.Length > 1)
            {
                return BadRequest("Only one expense can be edited at a time.");
            }

            var existingExpense = await _dbContext.Expenses.Include(e => e.Project).SingleOrDefaultAsync(e => e.Id == expenseIds[0] && e.Project.Team.Slug == TeamSlug);
            if (existingExpense == null)
            {
                return NotFound();
            }
            if (existingExpense.ProjectId != projectId)
            {
                return BadRequest("Project ID cannot be changed");
            }
            if (existingExpense.InvoiceId != null)
            {
                return BadRequest("Cannot edit an expense that has been billed.");
            }

            var allRates = await _dbContext.Rates.Where(a => a.IsActive).ToListAsync();

            if (!isFieldManager)
            {
                //Check if the worker belongs to the supervisor
                var myWorkers = await _dbContext.Permissions.Where(a => a.UserId == user.Id && a.Team.Slug == TeamSlug).Include(a => a.Children).ThenInclude(a => a.User)
                    .SelectMany(a => a.Children).Select(a => a.User).Select(a => a.Id).ToListAsync();
                if (existingExpense.CreatedById == null || !myWorkers.Contains(existingExpense.CreatedById.Value))
                {
                    return BadRequest("You can only edit expenses created by your workers.");
                }
            }

            var expense = expenses.Single(e => e.Id == existingExpense.Id); //Get the one we are editing            
            existingExpense.Activity = expense.Activity;
            existingExpense.Description = expense.Description;
            existingExpense.Type = expense.Type;
            existingExpense.Quantity = expense.Quantity;
            existingExpense.Markup = expense.Markup;
            //existingExpense.Rate = expense.Rate; Do not set this as it is a null value in the posted object, ends up deleting the expense.
            existingExpense.RateId = expense.RateId;
            existingExpense.Price = expense.Price;
            existingExpense.Total = expense.Total;
            existingExpense.CreatedOn = DateTime.UtcNow;
            existingExpense.Account = allRates.Single(a => a.Id == expense.RateId).Account;
            existingExpense.IsPassthrough = allRates.Single(a => a.Id == expense.RateId).IsPassthrough;
            if (existingExpense.Total <= 0)
            {
                return BadRequest("Maybe the expense was removed. Can't do that.");
            }

            //Get any other expenses that were added
            var newExpenses = expenses.Where(e => e.Id == 0).ToList();
            foreach (var newExpense in newExpenses)
            {
                newExpense.CreatedBy = user; //Possibly we could get the worker that created the original expense and use them here
                newExpense.CreatedOn = DateTime.UtcNow;
                newExpense.ProjectId = projectId;
                newExpense.InvoiceId = null;
                newExpense.Account = allRates.Single(a => a.Id == newExpense.RateId).Account;
                newExpense.IsPassthrough = allRates.Single(a => a.Id == newExpense.RateId).IsPassthrough;

                newExpense.Approved = true;
                newExpense.ApprovedBy = user;
                newExpense.ApprovedOn = DateTime.UtcNow;

            }

            if (newExpenses.Count > 0)
            {
                _dbContext.Expenses.AddRange(newExpenses);
                await _historyService.ExpensesCreated(projectId, newExpenses); //TODO: Change to edited?
            }
            

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

            //TODO: Do we want to prevent supervisors from deleting expenses if they don't manage the worker wo entered them? (Still allow field managers)
            //TODO: Have an extra check to make sure only unbilled expenses can be deleted

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
            var pendingExpenses = await _dbContext.Expenses.AsNoTracking()
                .Include(a => a.CreatedBy)
                .Include(a => a.ApprovedBy)
                .Include(a => a.Project)
                .Where(a => a.Project.Team.Slug == TeamSlug && !a.Approved && a.CreatedBy != null && myWorkers.Contains(a.CreatedById.Value)) //Created by can be null for auto generated expenses
                .ToArrayAsync();

            return Ok(pendingExpenses);
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> GetAllPendingExpenses()
        {
            var pendingExpenses = await _dbContext.Expenses.AsNoTracking()
                .Include(a => a.CreatedBy)
                .Include(a => a.ApprovedBy)
                .Include(a => a.Project)
                .Where(a => a.Project.Team.Slug == TeamSlug && !a.Approved && a.CreatedBy != null) //Created by can be null for auto generated expenses
                .ToArrayAsync();

            return Ok(pendingExpenses);

        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> GetApprovedExpenses()
        {

            var start = DateTime.UtcNow.AddMonths(-2).Date;

            var approvedExpenses = await _dbContext.Expenses.AsNoTracking()
                .Include(a => a.CreatedBy)
                .Include(a => a.ApprovedBy)
                .Include(a => a.Project)
                .Where(a => a.Project.Team.Slug == TeamSlug && a.Approved && a.ApprovedOn != null && a.ApprovedOn >= start)
                .ToArrayAsync();
            return Ok(approvedExpenses);
        }


        [HttpPost]
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> ApproveMyWorkerExpenses([FromBody] int[] expenseIds)
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
        public async Task<ActionResult> ApproveExpenses([FromBody] int[] expenseIds)
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
