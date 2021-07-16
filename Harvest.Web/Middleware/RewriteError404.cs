using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace Harvest.Web.Middleware
{
    public class RewriteError404 : DynamicRouteValueTransformer
    {
        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            values["controller"] = "Error";
            values["action"] = "Index";
            values["statusCode"] = 404;
            return ValueTask.FromResult(values);
        }
    }
}
