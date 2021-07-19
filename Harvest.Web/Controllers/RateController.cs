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

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.SystemAccess)]
    public class RateController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IFinancialService _financialService;

        public RateController(AppDbContext dbContext, IUserService userService, IFinancialService financialService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _financialService = financialService;
        }
        // GET: RateController
        public async Task<ActionResult> Index()
        {
            var rates = await _dbContext.Rates.Where(a => a.IsActive).ToListAsync();
            return View(rates);
        }

        // Returns the Active rates via API
        // GET: RateController/Active
        public async Task<ActionResult> Active()
        {
            var rates = await _dbContext.Rates.Where(a => a.IsActive).Select(r => new { r.Price, r.Unit, r.Type, r.Description, r.Id }).ToArrayAsync();
            return Ok(rates);
        }

        // GET: RateController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var rate = await _dbContext.Rates
                .Include(a => a.UpdatedBy)
                .Include(a => a.CreatedBy)
                .SingleAsync(a => a.Id == id);
            var model = new RateDetailsModel {Rate = rate};
            model.AccountValidation = await _financialService.IsValid(model.Rate.Account);

            return View(model);
        }

        // GET: RateController/Create
        public ActionResult Create()
        {
            var model = new RateEditModel {Rate = new Rate(), TypeList = Rate.Types.TypeList};
            model.Rate.Account = "3-";
            return View(model);
        }

        // POST: RateController/Create
        [HttpPost]
        public async Task<ActionResult> Create(RateEditModel model)
        {
            model.TypeList = Rate.Types.TypeList; //Set it here in case the model isn't valid
            if (!ModelState.IsValid)
            {
                ErrorMessage = "There are validation errors, please correct them and try again.";
                return View(model);
            }

            var accountValidation = await _financialService.IsValid(model.Rate.Account);
            if (!accountValidation.IsValid)
            {
                ModelState.AddModelError("Rate.Account", $"Field: {accountValidation.Field} is not valid: {accountValidation.Message}");
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
                Message = "Rate Created";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ErrorMessage = "There was an error trying to create this rate.";
                return View(model);
            }
        }

        // GET: RateController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var rate = await _dbContext.Rates.SingleAsync(a => a.Id == id);
            var model = new RateEditModel { Rate = rate, TypeList = Rate.Types.TypeList };

            return View(model);
        }

        // POST: RateController/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(int id, RateEditModel model)
        {
            model.Rate.Id = id;
            model.TypeList = Rate.Types.TypeList; //Set it here in case the model isn't valid
            if (!ModelState.IsValid)
            {
                ErrorMessage = "There are validation errors, please correct them and try again.";
                return View(model);
            }

            var accountValidation = await _financialService.IsValid(model.Rate.Account);
            if (!accountValidation.IsValid)
            {
                ModelState.AddModelError("Rate.Account", $"Field: {accountValidation.Field} is not valid: {accountValidation.Message}");
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
                Message = "Rate Updated";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ErrorMessage = "There was a problem updating the Rate, please try again.";
                return View(model);
            }
        }

        private static void UpdateCommonValues(RateEditModel model, Rate destinationRate, AccountValidationModel accountValidation, User user)
        {
            destinationRate.Account     = accountValidation.KfsAccount.ToString(); 
            destinationRate.BillingUnit = model.Rate.BillingUnit;
            destinationRate.Description = model.Rate.Description;
            destinationRate.EffectiveOn = model.Rate.EffectiveOn.FromPacificTime();
            destinationRate.Price       = model.Rate.Price; 
            destinationRate.Type        = model.Rate.Type;
            destinationRate.Unit        = model.Rate.Unit;
            destinationRate.UpdatedOn   = DateTime.UtcNow;
            destinationRate.UpdatedBy   = user;
        }

        // GET: RateController/Delete/5
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
