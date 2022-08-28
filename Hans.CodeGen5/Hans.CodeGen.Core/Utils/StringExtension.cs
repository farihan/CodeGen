using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hans.CodeGen.Core.Utils
{
    public static class StringExtension
    {
        public static string LoweredFirstChar(this string s)
        {
            var value = string.Empty;

            if (!string.IsNullOrEmpty(s))
                value = s.Substring(0, 1).ToLower() + s.Substring(1);

            return value;
        }

        public static string UpperedFirstChar(this string s)
        {
            var value = string.Empty;

            if (!string.IsNullOrEmpty(s))
                value = s.ToLower().Substring(0, 1).ToUpper() + s.Substring(1);

            return value;
        }

        public static string GetLoweredFirstChar(this string s)
        {
            var value = string.Empty;

            if (!string.IsNullOrEmpty(s))
                value = s.Substring(0, 1).ToLower();

            return value;
        }

        public static string GetUpperFirstChar(this string s)
        {
            var value = string.Empty;

            if (!string.IsNullOrEmpty(s))
                value = s.Substring(0, 1).ToUpper();

            return value;
        }

        public static string GetFirstWord(this string value)
        {
            var mc = Regex.Matches(value, @"(\P{Lu}+)|(\p{Lu}+\P{Lu}*)");
            var parts = new string[mc.Count];

            for (int i = 0; i < mc.Count; i++)
            {
                parts[i] = mc[i].ToString();
            }

            return LoweredFirstChar(parts[0].Replace("_", ""));
        }

        public static string ReplaceWithSpace(this string s)
        {
            var value = string.Empty;

            if (!string.IsNullOrEmpty(s))
            {
                for (int i = 0; i < s.Length; i++)
                {
                    value += " ";
                }
            }

            return value;
        }

        public static string RemoveUnderscoreAndUpperFirstLetter(this string s)
        {
            var value = string.Empty;
            var list = new List<string>();

            if (s.Contains("_"))
            {
                var words = s.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var w in words)
                {
                    var word = UpperedFirstChar(w);
                    list.Add(word);
                }

                return string.Join("", list.ToArray());
            }
            else
            {
                return s;
            }
        }

        public static string ReplaceUnderscoreWithSpace(this string s)
        {
            var value = string.Empty;
            var list = new List<string>();
            var words = s.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var w in words)
            {
                var word = UpperedFirstChar(w);
                list.Add(word);
            }

            return string.Join(" ", list.ToArray());
        }

        public static string ProcessColumn(this string s)
        {
            if (s.ToLower().EndsWith("id"))
            {
                if (s.Length == 2)
                {
                    return s.ToLower().Substring(0, 1).ToUpper() +
                        s.Substring(s.Length - 1, 1).ToLower();
                }
                else if (s.Length > 2)
                {
                    return s.ToLower().Substring(0, 1).ToUpper() +
                        s.Substring(1, s.Length - 3).ToLower() +
                        s.Substring(s.Length - 2, 1).ToUpper() +
                        s.Substring(s.Length - 1, 1).ToLower();
                }
                else
                {
                    return s;
                }
            }

            return s.ToLower();
        }

        public static string SplitCamelCase(this string input)
        {
            var list = Regex.Split(input, @"([A-Z]?[a-z]+)").Where(str => !string.IsNullOrEmpty(str));
            return string.Join(" ", list.ToArray());
        }
    }
}
