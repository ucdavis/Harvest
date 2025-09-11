using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Models.ProjectModels;
using Harvest.Core.Services;
using Harvest.Web.Models.MobileModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Harvest.Web.Controllers.Api
{
    [Authorize(AuthenticationSchemes = AccessCodes.ApiKey, Policy = AccessCodes.ApiKey)]
    public class MobileController : ApiController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IProjectHistoryService _historyService;

        public MobileController(AppDbContext appDbContext, IUserService userService, IProjectHistoryService historyService)
        {
            _dbContext = appDbContext;
            _userService = userService;
            _historyService = historyService;
        }

        [HttpGet]
        [Route("api/mobile/projects")]
        public async Task<IActionResult> Projects()
        {
            // Get team ID from claims set by authentication handler
            var teamId = TeamId;
            if (teamId == null)
            {
                return Unauthorized("Team information not found");
            }

            var projects = await _dbContext.Projects
                .Where(p => p.TeamId == teamId && p.IsActive && p.Status == Project.Statuses.Active)
                .Select(ProjectMobileModel.Projection())
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet]
        [Route("api/mobile/recentprojects")]
        public async Task<IActionResult> RecentProjects()
        {
            var teamId = TeamId;
            if (teamId == null)
            {
                return Unauthorized("Team information not found");
            }
            //Find my last 5 projects I have entered expenses for.
            var user = await _userService.GetCurrentUser();
            if (user == null)
            {
                return Unauthorized("User information not found");
            }
            // Step 1: Get recent project IDs and last activity
            var recentProjectIds = await _dbContext.Expenses
                .AsNoTracking()
                .Where(e => e.Project.TeamId == teamId && e.CreatedById == user.Id && e.Project.IsActive && e.Project.Status == Project.Statuses.Active)
                .GroupBy(e => new { e.ProjectId })
                .Select(g => new
                {
                    ProjectId = g.Key.ProjectId,
                    Last = g.Max(e => e.CreatedOn)
                })
                .OrderByDescending(x => x.Last)
                .Take(5)
                .ToListAsync();

            // Step 2: Get PI names for those projects
            var projectIds = recentProjectIds.Select(x => x.ProjectId).ToList();

            var projects = await _dbContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .Select(ProjectMobileModel.Projection())
                .ToListAsync();

            // Preserve recency order from recentProjectIds
            var orderMap = recentProjectIds.ToDictionary(x => x.ProjectId, x => x.Last);
            projects = projects
                .OrderByDescending(p => orderMap[p.Id])
                .ToList();

            return Ok(projects);
        }

        [HttpGet]
        [Route("api/mobile/activerates")]
        public async Task<ActionResult> ActiveRates()
        {
            var teamId = TeamId;
            if (teamId == null)
            {
                return Unauthorized("Team information not found");
            }

            if (await _dbContext.Teams.AnyAsync(t => t.Id == teamId) == false)
            {
                return BadRequest();
            }

            var rates = await _dbContext.Rates.Where(a => a.IsActive && a.TeamId == teamId && a.Type != Rate.Types.Acreage).OrderBy(a => a.Description).Select(RatesModel.Projection()).ToArrayAsync();
            return Ok(rates);
        }

        [HttpGet]
        [Route("api/mobile/recentrates")]
        public async Task<ActionResult> RecentRates()
        {
            var teamId = TeamId;
            if (teamId == null)
            {
                return Unauthorized("Team information not found");
            }
            var user = await _userService.GetCurrentUser();
            if (user == null)
            {
                return Unauthorized("User information not found");
            }
            //Find my last 5 rates I have entered expenses for.
            var recentRateIds = await _dbContext.Expenses
                .AsNoTracking()
                .Where(e => e.Rate.TeamId == teamId && e.CreatedById == user.Id && e.Rate.IsActive)
                .GroupBy(e => e.RateId)
                .Select(g => new { RateId = g.Key, Last = g.Max(e => e.CreatedOn) })
                .OrderByDescending(x => x.Last)
                .Take(5)
                .ToListAsync();

            var rateIds = recentRateIds.Select(x => x.RateId).ToList();

            var recentRates = await _dbContext.Rates
                .AsNoTracking()
                .Where(r => rateIds.Contains(r.Id))
                .Select(RatesModel.Projection())
                .ToListAsync();

            return Ok(recentRates);
        }

        /// <summary>
        /// Example endpoint to show how user information is now available through UserService
        /// </summary>
        [HttpGet]
        [Route("api/mobile/userinfo")]
        public async Task<IActionResult> UserInfo()
        {
            // The UserService.GetCurrentUser() will now work because we set the proper claims
            var user = await _userService.GetCurrentUser();

            var teamId = TeamId;
            var teamSlug = TeamSlug;
            var permissionId = PermissionId;

            return Ok(new
            {
                User = new
                {
                    user?.Id,
                    user?.Iam,
                    user?.FirstName,
                    user?.LastName,
                    user?.Email
                },
                PermissionId = permissionId,
                TeamSlug = teamSlug,
                AuthenticationType = User.Identity?.AuthenticationType,
                IsAuthenticated = User.Identity?.IsAuthenticated,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }

        //[HttpPost]
        //[Route("api/mobile/expense/create")]
        //[Consumes(MediaTypeNames.Application.Json)]
        //public async Task<ActionResult> CreateOld(int projectId, [FromBody] Expense[] expenses)
        //{
        //    if (expenses == null || expenses.Length == 0)
        //    {
        //        return BadRequest("No expenses provided");
        //    }
        //    if (expenses.All(e => e.WorkerMobileId == null))
        //    {
        //        return BadRequest("Missing WorkerMobileId");
        //    }

        //    var wmids = expenses.Where(e => e.WorkerMobileId != null).Select(e => e.WorkerMobileId).Distinct().ToList();

        //    var existingWorkerMobileIds = await _dbContext.Expenses.Where(e => e.WorkerMobileId != null && wmids.Contains(e.WorkerMobileId.Value)).Select(e => e.WorkerMobileId).ToListAsync();


        //    var project = await _dbContext.Projects.SingleAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);
        //    if (project.Status != Project.Statuses.Active
        //        && project.Status != Project.Statuses.AwaitingCloseout
        //        && project.Status != Project.Statuses.PendingCloseoutApproval)
        //    {
        //        return BadRequest($"Expenses cannot be created for project with status of {project.Status}");
        //    }

        //    var user = await _userService.GetCurrentUser();
        //    var autoApprove = await _userService.HasAnyTeamRoles(TeamSlug, new[] { Role.Codes.FieldManager, Role.Codes.Supervisor });
        //    var allRates = await _dbContext.Rates.Where(a => a.IsActive).ToListAsync();

        //    var expensesToAdd = new List<Expense>();

        //    foreach (var expense in expenses)
        //    {
        //        if (expense.WorkerMobileId == null || existingWorkerMobileIds != null && existingWorkerMobileIds.Contains(expense.WorkerMobileId))
        //        {
        //            //Skip duplicates
        //            continue;
        //        }

        //        expense.Approved = false; //DDon't trust what we pass, or use a model instead?
        //        expense.ApprovedById = null;
        //        expense.ApprovedOn = null;
        //        expense.ApprovedBy = null;

        //        expense.CreatedBy = user;
        //        expense.CreatedOn = DateTime.UtcNow;
        //        expense.ProjectId = projectId;
        //        expense.InvoiceId = null;
        //        expense.Account = allRates.Single(a => a.Id == expense.RateId).Account;
        //        expense.IsPassthrough = allRates.Single(a => a.Id == expense.RateId).IsPassthrough;
        //        if (autoApprove)
        //        {
        //            expense.Approved = true;
        //            expense.ApprovedBy = user;
        //            expense.ApprovedOn = DateTime.UtcNow;
        //        }

        //        expensesToAdd.Add(expense);
        //    }

        //    if (expensesToAdd.Count != 0)
        //    {

        //        _dbContext.Expenses.AddRange(expensesToAdd);

        //        await _historyService.ExpensesCreated(projectId, expensesToAdd);

        //        await _dbContext.SaveChangesAsync();
        //    }

        //    var addedWorkerMobileIds = expensesToAdd
        //        .Where(e => e.WorkerMobileId != null)
        //        .Select(e => e.WorkerMobileId)
        //        .Distinct()
        //        .ToList();

        //    var skippedWorkerMobileIds = wmids
        //        .Where(id => !addedWorkerMobileIds.Contains(id))
        //        .ToList();

        //    return Ok(new
        //    {
        //        Success = true,
        //        AddedWorkerMobileIds = addedWorkerMobileIds,
        //        SkippedWorkerMobileIds = skippedWorkerMobileIds
        //    });
        //}

        [HttpPost]
        [Route("api/mobile/expense/create")]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ActionResult> Create(int projectId, [FromBody] Expense[] expenses)
        {
            if (expenses == null || expenses.Length == 0)
            {
                return BadRequest("No expenses provided");
            }
            var user = await _userService.GetCurrentUser();
            var autoApprove = await _userService.HasAnyTeamRoles(TeamSlug, new[] { Role.Codes.FieldManager, Role.Codes.Supervisor }); //Currently, this will never be true, just future proofing.
            var allRates = await _dbContext.Rates.Where(a => a.IsActive && a.TeamId == TeamId && a.Type != Rate.Types.Acreage).ToListAsync();

            var results = new CreateExpenseResultsModel();
            foreach (var expense in expenses)
            {
                var resultItem = new CreateExpenseResultItem
                {
                    WorkerMobileId = expense.WorkerMobileId,
                    UserId = user.Kerberos,
                };

                if(expense.WorkerMobileId == null)
                {
                    resultItem.Result = "Rejected";
                    resultItem.Errors = new CreateExpenseErrors
                    {
                        Field = "WorkerMobileId",
                        Code = "Missing",
                        Message = "Missing WorkerMobileId"
                    };
                    results.Summary.Rejected++;
                    results.Results.Add(resultItem);
                    continue;
                }

                var existingExpense = await _dbContext.Expenses.Include(a => a.CreatedBy).AsNoTracking().FirstOrDefaultAsync(e => e.WorkerMobileId == expense.WorkerMobileId);
                if (existingExpense != null)
                {
                    resultItem.Result = "Duplicate";
                    resultItem.ExpenseId = existingExpense.Id;
                    resultItem.UserId = existingExpense.CreatedBy.Kerberos;
                    resultItem.CreatedDate = existingExpense.CreatedOn;
                    results.Summary.Duplicate++;
                    results.Results.Add(resultItem);
                    continue;
                }

                var project = await _dbContext.Projects.SingleOrDefaultAsync(p => p.Id == projectId && p.Team.Slug == TeamSlug);

                if(project == null)
                {
                    resultItem.Result = "Rejected";
                    resultItem.Errors = new CreateExpenseErrors
                    {
                        Field = "ProjectId",
                        Code = "Invalid",
                        Message = "Invalid ProjectId"
                    };
                    results.Summary.Rejected++;
                    results.Results.Add(resultItem);
                    continue;
                }

                if (project.Status != Project.Statuses.Active
                    && project.Status != Project.Statuses.AwaitingCloseout
                    && project.Status != Project.Statuses.PendingCloseoutApproval)
                {
                    resultItem.Result = "Rejected";
                    resultItem.Errors = new CreateExpenseErrors
                    {
                        Field = "Project.Status",
                        Code = "InvalidStatus",
                        Message = $"Expenses cannot be created for project with status of {project.Status}"
                    };
                    results.Summary.Rejected++;
                    results.Results.Add(resultItem);
                    continue;
                }

                if (expense.RateId == 0 || allRates.Any(a => a.Id == expense.RateId) == false)
                {
                    resultItem.Result = "Rejected";
                    resultItem.Errors = new CreateExpenseErrors
                    {
                        Field = "RateId",
                        Code = "Invalid",
                        Message = "Invalid RateId"
                    };
                    results.Summary.Rejected++;
                    results.Results.Add(resultItem);
                    continue;
                }

                try
                {
                    expense.Approved = false; //Don't trust what we pass, or use a model instead?
                    expense.ApprovedById = null;
                    expense.ApprovedOn = null;
                    expense.ApprovedBy = null;

                    expense.CreatedBy = user;
                    expense.CreatedOn = DateTime.UtcNow;
                    expense.ProjectId = projectId;
                    expense.InvoiceId = null;
                    expense.Account = allRates.Single(a => a.Id == expense.RateId).Account;
                    expense.IsPassthrough = allRates.Single(a => a.Id == expense.RateId).IsPassthrough;
                    if (autoApprove)
                    {
                        expense.Approved = true;
                        expense.ApprovedBy = user;
                        expense.ApprovedOn = DateTime.UtcNow;
                    }

                    _dbContext.Expenses.Add(expense);
                    await _dbContext.SaveChangesAsync(); //Do it here so we can catch errors per item.

                    resultItem.Result = "Created";
                    resultItem.ExpenseId = expense.Id;
                    resultItem.CreatedDate = expense.CreatedOn;
                    results.Summary.Created++;
                    results.Results.Add(resultItem);


                    try 
                    { 
                        await _historyService.ExpensesCreated(projectId, new List<Expense> { expense }); 
                    }
                    catch 
                    {
                        //History is not critical, ignore errors.
                    }
                }
                catch (Exception ex)
                {
                    resultItem.Result = "Rejected";
                    resultItem.Errors = new CreateExpenseErrors
                    {
                        Field = "Exception",
                        Code = "Exception",
                        Message = ex.Message
                    };
                    results.Summary.Rejected++;
                    results.Results.Add(resultItem);
                    continue;
                }

            }

            return Ok(results);
        }
    }
}
