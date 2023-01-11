using System.Threading.Tasks;
using AggieEnterpriseApi.Validation;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models.FinancialAccountModels;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Harvest.Web.Controllers.Api
{
    [Authorize]
    public class FinancialAccountController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IFinancialService _financialService;
        private readonly IAggieEnterpriseService _aggieEnterpriseService;
        private readonly AggieEnterpriseOptions _aeSettings;

        public FinancialAccountController(AppDbContext dbContext, IFinancialService financialService, IAggieEnterpriseService aggieEnterpriseService, IOptions<AggieEnterpriseOptions> options)
        {
            _dbContext = dbContext;
            _financialService = financialService;
            _aggieEnterpriseService = aggieEnterpriseService;
            _aeSettings = options.Value;
        }

        // Get info on the project as well as current proposed quote
        [HttpGet]
        public async Task<ActionResult> Get(string account)
        {
            //TODO: Call AE validation/lookup.
            AccountValidationModel validationModel;
            account = account?.ToUpper().Trim();
            if (_aeSettings.UseCoA)
            {
                validationModel = await _aggieEnterpriseService.IsAccountValid(account);
            }
            else
            {
                validationModel = await _financialService.IsValid(account);
            }
            if (validationModel.IsValid)
            {
                if(!_aeSettings.UseCoA)
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
                else
                {
                    return Ok(new Account
                    {
                        Name = validationModel.CoaChartType == FinancialChartStringType.Ppm ? "PPM Account" : "GL Account",
                        //AccountManagerName = validationModel.KfsAccount.AccountManager?.Name,
                        //AccountManagerEmail = validationModel.KfsAccount.AccountManager?.EmailAddress, //TODO: Use AE endpoint to get info
                        Number = validationModel.FinancialSegmentString
                    });
                }

            }

            return Ok(null);
        }
    }
}