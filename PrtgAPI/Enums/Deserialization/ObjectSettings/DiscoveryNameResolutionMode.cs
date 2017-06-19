using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum DiscoveryNameResolutionMode
    {
        [XmlEnum("1")]
        UseDNSOrWMIOrSNMP,

        [XmlEnum("0")]
        UseIPAddresses
    }
}
