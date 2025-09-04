using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Harvest.Web.Handlers
{
    public class ApiKeyAuthorizationHandler : AuthorizationHandler<ApiKeyRequirement>
    {
        private readonly ILogger<ApiKeyAuthorizationHandler> _logger;

        public ApiKeyAuthorizationHandler(ILogger<ApiKeyAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
        {
            _logger.LogInformation("ApiKeyAuthorizationHandler called. AuthenticationType: {AuthType}, IsAuthenticated: {IsAuth}", 
                context.User.Identity?.AuthenticationType, 
                context.User.Identity?.IsAuthenticated);

            // Check if the user was authenticated via API key
            if (context.User.Identity?.AuthenticationType == "ApiKey")
            {
                // Additional validation could be added here if needed
                // For now, just check that we have the required claims
                var permissionIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "PermissionId");
                if (permissionIdClaim != null)
                {
                    _logger.LogInformation("ApiKey authorization succeeded for PermissionId: {PermissionId}", permissionIdClaim.Value);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning("ApiKey authentication found but no PermissionId claim");
                }
            }
            else
            {
                _logger.LogWarning("User not authenticated via ApiKey. AuthenticationType: {AuthType}", context.User.Identity?.AuthenticationType);
            }

            return Task.CompletedTask;
        }
    }
}