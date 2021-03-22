using System;
using System.Collections.Generic;
using System.Text;
using Humanizer;

namespace Harvest.Core.Extensions
{
    public static class StringExtensions
    {
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
    }
}
