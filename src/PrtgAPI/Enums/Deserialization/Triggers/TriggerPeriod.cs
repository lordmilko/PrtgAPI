using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the time period for <see cref="TriggerType.Volume"/> triggers.
    /// </summary>
    public enum TriggerPeriod
    {
        /// <summary>
        /// Trigger when a volume limit is reached per hour.
        /// </summary>
        [XmlEnum("0")]
        Hour,

        /// <summary>
        /// Trigger when a volume limit is reached per day.
        /// </summary>
        [XmlEnum("1")]
        Day,

        /// <summary>
        /// Trigger when a volume limit is reached per week.
        /// </summary>
        [XmlEnum("2")]
        Week,

        /// <summary>
        /// Trigger when a volume limit is reached per month.
        /// </summary>
        [XmlEnum("3")]
        Month
    }
}
