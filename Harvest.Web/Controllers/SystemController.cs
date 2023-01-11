using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Models;
using Harvest.Web.Models;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Harvest.Core.Models.Settings;
using Microsoft.Extensions.Options;
using AggieEnterpriseApi.Validation;
using static Harvest.Core.Domain.Invoice;
using Harvest.Core.Domain;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.SystemAccess)]
    public class SystemController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IIdentityService _identityService;
        public const string IamIdClaimType = "ucdPersonIAMID";
        private readonly AggieEnterpriseOptions _aeSettings;

        public SystemController(AppDbContext dbContext, IIdentityService identityService, IOptions<AggieEnterpriseOptions> options)
        {
            _dbContext = dbContext;
            _identityService = identityService;
            _aeSettings = options.Value;
        }

        [HttpGet]
        public IActionResult Emulate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Emulate(EmulateUserViewModel model)
        {
            Log.Information($"Emulation attempted for {model.Search} by {User.Identity.Name}");
            var lookupVal = model.Search.Trim();

            var user = await  _dbContext.Users.SingleOrDefaultAsync(u => u.Email == lookupVal || u.Kerberos == lookupVal);
            if (user == null)
            {
                // not found in db, look up user in IAM
                user = model.Search.Contains("@")
                    ? await _identityService.GetByEmail(model.Search)
                    : await _identityService.GetByKerberos(model.Search);

                if (user != null) {
                    // user found in IAM but not in our db, add them and save before we continue
                    _dbContext.Users.Add(user);
                    await _dbContext.SaveChangesAsync();
                }
            }

            if (user == null)
            {
                // user not found in db or IAM
                return View(model);
            }
            
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Kerberos),
                new Claim(ClaimTypes.Name, user.Kerberos),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim("name", user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(IamIdClaimType, user.Iam),
            }, CookieAuthenticationDefaults.AuthenticationScheme);

            // kill old login
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // create new login
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> UpdatePendingExpenses()
        {

            var pendingExpenses = await _dbContext.Expenses.Where(a => a.Invoice == null || a.Invoice.Status == Statuses.Created).Include(a => a.Rate).Include(a => a.Invoice).Select(Expense.ExpenseProjectionToUnprocessedExpensesModel()).ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePendingExpenses(bool update)
        {
            if (!_aeSettings.UseCoA)
            {
                Log.Information("UpdatePendingExpenses called but CoA is not enabled");
                return Content("Not using CoA yet");
            }
            var rates = await _dbContext.Rates.Where(a => a.IsActive).ToListAsync();
            foreach (var rate in rates)
            {
                if (FinancialChartValidation.GetFinancialChartStringType(rate.Account) == FinancialChartStringType.Invalid)
                {
                    Log.Information($"At least one active rate has not been converted to a CoA yet. Can't continue. Rate: {rate.Description}");
                    return Content($"At least one active rate has not been converted to a CoA yet. Can't continue. Rate: {rate.Description}");
                }
            }

            var pendingExpenses = await _dbContext.Expenses.Where(a => a.Invoice == null || a.Invoice.Status == Statuses.Created).ToListAsync();
            var pendingExpenseCount = pendingExpenses.Count;
            var updatedExpenseCount = 0;
            foreach (var expense in pendingExpenses)
            {
                var rate = rates.FirstOrDefault(a => a.Id == expense.RateId);
                if (rate == null)
                {
                    Log.Information($"Expense {expense.Id} has no active rate. Can't continue.");
                    return Content($"Expense {expense.Id} has no active rate. Can't continue.");
                }
            }

            foreach (var expense in pendingExpenses)
            {
                var rate = rates.FirstOrDefault(a => a.Id == expense.RateId);
                if (expense.Account != rate.Account)
                {
                    Log.Information("Updating expense {expenseId} from {oldAccount} to {newAccount}", expense.Id, expense.Account, rate.Account);
                    expense.Account = rate.Account;
                    _dbContext.Expenses.Update(expense);
                    updatedExpenseCount++;
                }
            }

            await _dbContext.SaveChangesAsync();

            Log.Information("Updated {updatedExpenseCount} of {pendingExpenseCount} pending expenses", updatedExpenseCount, pendingExpenseCount);

            return Content($"{updatedExpenseCount} of {pendingExpenseCount} expenses updated");
        }

    }   
}
