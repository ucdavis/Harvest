using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Models;
using Harvest.Core.Models.ProjectModels;
using Harvest.Core.Models.Settings;
using Harvest.Core.Services;
using Harvest.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IEmailService _emailService;

        public ProjectController(AppDbContext dbContext, IUserService userService, IOptions<StorageSettings> storageSettings,
            IFileService fileService, IProjectHistoryService historyService, IEmailService emailService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _storageSettings = storageSettings.Value;
            _fileService = fileService;
            _historyService = historyService;
            _emailService = emailService;
        }

        [Authorize(Policy = AccessCodes.ProjectAccess)]

        public async Task<ActionResult> All()
        {
            // TODO: only show projects where between start and end?
            return Ok(await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Include(a => a.Team)
                .Where(p => p.IsActive && p.Team.Slug == TeamSlug)
                .OrderBy(p => p.Name)
                .ToArrayAsync());
        }

        [Authorize(Policy = AccessCodes.ProjectAccess)]
        public async Task<ActionResult> Active()
        {
            // TODO: only show projects where between start and end?
            return Ok(await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Include(a => a.Team)
                .Where(p => p.IsActive && p.Team.Slug == TeamSlug && p.Status == Project.Statuses.Active)
                .OrderBy(p => p.Name)
                .ToArrayAsync());
        }

        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> GetCompleted()
        {
            return Ok(await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Include(a => a.Team)
                .Where(p => p.IsActive && p.Team.Slug == TeamSlug && p.Status == Project.Statuses.Completed)
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
                .Include(a => a.Team)
                .Where(p => p.IsActive && p.Team.Slug == TeamSlug && attentionStatuses.Contains(p.Status))
                .OrderBy(p => p.CreatedOn) // older is more important, so it should be first
                .ToArrayAsync());
        }

        [Route("/api/{controller}/{action}")]
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

                var rtValue = await _dbContext.Projects.AsNoTracking()
                    .Include(p => p.PrincipalInvestigator)
                    .Include(a => a.Team)
                    .Where(p => p.PrincipalInvestigatorId == user.Id && p.IsActive && attentionStatuses.Contains(p.Status))
                    .OrderBy(a => a.CreatedOn) // Oldest first?
                    .Select(p => new { p.Id, p.Status, p.Name, p.Team })
                    .Take(4)
                    .ToArrayAsync();

                return Ok(rtValue);

            }
            catch (Exception e)
            {
                Log.Error("Exception getting projects requiring PI Attention. {e}", e.Message);
                return Ok(null);
            }

            // return basic info on projects which are waiting for PI attention

        }

        [Route("/api/{controller}/{action}")]
        public async Task<ActionResult> GetMine()
        {
            var user = await _userService.GetCurrentUser();

            //Look for any projects where I have a project permission
            if (user == null)
            {
                return Unauthorized();
            }

            // TODO: only show projects where between start and end?
            return Ok(await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Include(p => p.Team)
                .Where(p => p.IsActive && (p.PrincipalInvestigatorId == user.Id || p.ProjectPermissions.Any(a => a.UserId == user.Id)))
                .ToArrayAsync());
        }

        // Returns JSON info of the project
        [Route("/api/{team}/Project/Get/{projectId}/{shareId?}")]
        [Authorize(Policy = AccessCodes.InvoiceAccess)] //PI, Finance, Field Manager, Supervisor -- Don't really know a better name for this access (Maybe ProjectViewAccess?)        
        public async Task<ActionResult> Get(int projectId, Guid? shareId = null)
        {            
            var user = await _userService.GetCurrentUser();
            var project = await _dbContext.Projects
                .Include(a => a.Team)
                .Include(a => a.Attachments)
                .Include(p => p.Accounts)
                .Include(p => p.PrincipalInvestigator)
                .Include(p => p.CreatedBy)
                .Include(p => p.AcreageRate)
                .Include(p => p.ProjectPermissions).ThenInclude(a => a.User)
                .SingleOrDefaultAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);

            if (project == null)
            {
                return NotFound();
            }


            foreach (var file in project.Attachments)
            {
                file.SasLink = _fileService.GetDownloadUrl(_storageSettings.ContainerName, file.Identifier).AbsoluteUri;
            }

            if(shareId != null && project.ShareId != shareId)
            {
                return BadRequest("share id invalid");
            }

            if (shareId != null && project.PrincipalInvestigator.Id != user.Id)
            {
                //Check if shareId is used and it is not the PI. If so, log that.
                Log.Information("User {user} is trying to access project {projectId} with shareId {shareId}.", user.Id, projectId, shareId);
            }

            return Ok(project);
        }

        [HttpGet]
        public async Task<ActionResult> GetPendingChangeRequests(int projectId)
        {

            var projects = await _dbContext.Projects.Where(a => a.Team.Slug == TeamSlug && a.OriginalProjectId == projectId && a.IsActive && (a.Status == Project.Statuses.PendingApproval || a.Status == Project.Statuses.ChangeRequested || a.Status == Project.Statuses.QuoteRejected)).Select(ProjectChangeRequestModel.Projection()).ToListAsync();
            return Ok(projects);
        }

        [Authorize(Policy = AccessCodes.InvoiceAccess)]
        [HttpGet]
        public async Task<ActionResult> ListHistory(int projectId, int? maxRows)
        {
            var query = _dbContext.ProjectHistory.Include(a => a.Actor)
                .Where(a => a.Project.Id == projectId && a.Project.Team.Slug == TeamSlug && a.DisplayForPi)
                .OrderByDescending(a => a.ActionDate);
            if (maxRows.HasValue)
            {
                query = (IOrderedQueryable<ProjectHistory>)query.Take(maxRows.Value);
            }
            return Ok(await query.ToListAsync());
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigatorOnly)]
        public async Task<ActionResult> ResetShareLink(int projectId)
        {
            var user = await _userService.GetCurrentUser();
            var project = await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Include(p => p.ProjectPermissions).ThenInclude(pp => pp.User)
                .SingleOrDefaultAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);
            if (project == null)
            {
                return NotFound();
            }
            if (project.PrincipalInvestigator.Id != user.Id && !project.ProjectPermissions.Any(a => a.User.Id == user.Id && a.Permission == Role.Codes.ProjectEditor))
            {
                return BadRequest("You are not the PI of this project or an editor of the project.");
            }

            await _historyService.ShareResetByOther(projectId, "Share Id Reset", project);

            project.ShareId = Guid.NewGuid();
            await _dbContext.SaveChangesAsync();


            return Ok(project.ShareId);
        }


        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigatorOnly)]
        public async Task<ActionResult> AddProjectPermission(int projectId, [FromBody] ProjectPermission postModel)
        {
            var user = await _userService.GetCurrentUser();
            var project = await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Include(p => p.ProjectPermissions).ThenInclude(pp => pp.User)
                .SingleOrDefaultAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);
            if (project == null)
            {
                return NotFound();
            }
            if (project.PrincipalInvestigator.Id != user.Id && !project.ProjectPermissions.Any(a => a.User.Id == user.Id && a.Permission == Role.Codes.ProjectEditor))
            {
                return BadRequest("You are not the PI of this project or an editor of the project.");
            }
            
            if (project.ProjectPermissions.Any(a => a.User?.Iam == postModel.User.Iam) || project.PrincipalInvestigator.Iam == postModel.User.Iam)
            {
                return BadRequest("User already has a permission for this project.");
            }

            if(postModel.Permission != Role.Codes.ProjectEditor && postModel.Permission != Role.Codes.ProjectViewer)
            {
                return BadRequest("Invalid permission type. Only Project Editor and Project Viewer are allowed.");
            }

            var newPermission = new ProjectPermission
            {
                ProjectId = projectId,
                Permission = postModel.Permission,
            };
            

            //// create user if needed and assign to project
            var permissionUser = await _dbContext.Users.SingleOrDefaultAsync(x => x.Iam == postModel.User.Iam);
            if (permissionUser != null)
            {
                newPermission.UserId = permissionUser.Id;
            }
            else
            {
                // TODO: if User doesn't exist we'll just use what our client sent.  We may instead want to re-query to ensure the most up to date info?
                newPermission.User = postModel.User; // This will be used to create a new user if needed
            }

            await _dbContext.ProjectPermissions.AddAsync(newPermission);

            await _historyService.AdhocHistory(projectId, "ProjectPermissionAdded", $"Project Permission Added:\n{newPermission.User.Name}({newPermission.User.Email}) with permission: {newPermission.Permission.SplitCamelCase()}", project, true);
            await _dbContext.SaveChangesAsync(); // Might be able to remove the extra save if the history call is moved up.

            return Ok(newPermission);
        }

        [HttpPost]
        [Authorize(Policy = AccessCodes.PrincipalInvestigatorOnly)]
        public async Task<ActionResult> RemoveProjectPermission(int projectId, int permissionId)
        {

            var user = await _userService.GetCurrentUser();
            var project = await _dbContext.Projects
                .Include(p => p.PrincipalInvestigator)
                .Include(p => p.ProjectPermissions).ThenInclude(pp => pp.User)
                .SingleOrDefaultAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);
            if (project == null)
            {
                return NotFound();
            }
            //Might not need this check, but doesn't hurt.
            if (project.PrincipalInvestigator.Id != user.Id && !project.ProjectPermissions.Any(a => a.User.Id == user.Id && a.Permission == Role.Codes.ProjectEditor))
            {
                return BadRequest("You are not the PI of this project or an editor of the project.");
            }
            var permissionToRemove = project.ProjectPermissions
                .Where(pp => pp.ProjectId == projectId && pp.Id == permissionId).SingleOrDefault();
            if (permissionToRemove == null)
            {
                return NotFound("Permission not found.");
            }
            if(permissionToRemove.User.Iam == user.Iam)
            {
                return BadRequest("You cannot remove your own permission from the project. Please contact the PI or another" +
                    " project editor to do this.");
            }

            await _historyService.AdhocHistory(projectId, "ProjectPermissionRemoved", $"Project Permission Removed:\n{permissionToRemove.User.Name}({permissionToRemove.User.Email}) with permission: {permissionToRemove.Permission.SplitCamelCase()}", project, true);
            _dbContext.ProjectPermissions.Remove(permissionToRemove);

            

            await _dbContext.SaveChangesAsync();
            
            return Ok();
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.SupervisorAccess)]
        public async Task<ActionResult> GetFields()
        {
            var fields = await _dbContext.Fields.Where(f => f.IsActive && f.Project.Team.Slug == TeamSlug && f.Project.Status == Project.Statuses.Active).Include(f => f.Project).ToListAsync();

            return Ok(fields);
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        [Obsolete("This was never implemented. If it is, stuff like team route need to be configured")]
        public async Task<ActionResult> AccountApproval(int projectId)
        {
            var project = await _dbContext.Projects.Include(p => p.Accounts).SingleAsync(p => p.Id == projectId);

            return View(project);
        }

        [Authorize(Policy = AccessCodes.FieldManagerAccess)]
        public async Task<IActionResult> RefreshTotal(int projectId)
        {
            var project = await _dbContext.Projects.SingleAsync(a => a.Id == projectId);
            var invoiceTotal = await _dbContext.Invoices.Where(a => a.Project.Team.Slug == TeamSlug &&
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
            var team = await _dbContext.Teams.SingleOrDefaultAsync(t => t.Slug == TeamSlug);

            if (team == null)
            {
                ErrorMessage = $"Team not found! Team: {TeamSlug}";
                return RedirectToAction("Index", "Home");
            }

            var currentUser = await _userService.GetCurrentUser();

            using (var txn = await _dbContext.Database.BeginTransactionAsync())
            {

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
                    TeamId = team.Id,
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



                newProject.Expenses = new List<Expense>();

                var percentage = 0.0m;

                foreach (var account in postModel.Accounts)
                {
                    // Accounts will be auto-approved by quote approver
                    account.ApprovedById = currentUser.Id;
                    account.Project = newProject;
                    account.ApprovedOn = DateTime.UtcNow;
                    percentage += account.Percentage;
                    if (account.Percentage < 0)
                    {
                        return BadRequest("Negative Percentage Detected");
                    }
                    newProject.Accounts.Add(account); 
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
                    expense.Project = newProject;
                    expense.InvoiceId = null;
                    expense.Account = allRates.Single(a => a.Id == expense.RateId).Account;
                    expense.IsPassthrough = allRates.Single(a => a.Id == expense.RateId).IsPassthrough;
                    newProject.Expenses.Add(expense);
                }



                await _dbContext.Projects.AddAsync(newProject);
                await _dbContext.SaveChangesAsync();

                var quote = new Quote();
                quote.InitiatedById = currentUser.Id;
                quote.CreatedDate = DateTime.UtcNow;
                quote.Project = newProject;
                quote.ProjectId = newProject.Id;
                quote.Total = (decimal)Math.Round(postModel.Quote.GrandTotal, 2);


                var activities = new List<Activity>();
                foreach (var activity in postModel.Quote.Activities)
                {
                    if (activity.Total > 0)
                    {
                        var workItems = new List<WorkItem>();
                        foreach (var wi in activity.WorkItems)
                        {
                            if (wi.Total > 0)
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


                await _dbContext.Quotes.AddAsync(quote);
                await _historyService.AdhocProjectCreated(newProject);
                await _dbContext.SaveChangesAsync();

                await _emailService.AdhocProjectCreated(newProject);


                await txn.CommitAsync();
                return Ok(newProject);
            }
        }
    }
}
