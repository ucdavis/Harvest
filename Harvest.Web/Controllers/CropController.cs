using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class CropController : SuperController
    {
        private readonly AppDbContext _dbContext;

        public CropController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: CropController
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Index()
        {
            var crops = await _dbContext.Crops.OrderBy(a => a.Type).ThenBy(a => a.Name).ToListAsync();
            return View(crops);
        }

        // GET: CropController/Create
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public ActionResult Create()
        {
            var model = new Crop { Type = Project.CropTypes.Row };
            return View(model);
        }

        // POST: CropController/Create
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        [HttpPost]
        public async Task<ActionResult> Create(Crop model)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "There are validation errors, please correct them and try again.";
                return View(model);
            }
            if (!Project.CropTypes.TypeList.Contains(model.Type))
            {
                ErrorMessage = $"Type must be {Project.CropTypes.Tree} or {Project.CropTypes.Row} or {Project.CropTypes.Other}";
                return View(model);
            }

            if (await _dbContext.Crops.AnyAsync(a => a.Type == model.Type && a.Name == model.Name.Trim()))
            {
                Message = "Crop already exists";
                return View(model);
            }

            var modelToCreate = new Crop { Type = model.Type, Name = model.Name.Trim().Humanize(LetterCasing.Title) };

            try
            {
                await _dbContext.Crops.AddAsync(modelToCreate);
                await _dbContext.SaveChangesAsync();
                Message = $"{modelToCreate.Name} Created";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ErrorMessage = "There was an error trying to create this crop.";
                return View(modelToCreate);
            }

        }

        // GET: CropController/Edit/5
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Edit(int id)
        {
            var model = await _dbContext.Crops.SingleAsync(a => a.Id == id);
            return View(model);
        }

        // POST: CropController/Edit/5
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        [HttpPost]
        public async Task<ActionResult> Edit(int id, Crop model)
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "There are validation errors, please correct them and try again.";
                return View(model);
            }
            if (!Project.CropTypes.TypeList.Contains(model.Type))
            {
                ErrorMessage = $"Type must be {Project.CropTypes.Tree} or {Project.CropTypes.Row} or {Project.CropTypes.Other}";
                return View(model);
            }

            if (await _dbContext.Crops.AnyAsync(a => a.Id != id && a.Type == model.Type && a.Name == model.Name.Trim()))
            {
                Message = "Crop you are changing that to already exists";
                return View(model);
            }

            var modelToUpdate = await _dbContext.Crops.SingleAsync(a => a.Id == id);
            modelToUpdate.Type = model.Type;
            modelToUpdate.Name = model.Name.Trim().Humanize(LetterCasing.Title);

            try
            {
                await _dbContext.SaveChangesAsync();
                Message = $"{modelToUpdate.Name} Edited";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ErrorMessage = "There was an error trying to edit this crop.";
                return View(modelToUpdate);
            }
        }

        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        // GET: CropController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var model = await _dbContext.Crops.SingleAsync(a => a.Id == id);
            return View(model);
        }

        // POST: CropController/Delete/5
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        [HttpPost]
        public async Task<ActionResult> Delete(int id, Crop model)
        {
            var cropToDelete = await _dbContext.Crops.SingleAsync(a => a.Id == id);
            var saveName = cropToDelete.Name;
            try
            {
                _dbContext.Crops.Remove(cropToDelete);
                await _dbContext.SaveChangesAsync();
                Message = $"Crop {saveName} Deleted";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CropController/Search?type=Row
        public async Task<ActionResult> Search([FromQuery] string type, [FromQuery] string query = "")
        {
            var queryable = _dbContext.Crops.Where(c => c.Type == type);
            if (!string.IsNullOrWhiteSpace(query))
            {
                queryable = queryable.Where(c => EF.Functions.Contains(c.Name, query));
            }
            var model = await queryable.ToArrayAsync();
            return Ok(model);
        }
    }
}
