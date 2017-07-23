using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the host resolution mode to use with devices newly discovered through auto-discovery.<para/>
    /// This only applies to new devices; existing devices that are rescanned will not be affected.
    /// </summary>
    public enum DiscoveryNameResolutionMode
    {
        /// <summary>
        /// Use the DNS, WMI or SNMP name of the device (if applicable).
        /// </summary>
        [XmlEnum("1")]
        UseDNSOrWMIOrSNMP,

        /// <summary>
        /// Always use the device's IP Address.
        /// </summary>
        [XmlEnum("0")]
        UseIPAddresses
    }
}
