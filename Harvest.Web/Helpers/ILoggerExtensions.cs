using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Harvest.Web.Helpers
{
    public static class ILoggerExtensions
    {
        public static IDisposable BeginScope(this ILogger logger, string scope, object value)
        {
            return logger.BeginScope(new Dictionary<string, object>()
            {
                {scope, value}
            });
        }

        public static IDisposable BeginScope(this LoggerScopes builder)
        {
            return builder.Logger.BeginScope(builder.Scopes);
        }

        public static LoggerScopes WithScope(this ILogger logger, string scope, object value)
        {
            var builder = new LoggerScopes(logger);
            builder.Scopes.Add(scope, value);
            return builder;
        }

        public static LoggerScopes WithScope(this LoggerScopes builder, string scope, object value)
        {
            builder.Scopes.Add(scope, value);
            return builder;
        }
    }

    public class LoggerScopes
    {
        internal readonly ILogger Logger;
        internal readonly Dictionary<string, object> Scopes;

        internal LoggerScopes(ILogger logger)
        {
            Logger = logger;
            Scopes = new Dictionary<string, object>();
        }
    }
}
