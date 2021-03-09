using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

public class LogUserNameMiddleware
{
    private readonly RequestDelegate next;

    public LogUserNameMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("User", context.User.Identity.Name ?? "anonymous"))
        {
            await next(context);
        }
    }
}