using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Harvest.Core.Utilities;
using Humanizer;

namespace Harvest.Core.Extensions
{
    public static class StringExtensions
    {

        public static T Deserialize<T>(this string value) =>
            string.IsNullOrWhiteSpace(value) ? default : JsonSerializer.Deserialize<T>(value, JsonOptions.Standard);

        public static T DeserializeWithGeoJson<T>(this string value) =>
            string.IsNullOrWhiteSpace(value) ? default : JsonSerializer.Deserialize<T>(value, JsonOptions.Standard.WithGeoJson());

        // shameless copypasta from https://stackoverflow.com/a/5796793
        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }

        public static string SafeHumanizeTitle(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return value.ToLower().Humanize(LetterCasing.Title); //Lower it so all caps gets changed
        }


        public static string SafeToLower(this string value)
        {
            if (value == null)
            {
                return value;
            }

            return value.ToLower();
        }

        public static string EfContains(this string q)
        {
            return $"%{q}%";
        }

        public static string EfStartsWith(this string q)
        {
            return $"{q}%";
        }

        public static string EfEndsWith(this string q)
        {
            return $"%{q}";
        }

        public static string Truncate(this string value, int maxChars)
        {
            const string ellipses = "...";
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - ellipses.Length) + ellipses;
        }

        public static string TruncateAndAppend(this string value1, string value2, int maxChars)
        {
            var maxValue1Chars = maxChars - value2.Length;
            return maxValue1Chars > 0
                ? value1.Truncate(maxValue1Chars) + value2
                : value2;
        }

        // Naive utility for converting Serilog templates and arguments to a string
        public static string FormatTemplate(this string messageTemplate, object parameter, params object[] additionalParameters)
        {
            return FormatTemplate(messageTemplate, new[] { parameter }.Concat(additionalParameters));
        }

        // Naive utility for converting Serilog templates and arguments to a string
        public static string FormatTemplate(this string messageTemplate, IEnumerable<object> parameters)
        {
            var objects = parameters as object[] ?? parameters.ToArray();

            if (objects.Length == 0)
            {
                return messageTemplate;
            }

            if (objects.Length != Regex.Matches(messageTemplate, "{.*?}").Count)
            {
                throw new ArgumentException("Number of arguments does not match number of template parameters");
            }

            var i = 0;
            return Regex.Replace(messageTemplate, "{.*?}", _ => objects[i++].ToString());
        }

    }
}
