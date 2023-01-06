using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Models;
using Harvest.Core.Models.FinancialAccountModels;
using Harvest.Core.Services;
using Harvest.Web.Models.RateModels;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Harvest.Core.Models.Settings;
using Microsoft.Extensions.Options;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.WorkerAccess)]
    public class RateController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IFinancialService _financialService;
        private readonly IAggieEnterpriseService _aggieEnterpriseService;
        private readonly AggieEnterpriseOptions _aeSettings;

        public RateController(AppDbContext dbContext, IUserService userService, IFinancialService financialService, IAggieEnterpriseService aggieEnterpriseService, IOptions<AggieEnterpriseOptions> options)
        {
            _dbContext = dbContext;
            _userService = userService;
            _financialService = financialService;
            _aggieEnterpriseService = aggieEnterpriseService;
            _aeSettings = options.Value;
        }
        // GET: RateController
        public async Task<ActionResult> Index()
        {
            var rates = await _dbContext.Rates.Where(a => a.IsActive).ToListAsync();
            ViewBag.AllowEdit = await _userService.HasAnyRoles(new string[] { Role.Codes.System, Role.Codes.FieldManager });
            return View(rates);
        }

        // Returns the Active rates via API
        // GET: RateController/Active
        public async Task<ActionResult> Active()
        {
            var rates = await _dbContext.Rates.Where(a => a.IsActive).OrderBy(a => a.Description).Select(r => new { r.Price, r.Unit, r.Type, r.Description, r.Id, r.IsPassthrough }).ToArrayAsync();
            return Ok(rates);
        }

        // GET: RateController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var rate = await _dbContext.Rates
                .Include(a => a.UpdatedBy)
                .Include(a => a.CreatedBy)
                .SingleAsync(a => a.Id == id);
            var model = new RateDetailsModel { Rate = rate };
            if (_aeSettings.UseCoA)
            {
                model.AccountValidation = await _aggieEnterpriseService.IsAccountValid(rate.Account, validateRate: true);
            }
            else
            {
                model.AccountValidation = await _financialService.IsValid(model.Rate.Account);
            }
            return View(model);
        }

        // GET: RateController/Create
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public ActionResult Create()
        {
            var model = new RateEditModel { Rate = new Rate(), TypeList = Rate.Types.TypeList, UseCoA = _aeSettings.UseCoA };
            if (!_aeSettings.UseCoA)
            {
                model.Rate.Account = "3-";
            }
            
            return View(model);
        }

        // POST: RateController/Create
        [HttpPost]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Create(RateEditModel model)
        {
            model.TypeList = Rate.Types.TypeList; //Set it here in case the model isn't valid
            model.UseCoA = _aeSettings.UseCoA;
            if (!ModelState.IsValid)
            {
                ErrorMessage = "There are validation errors, please correct them and try again.";
                return View(model);
            }

            model.Rate.Account = model.Rate.Account?.ToUpper().Trim();

            AccountValidationModel accountValidation = null;
            if (_aeSettings.UseCoA)
            {
                accountValidation = await _aggieEnterpriseService.IsAccountValid(model.Rate.Account, validateRate: true);
            }
            else
            {
                accountValidation = await _financialService.IsValid(model.Rate.Account);
            }

            if (!accountValidation.IsValid)
            {
                ModelState.AddModelError("Rate.Account", $"Field: {accountValidation.Field} is not valid: {accountValidation.Message}");
            }
            else
            {
                if (!_aeSettings.UseCoA && string.IsNullOrWhiteSpace(accountValidation.KfsAccount.ObjectCode))
                {
                    ModelState.AddModelError("Rate.Account", $"Object Code is missing from the account.");
                }
                //TODO: Look at the segments to check this in the COA?
            }

            if (accountValidation.Warnings.Any())
            {
                ErrorMessage = "This COA has associated warnings, please select details to review them.";
            }

            

            if (model.Rate.IsPassthrough && model.Rate.Type != Rate.Types.Other)
            {
                ModelState.AddModelError("Rate.IsPassthrough", errorMessage: "Passthrough can only be checked for Other types.");
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "There are validation errors, please correct them and try again.";
                return View(model);
            }

            var createTime = DateTime.UtcNow;
            var user = await _userService.GetCurrentUser();

            var rateToCreate = new Rate();
            UpdateCommonValues(model, rateToCreate, accountValidation, user);
            rateToCreate.IsActive  = true;
            rateToCreate.CreatedBy = user;
            rateToCreate.CreatedOn = rateToCreate.UpdatedOn;


            try
            {
                await _dbContext.Rates.AddAsync(rateToCreate);
                await _dbContext.SaveChangesAsync();
                if (_aeSettings.UseCoA)
                {
                    if(rateToCreate.IsPassthrough && accountValidation.CoaChartType == AggieEnterpriseApi.Validation.FinancialChartStringType.Gl && accountValidation.GlSegments.Account != "775001")
                    {
                        Message = "Rate Created -- WARNING! Passthrough natural accounts should be 775001";
                    }
                    else 
                    { 
                        Message = "Rate Created";
                    }
                }
                else
                {
                    if (rateToCreate.IsPassthrough && accountValidation.KfsAccount.ObjectCode != "80RS")
                    {
                        Message = "Rate Created -- WARNING! Passthrough object code is not 80RS";
                    }
                    else
                    {
                        Message = "Rate Created";
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ErrorMessage = "There was an error trying to create this rate.";
                return View(model);
            }
        }

        // GET: RateController/Edit/5
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Edit(int id)
        {
            var rate = await _dbContext.Rates.SingleAsync(a => a.Id == id);
            var model = new RateEditModel { Rate = rate, TypeList = Rate.Types.TypeList };
            model.UseCoA = _aeSettings.UseCoA;

            return View(model);
        }

        // POST: RateController/Edit/5
        [HttpPost]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Edit(int id, RateEditModel model)
        {
            model.Rate.Id = id;
            model.TypeList = Rate.Types.TypeList; //Set it here in case the model isn't valid
            model.UseCoA = _aeSettings.UseCoA;
            model.Rate.Account = model.Rate.Account?.ToUpper().Trim();
            
            if (!ModelState.IsValid)
            {
                ErrorMessage = "There are validation errors, please correct them and try again.";
                return View(model);
            }

            AccountValidationModel accountValidation = null;
            if (_aeSettings.UseCoA)
            {
                accountValidation = await _aggieEnterpriseService.IsAccountValid(model.Rate.Account, validateRate: true);
            }
            else
            {
                accountValidation = await _financialService.IsValid(model.Rate.Account);
            }

            if (!accountValidation.IsValid)
            {
                ModelState.AddModelError("Rate.Account", $"Field: {accountValidation.Field} is not valid: {accountValidation.Message}");
            }
            else
            {
                if (!_aeSettings.UseCoA && string.IsNullOrWhiteSpace(accountValidation.KfsAccount.ObjectCode))
                {
                    ModelState.AddModelError("Rate.Account", $"Object Code is missing from the account.");
                }
                //TODO: Look at the segments to check this in the COA?
            }

            if (accountValidation.Warnings.Any())
            {
                ErrorMessage = "This COA has associated warnings, please select details to review them.";
            }

            if (!ModelState.IsValid)
            {
                ErrorMessage = "There are validation errors, please correct them and try again.";
                return View(model);
            }


            var rateToEdit = await _dbContext.Rates.Include(a => a.UpdatedBy).Include(a => a.CreatedBy).SingleAsync(a => a.Id == id);

            var user = await _userService.GetCurrentUser();

            UpdateCommonValues(model, rateToEdit, accountValidation, user);

            try
            {
                await _dbContext.SaveChangesAsync();
                if (_aeSettings.UseCoA)
                {
                    if (rateToEdit.IsPassthrough && accountValidation.CoaChartType == AggieEnterpriseApi.Validation.FinancialChartStringType.Gl && accountValidation.GlSegments.Account != "775001")
                    {
                        Message = "Rate Updated -- WARNING! Passthrough natural accounts should be 775001";
                    }
                    else
                    {
                        Message = "Rate Updated";
                    }
                }
                else
                {
                    if (rateToEdit.IsPassthrough && accountValidation.KfsAccount.ObjectCode != "80RS")
                    {
                        Message = "Rate Updated -- WARNING! Passthrough object code is not 80RS";
                    }
                    else
                    {
                        Message = "Rate Updated";
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ErrorMessage = "There was a problem updating the Rate, please try again.";
                return View(model);
            }
        }

        private void UpdateCommonValues(RateEditModel model, Rate destinationRate, AccountValidationModel accountValidation, User user)
        {
            if (_aeSettings.UseCoA)
            {
                destinationRate.Account = accountValidation.FinancialSegmentString;
            }
            else
            {
                destinationRate.Account = accountValidation.KfsAccount.ToString();
            }
            
            destinationRate.BillingUnit   = model.Rate.BillingUnit;
            destinationRate.Description   = model.Rate.Description;
            destinationRate.EffectiveOn   = model.Rate.EffectiveOn.FromPacificTime();
            destinationRate.Price         = model.Rate.Price; 
            destinationRate.Type          = model.Rate.Type;
            destinationRate.Unit          = model.Rate.Unit;
            destinationRate.UpdatedOn     = DateTime.UtcNow;
            destinationRate.UpdatedBy     = user;
            destinationRate.IsPassthrough = model.Rate.Type == Rate.Types.Other && model.Rate.IsPassthrough;
        }

        // GET: RateController/Delete/5
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Delete(int id)
        {
            var rate = await _dbContext.Rates
                .Include(a => a.UpdatedBy)
                .Include(a => a.CreatedBy)
                .SingleAsync(a => a.Id == id);
            var model = new RateDetailsModel { Rate = rate };
            model.AccountValidation = await _financialService.IsValid(model.Rate.Account);

            return View(model);
        }

        // POST: RateController/Delete/5
        [HttpPost]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            var rateToDelete = await _dbContext.Rates.SingleAsync(a => a.Id == id && a.IsActive);
            var user = await _userService.GetCurrentUser();

            rateToDelete.IsActive  = false;
            rateToDelete.UpdatedOn = DateTime.UtcNow;
            rateToDelete.UpdatedBy = user;
            try
            {
                await _dbContext.SaveChangesAsync();
                Message = "Rate deactivated.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ErrorMessage = "There was a problem trying to deactivate the Rate.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
