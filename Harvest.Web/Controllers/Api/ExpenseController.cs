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

        public ActionResult Entry() {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.DepartmentAdminAccess)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> Create(int id, [FromBody]Expense[] expenses)
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
    }
}
