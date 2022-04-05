using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Harvest.Web.Models;
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
                    .OrderBy(a => a.CreatedOn) // Oldest first?
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

        [HttpPost]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<IActionResult> CreateAdhoc([FromBody] AdhocPostModel postModel)
        {
            postModel.Project = new Project();
            postModel.Project.Name = "Fake One 3";
            postModel.Project.Crop = "Special Services";
            postModel.Project.CropType = "Row";
            postModel.Project.Start = DateTime.Now;
            postModel.Project.End = DateTime.Now.AddMonths(1);
            postModel.Project.Requirements = "Fake Requirements";

            postModel.Accounts = new Account[2];
            postModel.Accounts[0] = new Account();
            postModel.Accounts[1] = new Account();
            postModel.Accounts[0].Percentage = 75m;
            postModel.Accounts[0].Name = "CrU 1";
            postModel.Accounts[0].Number = "3-CRU9033";

            postModel.Accounts[1].Percentage = 25m;
            postModel.Accounts[1].Name = "CrU 2";
            postModel.Accounts[1].Number = "3-CRUEQIP";




            var currentUser = await _userService.GetCurrentUser();
            var newProject = new Project
            {
                Name = postModel.Project.Name,
                Crop = postModel.Project.Crop,
                CropType = postModel.Project.CropType,
                Start = postModel.Project.Start, //Start and stop just now and a month from now?
                End = postModel.Project.End,
                CreatedOn = DateTime.UtcNow,
                CreatedById = currentUser.Id,
                IsActive = true,
                IsApproved = true,
                Requirements = postModel.Project.Requirements,
                PrincipalInvestigatorId = currentUser.Id,
            };
            newProject.UpdateStatus(Project.Statuses.Active);

            newProject.Acres = 0;
            newProject.ChargedTotal = 0;
            newProject.QuoteTotal = postModel.Expenses.Select(a => a.Total).Sum();
           

            try
            {
                await _dbContext.Projects.AddAsync(newProject);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var xxx = ex.Message;
            }


            newProject.Expenses =  new List<Expense>();
            //Save project first if projectId needed. Wrap in a transaction
            var percentage = 0.0m;

            foreach (var account in postModel.Accounts)
            {
                // Accounts will be auto-approved by quote approver
                account.ApprovedById = currentUser.Id;
                account.ProjectId= newProject.Id;
                account.ApprovedOn = DateTime.UtcNow;
                percentage += account.Percentage;
                if (account.Percentage < 0)
                {
                    return BadRequest("Negative Percentage Detected");
                }
                newProject.Accounts.Add(account); //Don't need to specify projectId if I add it to the project?
            }

            if (percentage != 100.0m)
            {
                return BadRequest("Percentage of accounts is not 100%");
            }

            var allRates = await _dbContext.Rates.Where(a => a.IsActive).ToListAsync();
            foreach (var expense in postModel.Expenses)
            {
                expense.CreatedBy = currentUser;
                expense.CreatedOn = DateTime.UtcNow;
                expense.ProjectId = newProject.Id;
                expense.InvoiceId = null;
                expense.Account = allRates.Single(a => a.Id == expense.RateId).Account;
                expense.IsPassthrough = allRates.Single(a => a.Id == expense.RateId).IsPassthrough;
            }
            _dbContext.Expenses.AddRange(postModel.Expenses);

            //Create Quote from Expenses?
            //I think we may just want to leave null


            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var xxx = ex.Message;
            }

            return Ok(newProject);
        }
    }
}
