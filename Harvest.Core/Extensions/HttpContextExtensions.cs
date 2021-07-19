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
            if (int.TryParse(httpContextAccessor.HttpContext?.GetRouteValue("projectId") as string, out int projectId))
            {
                return projectId;
            }

            return null;
        }
    }
}