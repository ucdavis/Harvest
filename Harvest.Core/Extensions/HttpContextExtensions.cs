using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Harvest.Core.Extensions
{
    public static class HttpContextExtensions
    {
        public static int? GetProjectId(this IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            var projectIdString =  (string)httpContext.Request.Query["projectId"] ?? httpContext.GetRouteValue("projectId") as string;

            if (int.TryParse(projectIdString, out int projectId))
            {
                return projectId;
            }

            return null;
        }

        public static Guid? GetProjectShareId(this IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }
            var projectShareGuidString = (string)httpContext.Request.Query["shareId"] ?? httpContext.GetRouteValue("shareId") as string;
            if (Guid.TryParse(projectShareGuidString, out Guid projectShareGuid))
            {
                return projectShareGuid;
            }
            return null;
        }

        public static string GetTeam(this IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            return (string)httpContext.Request.Query["team"] ?? httpContext.GetRouteValue("team") as string;
        }
    }
}