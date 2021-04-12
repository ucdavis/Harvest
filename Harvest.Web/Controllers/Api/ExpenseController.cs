using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class ExpenseController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public ExpenseController(AppDbContext dbContext, IUserService userService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.DepartmentAdminAccess)]
        public async Task<ActionResult> Create(int projectId, List<Expense> expenses)
        {
            // TODO: validation!
            var user = await _userService.GetCurrentUser();

            foreach (var expense in expenses)
            {
                expense.CreatedBy = user;
                expense.CreatedOn = DateTime.UtcNow;
                expense.InvoiceId = null;
            }

            _dbContext.Expenses.AddRange(expenses);

            await _dbContext.SaveChangesAsync();

            return Ok(expenses);
        }
    }
}
