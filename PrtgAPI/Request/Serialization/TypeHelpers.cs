using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace PrtgAPI.Request.Serialization
{
    [ExcludeFromCodeCoverage]
    static class TypeHelpers
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
                return ConvertFromPrtgDateTimeInternal(datetime);

            return null;
        }

        internal static DateTime ConvertFromPrtgDateTimeInternal(double datetime)
        {
            return DateTime.FromOADate(datetime).ToLocalTime();
        }

        internal static double ConvertToPrtgDateTime(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToOADate();
        }

        internal static DateTime StringToDate(string str)
        {
            return DateTime.ParseExact(str, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
        }

        internal static string DateToString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd-HH-mm-ss");
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
