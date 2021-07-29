using System;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Web.Models;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class FinancialAccountController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IFinancialService _financialService;

        public FinancialAccountController(AppDbContext dbContext, IFinancialService financialService)
        {
            this._dbContext = dbContext;
            this._financialService = financialService;
        }

        // Get info on the project as well as current proposed quote
        [HttpGet]
        public async Task<ActionResult> Get(string account)
        {
            var validationModel = await _financialService.IsValid(account);
            if (validationModel.IsValid)
            {
                // want to grab the account info and also fiscal officer
                return Ok(new Account
                {
                    Name = validationModel.KfsAccount.AccountName,
                    AccountManagerName = validationModel.KfsAccount.AccountManager?.Name,
                    AccountManagerEmail = validationModel.KfsAccount.AccountManager?.EmailAddress,
                    Number = validationModel.KfsAccount.ToString()
                });
            }

            return Ok(null);
        }
    }
}