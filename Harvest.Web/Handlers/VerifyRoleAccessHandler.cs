using System;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
using Harvest.Core.Extensions;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace Harvest.Web.Handlers
{
    public class VerifyRoleAccessHandler : AuthorizationHandler<VerifyRoleAccess>
    {
        private readonly AppDbContext _dbContext;

        private readonly IHttpContextAccessor _httpContext;

        public VerifyRoleAccessHandler(AppDbContext dbContext, IHttpContextAccessor httpContext)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            VerifyRoleAccess requirement)
        {
            var userIamId = context.User.Claims.SingleOrDefault(c => c.Type == UserService.IamIdClaimType)?.Value;

            if (string.IsNullOrWhiteSpace(userIamId))
            {
                return;
            }

            var nonPiRequirements = requirement.RoleStrings.Where(r => r != Role.Codes.PI);

            var projectId = _httpContext.GetProjectId();
            var shareId = _httpContext.GetProjectShareId();

            // if we have a project context, we need to check if the user is a PI on that project or has a valid team role, depending on the requirement
            if (projectId.HasValue)
            {
                if(shareId.HasValue)
                {
                    //If using a share id, make sure it matches the project
                    if (await _dbContext.Projects.AnyAsync(a => a.Id == projectId && a.ShareId == shareId))
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
                // check for a PI
                if (requirement.RoleStrings.Contains(Role.Codes.PI))
                {
                    if (await _dbContext.Projects.AnyAsync(a =>
                            a.Id == projectId && a.PrincipalInvestigator.Iam == userIamId))
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }

                var teamIdForProject = await _dbContext.Projects
                    .Where(p => p.Id == projectId)
                    .Select(pt => pt.Team.Id)
                    .SingleAsync();

                // if the user has the requested role on the team for the project, they are good to go
                if (await _dbContext.Permissions.AnyAsync(p =>
                        p.User.Iam == userIamId && p.Team.Id == teamIdForProject &&
                        nonPiRequirements.Contains(p.Role.Name)))
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            var team = _httpContext.GetTeam();

            if (team != null)
            {
                // if we are in a team context, make sure the user has the requested role on that team
                if (await _dbContext.Permissions.AnyAsync(p =>
                        p.User.Iam == userIamId && p.Team.Slug == team &&
                        nonPiRequirements.Contains(p.Role.Name)))
                {
                    context.Succeed(requirement);
                    return;
                }
            }

            // nothing else worked so check for system role
            if (await _dbContext.Permissions.AnyAsync(p => p.User.Iam == userIamId && p.Role.Name == Role.Codes.System))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}