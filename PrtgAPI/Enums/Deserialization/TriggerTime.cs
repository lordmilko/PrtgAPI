using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies data transfer time components for <see cref="TriggerType.Speed"/> notification triggers. 
    /// </summary>
    public enum TriggerUnitTime
    {
        /// <summary>
        /// Seconds
        /// </summary>
        [Description("s")]
        Sec,

        /// <summary>
        /// Minutes
        /// </summary>
        [Description("m")]
        Min,

        /// <summary>
        /// Hours
        /// </summary>
        [Description("h")]
        Hour,

        /// <summary>
        /// Days
        /// </summary>
        [Description("d")]
        Day
    }
}
