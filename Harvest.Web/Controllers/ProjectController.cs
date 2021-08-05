using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Harvest.Web.Controllers
{
    [Authorize]
    public class ProjectController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IProjectHistoryService _historyService;
        private readonly StorageSettings storageSettings;
        private readonly IFileService fileService;        
        public ProjectController(AppDbContext dbContext, IUserService userService, IOptions<StorageSettings> storageSettings,
            IFileService fileService, IProjectHistoryService historyService)
        {
            this._dbContext = dbContext;
            this._userService = userService;
            this.storageSettings = storageSettings.Value;
            this.fileService = fileService;
            this._historyService = historyService;
        }

        public ActionResult Index()
        {
            // // TODO: only show user's projects
            // var projects = await _dbContext.Projects.Take(20).ToArrayAsync();
            // return View(projects);
            return View("React");
        }

        public ActionResult Mine()
        {
            // // TODO: only show user's projects
            // var projects = await _dbContext.Projects.Take(20).ToArrayAsync();
            // return View(projects);
            return View("React");
        }

        [Authorize(Policy = AccessCodes.WorkerAccess)]
        public async Task<ActionResult> Active()
        {
            // TODO: only show projects where between start and end?
            return Ok(await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToArrayAsync());
        }

        public async Task<ActionResult> GetMine()
        {
            var user = await _userService.GetCurrentUser();

            // TODO: only show projects where between start and end?
            return Ok(await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Where(p => p.IsActive && p.PrincipalInvestigatorId == user.Id)
                .ToArrayAsync());
        }

        public async Task<ActionResult> Invoices(int projectId)
        {
            var user = await _userService.GetCurrentUser();
            var hasAccess = await _userService.HasAccess(AccessCodes.FieldManagerAccess);
            return Ok(await _dbContext.Invoices.Where(a => 
                a.ProjectId == projectId 
                && (hasAccess || a.Project.PrincipalInvestigatorId == user.Id))
                .ToArrayAsync());
        }

        public ActionResult Details(int projectId)
        {
            // TODO: move routes so react handles this natively and place API stuff in own controller
            return View("React");
        }

        // Returns JSON info of the project
        [Authorize(Policy = AccessCodes.PrincipalInvestigatorAndWorker)]
        public async Task<ActionResult> Get(int projectId)
        {
            var user = await _userService.GetCurrentUser();

            var project = await _dbContext.Projects
                .Include(a => a.Attachments)
                .Include(p => p.Accounts)
                .Include(p => p.PrincipalInvestigator)
                .Include(p => p.CreatedBy)
                .SingleOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }


            foreach (var file in project.Attachments) {
                file.SasLink = fileService.GetDownloadUrl(storageSettings.ContainerName, file.Identifier).AbsoluteUri;
            }

            return Ok(project);
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<ActionResult> AccountApproval(int projectId)
        {
            var project = await _dbContext.Projects.Include(p => p.Accounts).SingleAsync(p => p.Id == projectId);

            return View(project);
        }

        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<IActionResult> RefreshTotal(int projectId)
        {
            var project = await _dbContext.Projects.SingleAsync(a => a.Id == projectId);
            var invoiceTotal = await _dbContext.Invoices.Where(a =>
                    a.ProjectId == projectId &&
                    (a.Status == Invoice.Statuses.Pending || a.Status == Invoice.Statuses.Completed)).Select(a => a.Total).SumAsync();
            var originalTotal = project.ChargedTotal;
            if (project.ChargedTotal != invoiceTotal)
            {
                project.ChargedTotal = invoiceTotal;

                await _historyService.ProjectTotalRefreshed(project.Id, project);
                await _dbContext.SaveChangesAsync();
                return Content($"Project total updated from {originalTotal} to {project.ChargedTotal}");
            }

            return Content("Project already up to date.");
        }
    }
}
