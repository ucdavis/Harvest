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
    [Authorize(AuthenticationSchemes = "ApiKey", Policy = "ApiKey")]
    public class MobileController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IUserService _userService;

        public MobileController(AppDbContext appDbContext, IUserService userService)
        {
            _dbContext = appDbContext;
            _userService = userService;
        }

        public async Task<IActionResult> Projects()
        {
            // Get team ID from claims set by authentication handler
            var teamIdClaim = User.Claims.FirstOrDefault(c => c.Type == "TeamId");
            if (teamIdClaim == null || !int.TryParse(teamIdClaim.Value, out var teamId))
            {
                return Unauthorized("Team information not found");
            }

            var projects = await _dbContext.Projects
                .Where(p => p.TeamId == teamId && p.IsActive && p.Status == Project.Statuses.Active)
                .Select(p => new
                {
                    p.Id,
                    p.Name,                    
                })
                .ToListAsync();
            
            return Ok(projects);
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
            
            // Get additional information from claims
            var permissionIdClaim = User.Claims.FirstOrDefault(c => c.Type == "PermissionId");
            var teamSlugClaim = User.Claims.FirstOrDefault(c => c.Type == "TeamSlug");
            
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
                PermissionId = permissionIdClaim?.Value,
                TeamSlug = teamSlugClaim?.Value,
                AuthenticationType = User.Identity?.AuthenticationType,
                IsAuthenticated = User.Identity?.IsAuthenticated,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
    }
}
