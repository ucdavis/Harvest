using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Humanizer;
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
            var crops = await _dbContext.Crops.OrderBy(a => a.Type).ThenBy(a => a.Name).ToListAsync();
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
        public async Task<ActionResult> Create(Crop model)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "There are validation errors, please correct them and try again.";
                return View(model);
            }
            if (!Project.CropTypes.TypeList.Contains(model.Type))
            {
                ErrorMessage = $"Type must be {Project.CropTypes.Tree} or {Project.CropTypes.Row}";
                return View(model);
            }

            if (await _dbContext.Crops.AnyAsync(a => a.Type == model.Type && a.Name == model.Name.Trim()))
            {
                Message = "Crop already exists";
                return View(model);
            }

            var modelToCreate = new Crop {Type = model.Type, Name = model.Name.Trim().Humanize(LetterCasing.Title)};
            
            try
            {
                await _dbContext.Crops.AddAsync(modelToCreate);
                await _dbContext.SaveChangesAsync();
                Message = $"{modelToCreate.Name} Created";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ErrorMessage = "There was an error trying to create this rate.";
                return View(modelToCreate);
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
