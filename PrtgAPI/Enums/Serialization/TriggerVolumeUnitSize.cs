using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
