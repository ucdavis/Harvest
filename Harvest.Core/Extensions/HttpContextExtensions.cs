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
    }
}