using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies data unit sizes that measure data volume.
    /// </summary>
    public enum DataVolumeUnit
    {
        /// <summary>
        /// Bytes
        /// </summary>
        [XmlEnum("6")]
        Byte,

        /// <summary>
        /// Kilobytes
        /// </summary>
        [XmlEnum("7")]
        KByte,

        /// <summary>
        /// Megabytes
        /// </summary>
        [XmlEnum("8")]
        MByte,

        /// <summary>
        /// Gigabytes
        /// </summary>
        [XmlEnum("9")]
        GByte,

        /// <summary>
        /// Terabytes
        /// </summary>
        [XmlEnum("10")]
        TByte
    }
}
