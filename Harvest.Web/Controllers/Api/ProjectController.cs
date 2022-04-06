using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
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
            var currentUser = await _userService.GetCurrentUser();
          
            var newProject = new Project
            {
                Name = postModel.Project.Name,
                Crop = postModel.Project.Crop,
                CropType = postModel.Project.CropType,
                Start = DateTime.UtcNow.ToPacificTime(), //Start and stop just now and a month from now?
                End = DateTime.UtcNow.ToPacificTime().AddMonths(1),
                CreatedOn = DateTime.UtcNow,
                CreatedById = currentUser.Id,
                IsActive = true,
                IsApproved = true,
                Requirements = postModel.Project.Requirements, 
            };

            // create PI if needed and assign to project
            var pi = await _dbContext.Users.SingleOrDefaultAsync(x => x.Iam == postModel.Project.PrincipalInvestigator.Iam);
            if (pi != null)
            {
                newProject.PrincipalInvestigatorId = pi.Id;
            }
            else
            {
                // TODO: if PI doesn't exist we'll just use what our client sent.  We may instead want to re-query to ensure the most up to date info?
                newProject.PrincipalInvestigator = postModel.Project.PrincipalInvestigator;
            }

            newProject.UpdateStatus(Project.Statuses.Active);

            newProject.Acres = 0;
            newProject.ChargedTotal = 0;
            newProject.QuoteTotal = postModel.Expenses.Select(a => a.Total).Sum();
           




            newProject.Expenses =  new List<Expense>();

            var percentage = 0.0m;

            foreach (var account in postModel.Accounts)
            {
                // Accounts will be auto-approved by quote approver
                account.ApprovedById = currentUser.Id;
                //account.ProjectId= newProject.Id;
                account.Project = newProject;
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
                //expense.ProjectId = newProject.Id;
                expense.Project = newProject;
                expense.InvoiceId = null;
                expense.Account = allRates.Single(a => a.Id == expense.RateId).Account;
                expense.IsPassthrough = allRates.Single(a => a.Id == expense.RateId).IsPassthrough;
                newProject.Expenses.Add(expense);
            }
            //_dbContext.Expenses.AddRange(postModel.Expenses);

            //Create Quote from Expenses? Or if needed, pass it as a new parameter in the postmodel? I think expenses have much stripped out.
            //I think we may just want to leave null
            //Without the quote, the create change request doesn't work?


            await _dbContext.Projects.AddAsync(newProject);
            await _dbContext.SaveChangesAsync();

            var quote = new Quote();
            quote.InitiatedById = currentUser.Id;
            quote.CreatedDate = DateTime.UtcNow;
            quote.Project = newProject;
            quote.ProjectId = newProject.Id;
            quote.Total = postModel.Expenses.Select(a => a.Total).Sum(); //Calculate totals here? probably not done yet
            postModel.Quote.EquipmentTotal = (double)postModel.Expenses.Where(a=> a.Type == Rate.Types.Equipment).Select(a => a.Total).Sum();
            postModel.Quote.LaborTotal = (double)postModel.Expenses.Where(a => a.Type == Rate.Types.Labor).Select(a => a.Total).Sum();
            postModel.Quote.OtherTotal = (double)postModel.Expenses.Where(a => a.Type == Rate.Types.Other).Select(a => a.Total).Sum();
            postModel.Quote.GrandTotal = (double)quote.Total;

            var activities = new List<Activity>();
            foreach(var activity in postModel.Quote.Activities)
            {
                if(activity.Total > 0)
                {
                    var workItems = new List<WorkItem>();
                    foreach(var wi in activity.WorkItems)
                    {
                        if(wi.Total > 0)
                        {
                            workItems.Add(wi);
                        }
                    }
                    activity.WorkItems = workItems.ToArray();
                    activities.Add(activity);
                }
            }
            postModel.Quote.Activities = activities.ToArray();

            quote.Text = QuoteDetail.Serialize(postModel.Quote);
            quote.ApprovedById = currentUser.Id;
            quote.Status = Quote.Statuses.Approved;

            newProject.Quote = quote;
 
            //_dbContext.Projects.Update(newProject);

            await _dbContext.Quotes.AddAsync(quote);
            await _dbContext.SaveChangesAsync();


            return Ok(newProject);
        }
    }
}
