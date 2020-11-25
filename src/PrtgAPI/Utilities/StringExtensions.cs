using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PrtgAPI.Utilities
{
    [ExcludeFromCodeCoverage]
    internal static class StringExtensions
    {
        public static string ForcePlural(this string str)
        {
            if (str.EndsWith("s")) //either "s" or "ies"
                return str;

            if (str.EndsWith("y"))
                return str.Substring(0, str.Length - 1) + "ies";

            return str + "s";
        }

        public static string Plural(this string str, int count)
        {
            if (count > 1 || count == 0)
                return str + "s";

            return str;
        }

        public static string Plural(this string str, IList list)
        {
            return Plural(str, list.Count);
        }

        public static string FromPlural(this string str)
        {
            if (str.EndsWith("s"))
                return str.Substring(0, str.Length - 1);

            return str;
        }

        public static string ToSentenceCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str[0].ToString().ToUpper() + str.Substring(1);
        }

        public static string EnsurePeriod(this string str)
        {
            if (str == null)
                return ".";

            if (str.EndsWith(".") || str.EndsWith("?") || str.EndsWith("!"))
                return str;

            return $"{str}.";
        }

        public static string ToQuotedList<T>(this IEnumerable<T> list)
        {
            if (list == null)
                return "''";

            return string.Join(", ", list.Select(l => $"'{l}'"));
        }

        public static string ToQuotedList(this object obj)
        {
            if (obj.IsIEnumerable())
                return obj.ToIEnumerable().ToQuotedList();

            return $"'{obj}'";
        }
    }
}
