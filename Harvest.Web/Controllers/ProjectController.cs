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
        public ActionResult Index()
        {
            // // TODO: only show user's projects
            // var projects = await _dbContext.Projects.Take(20).ToArrayAsync();
            // return View(projects);
            return View("React");
        }

        // TODO: move or determine proper permissions
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Active()
        {
            // TODO: only show projects where between start and end?
            return Ok(await _dbContext.Projects.Include(p => p.PrincipalInvestigator).Where(p => p.IsActive).ToArrayAsync());
        }

        // TODO: move or determine proper permissions
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> Invoices(int id)
        {
            return Ok(await _dbContext.Invoices.Where(a => a.ProjectId == id).ToArrayAsync());
        }

        public ActionResult Details(int id)
        {
            // TODO: move routes so react handles this natively and place API stuff in own controller
            return View("React");
        }

        // TODO: permissions
        // Returns JSON info of the project
        public async Task<ActionResult> Get(int id)
        {
            return Ok(await _dbContext.Projects.Include(p => p.PrincipalInvestigator).Include(p => p.CreatedBy).SingleAsync(p => p.Id == id));
        }

        [HttpGet]
        public async Task<ActionResult> AccountApproval(int id)
        {
            var project = await _dbContext.Projects.Include(p => p.Accounts).SingleAsync(p => p.Id == id);

            return View(project);
        }

        [HttpPost]
        public async Task<ActionResult> AccountApproval(int id, bool approved)
        {
            // TODO: Handle account not approved condition
            // TODO: IF we want to approve accounts individually, handle that here
            var project = await _dbContext.Projects.Include(x => x.Accounts).SingleAsync(p => p.Id == id);
            var user = await _userService.GetCurrentUser();

            // assume all accounts are approved
            foreach (var account in project.Accounts)
            {
                account.ApprovedOn = DateTime.UtcNow;
                account.ApprovedById = user.Id;
            }

            project.IsApproved = true;
            project.Status = Project.Statuses.Active;

            // TODO: log history

            await _dbContext.SaveChangesAsync();

            Message = "Accounts Approved. Project is now active";

            return RedirectToAction("Details", new { id });
        }
    }
}
