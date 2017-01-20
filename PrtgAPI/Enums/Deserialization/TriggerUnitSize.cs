using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies unit sizes to be used in notification triggers.
    /// </summary>
    public enum TriggerUnitSize
    {
        /// <summary>
        /// Bits
        /// </summary>
        [XmlEnum("11")]
        [Description("bit")]
        Bit,

        /// <summary>
        /// Kilobits
        /// </summary>
        [XmlEnum("12")]
        [Description("kbit")]
        Kbit,

        /// <summary>
        /// Megabits
        /// </summary>
        [XmlEnum("13")]
        Mbit,

        /// <summary>
        /// Gigabits
        /// </summary>
        [XmlEnum("14")]
        Gbit,

        /// <summary>
        /// Terabits
        /// </summary>
        [XmlEnum("15")]
        Tbit,

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
