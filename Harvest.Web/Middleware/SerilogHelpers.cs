using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace Harvest.Web.Middleware
{
    public static class SerilogHelpers
    {
        public static LoggerConfiguration GetConfiguration(IConfiguration configuration)
        {
            if (configuration == null) throw new InvalidOperationException("Call Setup() before requesting a Logger Configuration"); ;

            // standard logger
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                // .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning) // uncomment this to hide EF core general info logs
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithClientIp()
                .Enrich.WithClientAgent()
                .Enrich.WithExceptionDetails();

            // various sinks
            logConfig = logConfig
                .WriteTo.Console(outputTemplate: "[{yyyy-MM-dd HH:mm:ss.fff}] [{Level}] {MachineName} <{SourceContext}> {Message}{NewLine}{Exception}");

            return logConfig;
        }

        public static LogEventLevel GetLogEventLevel(HttpContext ctx, double _, Exception ex) =>
            // currently implementing logic from SerilogApplicationBuilderExtensions.DefaultGetLevel
            ex != null
                ? LogEventLevel.Error
                : ctx.Response.StatusCode > 499
                    ? LogEventLevel.Error
                    : LogEventLevel.Information;

    }
}
