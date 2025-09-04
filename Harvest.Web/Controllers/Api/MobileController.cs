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
