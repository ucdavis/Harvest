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
using Serilog;

namespace Harvest.Web.Controllers.Api
{
    [Authorize]
    public class ProjectController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IProjectHistoryService _historyService;
        private readonly StorageSettings _storageSettings;
        private readonly IFileService _fileService;
        private readonly IInvoiceService _invoiceService;

        public ProjectController(AppDbContext dbContext, IUserService userService, IOptions<StorageSettings> storageSettings,
            IFileService fileService, IProjectHistoryService historyService, IInvoiceService invoiceService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _storageSettings = storageSettings.Value;
            _fileService = fileService;
            _historyService = historyService;
            _invoiceService = invoiceService;
        }

        [Authorize(Policy = AccessCodes.WorkerAccess)]
        public async Task<ActionResult> All()
        {
            // TODO: only show projects where between start and end?
            return Ok(await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToArrayAsync());
        }

        [Authorize(Policy = AccessCodes.WorkerAccess)]
        public async Task<ActionResult> Active()
        {
            // TODO: only show projects where between start and end?
            return Ok(await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Where(p => p.IsActive && p.Status == Project.Statuses.Active)
                .OrderBy(p => p.Name)
                .ToArrayAsync());
        }

        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> GetCompleted()
        {
            return Ok(await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Where(p => p.IsActive && p.Status == Project.Statuses.Completed)
                .OrderBy(p => p.Name)
                .ToArrayAsync());
        }

        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> RequiringManagerAttention()
        {
            // return basic info on projects which are waiting for manager attention
            var attentionStatuses = new string[] { Project.Statuses.Requested, Project.Statuses.ChangeRequested, Project.Statuses.QuoteRejected }.ToArray();

            return Ok(await _dbContext.Projects.AsNoTracking()
                .Include(p => p.PrincipalInvestigator)
                .Where(p => p.IsActive && attentionStatuses.Contains(p.Status))
                .OrderBy(p => p.CreatedOn) // older is more important, so it should be first
                .ToArrayAsync());
        }

        public async Task<ActionResult> RequiringPIAttention()
        {
            try
            {
                var user = await _userService.GetCurrentUser();
                if (user == null)
                {
                    throw new Exception("User is null");
                }
                var attentionStatuses = new string[] { Project.Statuses.PendingApproval, Project.Statuses.PendingAccountApproval }.ToArray();

                return Ok(await _dbContext.Projects.AsNoTracking()
                    .Where(p => p.PrincipalInvestigatorId == user.Id && p.IsActive && attentionStatuses.Contains(p.Status))
                    .Select(p => new { p.Id, p.Status, p.Name })
                    .Take(4)
                    .ToArrayAsync());

            }
            catch (Exception e)
            {
                Log.Error("Exception getting projects requiring PI Attention. {e}", e.Message);
                return Ok(null);
            }

            // return basic info on projects which are waiting for PI attention

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

        // Returns JSON info of the project
        [Authorize(Policy = AccessCodes.PrincipalInvestigator)]
        public async Task<ActionResult> Get(int projectId)
        {
            var user = await _userService.GetCurrentUser();
            var project = await _dbContext.Projects
                .Include(a => a.Attachments)
                .Include(p => p.Accounts)
                .Include(p => p.PrincipalInvestigator)
                .Include(p => p.CreatedBy)
                .Include(p => p.AcreageRate)
                .SingleOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }


            foreach (var file in project.Attachments)
            {
                file.SasLink = _fileService.GetDownloadUrl(_storageSettings.ContainerName, file.Identifier).AbsoluteUri;
            }

            return Ok(project);
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> GetFields()
        {
            var fields = await _dbContext.Fields.Where(f => f.IsActive && f.Project.Status == Project.Statuses.Active).Include(f => f.Project).ToListAsync();

            return Ok(fields);
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
