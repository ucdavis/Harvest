using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Harvest.Web.Controllers.Api
{
    [Authorize(AuthenticationSchemes = AccessCodes.ApiKey, Policy = AccessCodes.ApiKey)]
    public class MobileController : ApiController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public MobileController(AppDbContext appDbContext, IUserService userService)
        {
            _dbContext = appDbContext;
            _userService = userService;
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
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    piName = p.PrincipalInvestigator != null ? p.PrincipalInvestigator.Name : string.Empty,
                })
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
                .Where(e => e.Project.TeamId == teamId && e.CreatedById == user.Id)
                .GroupBy(e => new { e.ProjectId, e.Project.Name })
                .Select(g => new
                {
                    ProjectId = g.Key.ProjectId,
                    Name = g.Key.Name,
                    Last = g.Max(e => e.CreatedOn)
                })
                .OrderByDescending(x => x.Last)
                .Take(5)
                .ToListAsync();

            // Step 2: Get PI names for those projects
            var projectIds = recentProjectIds.Select(x => x.ProjectId).ToList();

            var projects = await _dbContext.Projects
                .Where(p => projectIds.Contains(p.Id))
                .Include(p => p.PrincipalInvestigator)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    piName = p.PrincipalInvestigator != null ? p.PrincipalInvestigator.Name : string.Empty
                })
                .ToListAsync();

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

            var rates = await _dbContext.Rates.Where(a => a.IsActive && a.TeamId == teamId).OrderBy(a => a.Description).Select(r => new { r.Price, r.Unit, r.Type, r.Description, r.Id, r.IsPassthrough }).ToArrayAsync();
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
                .Select(e => new { e.Id, e.Description, e.Price, e.Unit, e.Type, e.IsPassthrough })
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
    }
}
