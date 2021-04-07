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
using Harvest.Core.Services;
using Harvest.Web.Models.RateModels;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.AdminAccess)]
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

        // GET: RateController/Details/5
        public ActionResult Details(int id)
        {
            return View();
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
        [ValidateAntiForgeryToken]
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
            var user = _userService.GetCurrentUser();

            var rateToCreate = new Rate();
            rateToCreate.IsActive = true;
            rateToCreate.Account = model.Rate.Account;
            rateToCreate.BillingUnit = model.Rate.BillingUnit;
            rateToCreate.Description = model.Rate.Description;
            rateToCreate.EffectiveOn = model.Rate.EffectiveOn.ToPacificTime();
            rateToCreate.Price = model.Rate.Price;
            rateToCreate.Type = model.Rate.Type;
            rateToCreate.Unit = model.Rate.Unit;
            rateToCreate.CreatedOn = createTime;
            rateToCreate.UpdatedOn = createTime;
            //rateToCreate.CreatedById = user.Id;

            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: RateController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: RateController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: RateController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: RateController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
