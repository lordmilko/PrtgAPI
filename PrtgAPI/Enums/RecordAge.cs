using System.ComponentModel;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies time spans for filtering records by.
    /// </summary>
    public enum RecordAge
    {
        /// <summary>
        /// Records that were created today.
        /// </summary>
        Today,

        /// <summary>
        /// Records that were created yesterday.
        /// </summary>
        Yesterday,

        /// <summary>
        /// Records that were created within the last 7 days.
        /// </summary>
        [Description("7days")]
        LastWeek,

        /// <summary>
        /// Records that were created within the last 30 days.
        /// </summary>
        [Description("30days")]
        LastMonth,

        /// <summary>
        /// Records that were created within the last 6 months.
        /// </summary>
        [Description("6months")]
        LastSixMonths,

        /// <summary>
        /// Records that were created within the last 12 months.
        /// </summary>
        [Description("12months")]
        LastYear
    }
}
