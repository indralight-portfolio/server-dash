using System;

namespace Common.Utility
{
    public static class StringExtensions
    {
        public static string Left(this string s, int length)
        {
            if (length > s.Length)
            {
                return s;
            }
            return s.Substring(0, length);
        }

        public static string Right(this string s, int length)
        {
            if (length > s.Length)
            {
                return s;
            }
            return s.Substring(s.Length - length, length);
        }

        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return Char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
            return str;
        }

        public static string RemoveTag(this string s)
        {
            return System.Text.RegularExpressions.Regex.Replace(s, @"<(.|\n)*?>", string.Empty);
        }
    }
}