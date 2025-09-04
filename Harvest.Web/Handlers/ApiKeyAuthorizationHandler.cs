using System.Linq;
using System.Threading.Tasks;
using Harvest.Core.Models;
using Microsoft.AspNetCore.Authorization;


namespace Harvest.Web.Handlers
{
    public class ApiKeyAuthorizationHandler : AuthorizationHandler<ApiKeyRequirement>
    {

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
        {
            // Check if the user was authenticated via API key
            if (context.User.Identity?.AuthenticationType == AccessCodes.ApiKey)
            {
                // Additional validation could be added here if needed
                // For now, just check that we have the required claims
                var permissionIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "PermissionId");
                if (permissionIdClaim != null)
                {

                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}