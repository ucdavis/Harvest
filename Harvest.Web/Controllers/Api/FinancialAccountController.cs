using System;
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
using Serilog;

namespace Harvest.Web.Controllers.Api
{
    [Authorize]
    public class FinancialAccountController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IFinancialService _financialService;
        private IAggieEnterpriseService _aggieEnterpriseService;
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
        [Route("/api/{controller}/{action}")]
        public async Task<ActionResult> Get(string account)
        {
            AccountValidationModel validationModel;
            account = account?.ToUpper().Trim();
            if (_aeSettings.UseCoA)
            {
                //Replace Natural Account/Expenditure type if needed
                var segmentStringType = FinancialChartValidation.GetFinancialChartStringType(account);
                if(segmentStringType == FinancialChartStringType.Gl)
                {
                    var glSegments = FinancialChartValidation.GetGlSegments(account);
                    if (glSegments.Account != _aeSettings.NormalCoaNaturalAccount && glSegments.Account != _aeSettings.PassthroughCoaNaturalAccount)
                    {
                        glSegments.Account = _aeSettings.NormalCoaNaturalAccount;
                        account = glSegments.ToSegmentString();
                    }
                }
                else if (segmentStringType == FinancialChartStringType.Ppm)
                {
                    var ppmSegments = FinancialChartValidation.GetPpmSegments(account);
                    if (ppmSegments.ExpenditureType != _aeSettings.NormalCoaNaturalAccount && 
                        ppmSegments.ExpenditureType != _aeSettings.PassthroughCoaNaturalAccount && 
                        !_aeSettings.PpmSpecialNaturalAccounts.Contains(ppmSegments.ExpenditureType))
                    {
                        ppmSegments.ExpenditureType = _aeSettings.NormalCoaNaturalAccount;
                        account = ppmSegments.ToSegmentString();
                    }
                }

                try
                {
                    //Now Validate...
                    validationModel = await _aggieEnterpriseService.IsAccountValid(account);
                }
                catch (Exception ex)
                {
                    Log.Error("Error looking up account info. {account} - Exception: {ex}", account, ex);
                    return Ok(null); //This will at least display something to the user.
                }
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
                        //TODO: Use PI name instead of account manager when it is available from the API
                        Name = validationModel.CoaChartType == FinancialChartStringType.Ppm ? $"PPM: {validationModel.Description} ({validationModel.AccountManager})" : $"GL: {validationModel.Description}",
                        AccountManagerName = validationModel.AccountManager,
                        AccountManagerEmail = validationModel.AccountManagerEmail, 
                        Number = validationModel.FinancialSegmentString
                    });
                }

            }
            else
            {
                if(_aeSettings.UseCoA)
                {
                    //return a new type with a success false and the error message
                    return Ok(new { success = false, error = validationModel.Message });
                }
            }

            return Ok(null);
        }
    }
}