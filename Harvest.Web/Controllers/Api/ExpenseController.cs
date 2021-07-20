using System;
using System.Collections.Generic;
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

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class ExpenseController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IProjectHistoryService _historyService;

        public ExpenseController(AppDbContext dbContext, IUserService userService, IProjectHistoryService historyService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
            this._historyService = historyService;
        }

        public ActionResult Entry()
        {
            return View("React");
        }

        public ActionResult Unbilled(int id)
        {
            return View("React");
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.WorkerAccess)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> Create(int projectId, [FromBody] Expense[] expenses)
        {
            // TODO: validation!
            var user = await _userService.GetCurrentUser();
            var allRates = await _dbContext.Rates.Where(a => a.IsActive).ToListAsync();
            foreach (var expense in expenses)
            {
                expense.CreatedBy = user;
                expense.CreatedOn = DateTime.UtcNow;
                expense.ProjectId = projectId;
                expense.InvoiceId = null;
                expense.Account = allRates.Single(a => a.Id == expense.RateId).Account;
            }

            await _historyService.AddProjectHistory(projectId, $"{nameof(ExpenseController)}.{nameof(Create)}", "Expenses Created", expenses);

            _dbContext.Expenses.AddRange(expenses);

            await _dbContext.SaveChangesAsync();

            return Ok(expenses);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> Delete(int expenseId) {
            var expense = await _dbContext.Expenses.FindAsync(expenseId);

            if (expense == null) {
                return NotFound();
            }

            await _historyService.AddProjectHistory(expense.ProjectId, $"{nameof(ExpenseController)}.{nameof(Delete)}", "Expense Deleted", expense);

            _dbContext.Expenses.Remove(expense);
            await _dbContext.SaveChangesAsync();

            return Ok(null);
        }

        // Get all unbilled expenses for the given project
        [HttpGet]
        public async Task<ActionResult> GetUnbilled(int projectId)
        {
            return Ok(await _dbContext.Expenses.Include(e => e.CreatedBy).Where(e => e.InvoiceId == null && e.ProjectId == projectId).ToArrayAsync());
        }

        // Get just the total of unbilled expenses for the current project
        [HttpGet]
        public async Task<ActionResult> GetUnbilledTotal(int projectId)
        {
            return Ok(await _dbContext.Expenses.Where(e => e.InvoiceId == null && e.ProjectId == projectId).SumAsync(e=>e.Total));
        }
    }
}
