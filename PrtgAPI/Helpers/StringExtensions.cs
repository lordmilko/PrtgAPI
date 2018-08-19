using System.Collections;

namespace PrtgAPI.Helpers
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
    }
}
