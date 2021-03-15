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
    }
}
