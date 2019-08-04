using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how PRTG should summarize multiple notifications within a certain timespan before sending an alert.
    /// </summary>
    public enum SummaryMode
    {
        /// <summary>
        /// Always notify immediately without summarizing.
        /// </summary>
        [XmlEnum("0")]
        None,

        /// <summary>
        /// Send the first <see cref="Status.Down"/> alert immediately, summarizing all subsequent failures.
        /// </summary>
        [XmlEnum("1")]
        AfterFirstDown,

        /// <summary>
        /// Send the fisrt <see cref="Status.Down"/> and <see cref="Status.Up"/> alert immediately, summarizing all subsequent alerts.
        /// </summary>
        [XmlEnum("2")]
        AfterFirstDownUp,

        /// <summary>
        /// Summarize all alerts but alerts for <see cref="Status.Down"/>.
        /// </summary>
        [XmlEnum("3")]
        AllButDown,

        /// <summary>
        /// Summarize all alerts but alerts for <see cref="Status.Down"/> and <see cref="Status.Up"/>.
        /// </summary>
        [XmlEnum("4")]
        AllButDownAndUp,

        /// <summary>
        /// Summarize all notifications.
        /// </summary>
        [XmlEnum("5")]
        All
    }
}
