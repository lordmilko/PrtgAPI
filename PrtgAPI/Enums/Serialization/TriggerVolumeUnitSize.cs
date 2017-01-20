using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies unit sizes to be used in <see cref="TriggerType.Volume"/> notification triggers.
    /// </summary>
    public enum TriggerVolumeUnitSize
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
