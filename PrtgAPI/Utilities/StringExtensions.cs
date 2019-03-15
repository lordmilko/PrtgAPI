using System.Collections;

namespace PrtgAPI.Utilities
{
    static class StringExtensions
    {
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
    }
}
