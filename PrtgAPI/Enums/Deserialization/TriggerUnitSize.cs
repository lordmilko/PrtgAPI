using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        [Description("bit")]
        Bit,

        /// <summary>
        /// Kilobits
        /// </summary>
        [Description("kbit")]
        Kbit,

        /// <summary>
        /// Megabits
        /// </summary>
        Mbit,

        /// <summary>
        /// Gigabits
        /// </summary>
        Gbit,

        /// <summary>
        /// Terabits
        /// </summary>
        Tbit,

        /// <summary>
        /// Bytes
        /// </summary>
        Byte,

        /// <summary>
        /// Kilobytes
        /// </summary>
        KByte,

        /// <summary>
        /// Megabytes
        /// </summary>
        MByte,

        /// <summary>
        /// Gigabytes
        /// </summary>
        GByte,

        /// <summary>
        /// Terabytes
        /// </summary>
        TByte
    }
}
