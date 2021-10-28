using System.Threading.Tasks;
using Harvest.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Context;

namespace Harvest.Web.Middleware
{
    public class LogUserNameMiddleware
    {
        private readonly RequestDelegate _next;

        public LogUserNameMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            using (LogContext.PushProperty("User", context.User.Identity.Name ?? "anonymous"))
            {
                await _next(context);
            }
        }
    }
}