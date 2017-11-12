using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace PrtgAPI.Helpers
{
    [ExcludeFromCodeCoverage]
    static class ParameterHelpers
    {
        public static DateTime StringToDate(string str)
        {
            return DateTime.ParseExact(str, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
        }

        public static string DateToString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd-HH-mm-ss");
        }
    }
}
