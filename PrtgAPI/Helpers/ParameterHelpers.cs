using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Helpers
{
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
