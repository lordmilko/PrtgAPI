using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Request.Serialization
{
    [ExcludeFromCodeCoverage]
    static class DeserializationHelpers
    {
        /// <summary>
        /// Converts a <see cref="string"/> to an <see cref="int"/>. If the specified value is null or empty,
        /// returns 0.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>If the string is not null or empty, the value as an integer. Otherwise, 0.</returns>
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
        internal static DateTime? ConvertFromPrtgDateTime(double datetime)
        {
            if (datetime != 0)
                return DateTime.FromOADate(datetime).ToLocalTime();

            return null;
        }

        internal static double ConvertToPrtgDateTime(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToOADate();
        }

        /// <summary>
        /// Convert a PRTG TimeSpan to a <see cref="TimeSpan"/> object.
        /// </summary>
        /// <param name="timeSpan">PRTG TimeSpan representing the number of seconds since an event occurred. If this value is null, this method will return null.</param>
        /// <returns></returns>
        internal static TimeSpan ConvertFromPrtgTimeSpan(double timeSpan)
        {
            return TimeSpan.FromSeconds(timeSpan);
        }

        internal static double ConvertToPrtgTimeSpan(TimeSpan timeSpan)
        {
            return timeSpan.TotalSeconds;
        }
    }
}
