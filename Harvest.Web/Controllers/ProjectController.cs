using System;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class ProjectController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public ProjectController(AppDbContext dbContext, IUserService userService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
        }
        public async Task<ActionResult> Index()
        {
            // TODO: only show user's projects
            var projects = await _dbContext.Projects.Take(20).ToArrayAsync();
            return View(projects);
        }

        // TODO: move or determine proper permissions
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Active()
        {
            // TODO: only show projects where between start and end?
            return Ok(await _dbContext.Projects.Include(p => p.PrincipalInvestigator).Where(p => p.IsActive).ToArrayAsync());
        }

        public async Task<ActionResult> Details(int id)
        {
            return View(await _dbContext.Projects.SingleAsync(p => p.Id == id));
        }
    }
}
