using System;
using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Data;
using Harvest.Core.Domain;
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

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, VerifyRoleAccess requirement)
        {
            var userIamId = context.User.Claims.SingleOrDefault(c => c.Type == UserService.IamIdClaimType)?.Value;

            if (string.IsNullOrWhiteSpace(userIamId))
            {
                return;
            }


            if (await _dbContext.Permissions.AnyAsync(p => p.User.Iam == userIamId && (requirement.RoleStrings.Contains(p.Role.Name)
                || p.Role.Name == Role.Codes.System))) // system admin should have access to all the things
            {
                context.Succeed(requirement);
                return;
            }

            if (requirement.RoleStrings.Contains(Role.Codes.PI))
            {
                try
                {

                    //Try to get projectId Parameter:
                    var projectString = _httpContext.HttpContext?.Request.RouteValues["projectId"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(projectString))
                    {
                        var projectId = Int32.Parse(projectString);
                        if (await _dbContext.Projects.AnyAsync(a => a.Id == projectId && a.PrincipalInvestigator.Iam == userIamId))
                        {
                            context.Succeed(requirement);
                            return;
                        }
                    }

                }
                catch
                {

                }
            }

        }
    }
}
