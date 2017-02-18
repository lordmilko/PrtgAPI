using System;

namespace PrtgAPI.Objects.Deserialization
{
    static class DeserializationHelpers
    {
        internal static bool YesNoToBool(string str)
        {
            if (str.ToLower() == "yes")
                return true;
            if (str.ToLower() == "no")
                return false;
            throw new Exception("Invalid value specified: " + str);
        }

        internal static int StrToInt(string value)
        {
            return string.IsNullOrEmpty(value) ? 0 : Convert.ToInt32(value);
        }

        internal static int? StrToNullableInt(string str)
        {
            return string.IsNullOrEmpty(str) ? null : (int?)Convert.ToInt32(str);
        }

        internal static double? StrToNullableDouble(string str)
        {
            return string.IsNullOrEmpty(str) ? null : (double?)Convert.ToDouble(str);
        }

        /// <summary>
        /// Convert a PRTG OLE Automation style DateTime to local time.
        /// </summary>
        /// <param name="datetime">An OLE Automation style DateTime.</param>
        /// <returns>If <paramref name="datetime"/> contains a value, the PRTG DateTime formatted for the local timezone. Otherwise, null.</returns>
        internal static DateTime? ConvertPrtgDateTime(double datetime)
        {
            if (datetime != 0)
                return DateTime.FromOADate((double)datetime).ToLocalTime();
            return null;
            //return null;
        }

        /// <summary>
        /// Convert a PRTG TimeSpan to a <see cref="TimeSpan"/> object.
        /// </summary>
        /// <param name="timespan">PRTG TimeSpan representing the number of seconds since an event occurred. If this value is null, this method will return null.</param>
        /// <returns></returns>
        internal static TimeSpan ConvertPrtgTimeSpan(double timespan)
        {
            return TimeSpan.FromSeconds(timespan);
        }
    }
}
