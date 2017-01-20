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
    /// Specifies data transfer time components for <see cref="TriggerType.Speed"/> notification triggers. 
    /// </summary>
    public enum TriggerUnitTime
    {
        /// <summary>
        /// Seconds
        /// </summary>
        [XmlEnum("1")]
        [Description("s")]
        Sec,

        /// <summary>
        /// Minutes
        /// </summary>
        [XmlEnum("2")]
        [Description("m")]
        Min,

        /// <summary>
        /// Hours
        /// </summary>
        [XmlEnum("3")]
        [Description("h")]
        Hour,

        /// <summary>
        /// Days
        /// </summary>
        [XmlEnum("4")]
        [Description("d")]
        Day
    }
}
