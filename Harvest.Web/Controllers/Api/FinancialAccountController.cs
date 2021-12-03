using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harvest.Web.Controllers.Api
{
    [Authorize]
    public class FinancialAccountController : SuperController
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
                validationModel.KfsAccount.ObjectCode = null; // Don't allow object code on PI lookup
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