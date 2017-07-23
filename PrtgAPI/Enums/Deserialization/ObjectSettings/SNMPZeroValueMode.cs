using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies whether spurious "0" values should be ignored for delta (difference) SNMP sensors.
    /// </summary>
    public enum SNMPZeroValueMode
    {
        /// <summary>
        /// Ignore 0 values.
        /// </summary>
        [XmlEnum("1")]
        Ignore,

        /// <summary>
        /// Don't ignore 0 values.
        /// </summary>
        [XmlEnum("0")]
        DontIgnore
    }
}
