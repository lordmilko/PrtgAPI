using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    //todo: update xmlenum

    /// <summary>
    /// Specifies data transfer time components for <see cref="TriggerType.Speed"/> notification triggers. 
    /// </summary>
    public enum TimeUnit
    {
        /// <summary>
        /// Seconds
        /// </summary>
        [XmlEnum("1")]
        [XmlEnumAlternateName("s")]
        Second,

        /// <summary>
        /// Minutes
        /// </summary>
        [XmlEnum("2")]
        [XmlEnumAlternateName("m")]
        Minute,

        /// <summary>
        /// Hours
        /// </summary>
        [XmlEnum("3")]
        [XmlEnumAlternateName("h")]
        Hour,

        /// <summary>
        /// Days
        /// </summary>
        [XmlEnum("4")]
        [XmlEnumAlternateName("d")]
        Day
    }
}
