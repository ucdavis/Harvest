using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Models;
using Harvest.Core.Services;
using Harvest.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Harvest.Web.Controllers.Api
{
    [Authorize]
    public class LinkController : SuperController
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly IApiKeyService _apiKeyService;
        public LinkController(AppDbContext dbContext, IUserService userService, IApiKeyService apiKeyService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _apiKeyService = apiKeyService;
        }

        [HttpGet]
        [Authorize(Policy = AccessCodes.WorkerAccess)]
        [Route("api/{team}/link")]
        public async Task<IActionResult> Get()
        {
            var user = await _userService.GetCurrentUser();

            var validRoles = new List<string> { Role.Codes.Worker, Role.Codes.FieldManager, Role.Codes.Supervisor };


            var permission = await _dbContext.Permissions
                .Where(p => p.UserId == user.Id && p.Team.Slug == TeamSlug && validRoles.Contains(p.Role.Name))
                .SingleOrDefaultAsync();
            if (permission == null)
            {
                return NotFound();
            }
            permission.Token = Guid.NewGuid();
            permission.TokenExpires = DateTime.UtcNow.AddMinutes(5);

            await _dbContext.SaveChangesAsync();

            return Ok(permission.Token);
        }

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [Route("api/getapi/{id}")]
        public async Task<IActionResult> GetApi(Guid id)
        {
            var permission = await _dbContext.Permissions
                .Include(p => p.Team)
                .Where(p => p.Token == id && p.TokenExpires > DateTime.UtcNow)
                .SingleOrDefaultAsync();
            if (permission == null)
            {
                return NotFound();
            }
            var apiKey = await _apiKeyService.GenerateApiKeyAsync(permission.Id);
            permission.Token = null;
            permission.TokenExpires = null;
            await _dbContext.SaveChangesAsync();
            return Ok(new { apiKey, team = permission.Team.Slug });
        }

        [HttpGet]
        [Route("/api/Link/GetTeam")]
        public async Task<IActionResult> GetTeam()
        {
            var user = await _userService.GetCurrentUser();
            var permissions = await _dbContext.Permissions
                .Include(p => p.Team)
                .Where(p => p.UserId == user.Id && p.Role.Name == Role.Codes.Worker)
                .ToListAsync();

            if (permissions.Count == 0)
            {
                return NotFound("No worker permissions found");
            }

            if (permissions.Count == 1)
            {
                return Ok(permissions.Single().Team.Slug);
            }

            // Multiple permissions found - ambiguous team selection
            return BadRequest("Multiple worker permissions found - cannot determine single team");

        }

        [HttpGet]
        [Route("/mobileToken")]
        [Route("/mobileToken/{team?}")]
        public async Task<IActionResult> CheckTeamAccess(string team = null)
        {
            var user = await _userService.GetCurrentUser();
            var validRoles = new List<string> { Role.Codes.Worker, Role.Codes.FieldManager, Role.Codes.Supervisor };

            // If a specific team slug is provided, use it directly
            if (!string.IsNullOrEmpty(team))
            {
                // Verify the user has appropriate permissions for this team
                

                var hasPermission = await _dbContext.Permissions
                    .Include(p => p.Team)
                    .Include(p => p.Role)
                    .AnyAsync(p => p.UserId == user.Id &&
                                  p.Team.Slug == team &&
                                  validRoles.Contains(p.Role.Name));

                if (hasPermission)
                {
                    return Redirect($"/{team}/mobile/token");
                }
                else
                {
                    // User doesn't have permission for the specified team, redirect to team selector with mobile parameter
                    return Redirect("/team?mobile=token");
                }
            }


            var permissions = await _dbContext.Permissions
                .Include(p => p.Team)
                .Include(p => p.Role)
                .Where(p => p.UserId == user.Id && validRoles.Contains(p.Role.Name))
                .ToListAsync();

            // Group by team to get unique teams where user has qualifying permissions
            var teamsWithAccess = permissions
                .GroupBy(p => p.Team.Slug)
                .Select(g => g.Key)
                .ToList();

            if (teamsWithAccess.Count == 1)
            {
                // Single team - redirect to mobile token page
                var singleTeamSlug = teamsWithAccess.Single();
                return Redirect($"/{singleTeamSlug}/mobile/token");
            }


            return Redirect("/team?mobile=token");

        }

        //Can use this for testing
        //[HttpGet]
        //[AllowAnonymous]
        //[Route("api/validate/{id}")]
        //public async Task<IActionResult> Validate(string id)
        //{
        //    var permission = await _apiKeyService.ValidateApiKeyAsync(id);
        //    if (permission == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(new { valid = true });
        //}

    }
}
