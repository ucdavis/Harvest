using System;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers.Api
{
    [Authorize(Policy = AccessCodes.SupervisorAccess)]
    public class FieldController : Controller
    {
        private AppDbContext _dbContext;

        public FieldController(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        // Get all fields that are part of projects active between the given dates
        public async Task<IActionResult> Active(DateTime start, DateTime end, int projectId)
        {
            var origProjectId = await _dbContext.Projects.Where(a => a.Id == projectId).Select(a => a.OriginalProjectId).SingleOrDefaultAsync();

            // TODO: NOTE - need to convert Location to Geometry since we store it in the db as Location even though GeoJSON would like the name to be "geometry"
            // overlapping projects are those that start before the given project ends, and ends after the given project starts
            var fields = await _dbContext.Fields
                .Where(f =>f.ProjectId != origProjectId && f.Project.IsApproved && (f.Project.End >= start && f.Project.Start <= end))
                .AsNoTracking().Select(f => new { f.Id, f.Crop, Geometry = f.Location }).ToArrayAsync();

            return Json(fields);

        }
    }
}