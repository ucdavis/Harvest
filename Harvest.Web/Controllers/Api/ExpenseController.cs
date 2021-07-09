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

        public ExpenseController(AppDbContext dbContext, IUserService userService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
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
        [Authorize(Policy = AccessCodes.DepartmentAdminAccess)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> Create(int id, [FromBody] Expense[] expenses)
        {
            // TODO: validation!
            var user = await _userService.GetCurrentUser();
            var allRates = await _dbContext.Rates.Where(a => a.IsActive).ToListAsync();
            foreach (var expense in expenses)
            {
                expense.CreatedBy = user;
                expense.CreatedOn = DateTime.UtcNow;
                expense.ProjectId = id;
                expense.InvoiceId = null;
                expense.Account = allRates.Single(a => a.Id == expense.RateId).Account;
            }

            _dbContext.Expenses.AddRange(expenses);

            await _dbContext.SaveChangesAsync();

            return Ok(expenses);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> Delete(int id) {
            var expense = await _dbContext.Expenses.FindAsync(id);

            if (expense == null) {
                return NotFound();
            }

            _dbContext.Expenses.Remove(expense);
            await _dbContext.SaveChangesAsync();

            return Ok(null);
        }

        // Get all unbilled expenses for the given project
        [HttpGet]
        public async Task<ActionResult> GetUnbilled(int id)
        {
            return Ok(await _dbContext.Expenses.Include(e => e.CreatedBy).Where(e => e.InvoiceId == null && e.ProjectId == id).ToArrayAsync());
        }

        // Get just the total of unbilled expenses for the current project
        [HttpGet]
        public async Task<ActionResult> GetUnbilledTotal(int id)
        {
            return Ok(await _dbContext.Expenses.Where(e => e.InvoiceId == null && e.ProjectId == id).SumAsync(e=>e.Total));
        }
    }
}
