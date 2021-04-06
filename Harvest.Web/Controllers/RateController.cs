using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.AdminAccess)]
    public class RateController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IIdentityService _identityService;
        private readonly IFinancialService _financialService;

        public RateController(AppDbContext dbContext, IIdentityService identityService, IFinancialService financialService)
        {
            _dbContext = dbContext;
            _identityService = identityService;
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
            return View();
        }

        // POST: RateController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
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
