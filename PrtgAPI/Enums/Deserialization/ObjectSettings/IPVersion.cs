using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the Internet Protocol version to use.
    /// </summary>
    public enum IPVersion
    {
        /// <summary>
        /// Internet Protocol version 4.
        /// </summary>
        [XmlEnum("0")]
        IPv4,

        /// <summary>
        /// Internet Protocol version 6.
        /// </summary>
        [XmlEnum("1")]
        IPv6
    }
}
