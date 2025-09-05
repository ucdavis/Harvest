using Microsoft.AspNetCore.Authorization;

namespace Harvest.Web.Handlers
{
    public class ApiKeyRequirement : IAuthorizationRequirement
    {
        public ApiKeyRequirement()
        {
        }
    }
}