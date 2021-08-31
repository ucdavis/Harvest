using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Harvest.Core.Extensions;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace Harvest.Core.Models
{
    public class Result<T>
    {
        public T Value { get; }
        public bool IsError { get; private set; }
        public string Message { get; private set; }

        public Result(T value, string message = "")
        {
            Value = value;
            Message = message;
        }

        public static implicit operator Result<T>(Result.ResultError error) => new Result<T>(default) { Message = error.ErrorMessage, IsError = true };
    }

    // It turned out VS caused some auto-fill headaches when mistyping/fatfingering "Result<int>.Error(...". And it's kind of verbose having to always type
    // the generic argument. So adding a bit of type inference and implicit casting makes for a better experience without getting too fancy...
    public static class Result
    {
        public static ResultError Error(string messageTemplate, 
            LogEventLevel logLevel = LogEventLevel.Error,
            [CallerFilePath] string callerFilePath = "", 
            [CallerLineNumber] int callerLineNumber = 0)
        {
            using var _ = LogContext.PushProperty("FileName", Path.GetFileName(callerFilePath));
            LogContext.PushProperty("LineNumber", callerLineNumber);
            Log.Write(logLevel, messageTemplate);
            return new ResultError(messageTemplate);
        }

        public static ResultError Error(string messageTemplate, object prop0, object prop1, object prop2,
            LogEventLevel logLevel = LogEventLevel.Error,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            using var _ = LogContext.PushProperty("FileName", Path.GetFileName(callerFilePath));
            LogContext.PushProperty("LineNumber", callerLineNumber);
            Log.Write(logLevel, messageTemplate, prop0, prop1, prop2);
            return new ResultError(messageTemplate.FormatTemplate(prop0, prop1, prop2));
        }

        public static ResultError Error(string messageTemplate, object prop0, object prop1,
            LogEventLevel logLevel = LogEventLevel.Error,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            using var _ = LogContext.PushProperty("FileName", Path.GetFileName(callerFilePath));
            LogContext.PushProperty("LineNumber", callerLineNumber);
            Log.Write(logLevel, messageTemplate, prop0, prop1);
            return new ResultError(messageTemplate.FormatTemplate(prop0, prop1));
        }

        public static ResultError Error(string messageTemplate, object prop0,
            LogEventLevel logLevel = LogEventLevel.Error,
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            using var _ = LogContext.PushProperty("FileName", Path.GetFileName(callerFilePath));
            LogContext.PushProperty("LineNumber", callerLineNumber);
            Log.Write(logLevel, messageTemplate, prop0);
            return new ResultError(messageTemplate.FormatTemplate(prop0));
        }

        public static Result<T> Value<T>(T value, string message = "") => new Result<T>(value, message);

        public class ResultError
        {
            public string ErrorMessage { get; }
            public ResultError(string errorMessage) => ErrorMessage = errorMessage;
        }

    }

}
