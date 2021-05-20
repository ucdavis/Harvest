using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harvest.Core.Models
{
    public class Result<T>
    {
        public T Value { get; }
        public bool IsError { get; private set; }
        public string ErrorMessage { get; private set; }

        public Result(T value) => Value = value;

        public static implicit operator Result<T>(Result.ResultError error) => new Result<T>(default) { ErrorMessage = error.ErrorMessage, IsError = true };
    }

    // It turned out VS caused some auto-fill headaches when mistyping/fatfingering "Result<int>.Error(...". And it's kind of verbose having to always type
    // the generic argument. So adding a bit of type inference and implicit casting makes for a better experience without getting too fancy...
    public static class Result
    {
        public static ResultError Error(string errorMessage) => new ResultError(errorMessage);
        public static Result<T> Value<T>(T value) => new Result<T>(value);

        public class ResultError
        {
            public string ErrorMessage { get; }
            public ResultError(string errorMessage) => ErrorMessage = errorMessage;
        }

    }

}
