using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Specifies time spans for filtering records by.</para>
    /// </summary>
    public enum RecordAge
    {
        /// <summary>
        /// Records that were created today.
        /// </summary>
        [XmlEnum("today")]
        Today,

        /// <summary>
        /// Records that were created yesterday.
        /// </summary>
        [XmlEnum("yesterday")]
        Yesterday,

        /// <summary>
        /// Records that were created within the last 7 days.
        /// </summary>
        [XmlEnum("7days")]
        LastWeek,

        /// <summary>
        /// Records that were created within the last 30 days.
        /// </summary>
        [XmlEnum("30days")]
        LastMonth,

        /// <summary>
        /// Records that were created within the last 6 months.
        /// </summary>
        [XmlEnum("6months")]
        LastSixMonths,

        /// <summary>
        /// Records that were created within the last 12 months.
        /// </summary>
        [XmlEnum("12months")]
        LastYear,

        /// <summary>
        /// All records present in the PRTG Server. By default, PRTG only stores 30 days.
        /// </summary>
        All
    }
}
