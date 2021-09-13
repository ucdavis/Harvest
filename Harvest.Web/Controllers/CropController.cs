using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize(Policy = AccessCodes.FieldManagerAccess)]
    public class CropController : SuperController
    {
        private readonly AppDbContext _dbContext;

        public CropController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        // GET: CropController
        public async Task<ActionResult> Index()
        {
            var crops = await _dbContext.CropLookups.OrderBy(a => a.Type).ThenBy(a => a.Crop).ToListAsync();
            return View(crops);
        }

        // GET: CropController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CropController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CropController/Create
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

        // GET: CropController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CropController/Edit/5
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

        // GET: CropController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CropController/Delete/5
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
