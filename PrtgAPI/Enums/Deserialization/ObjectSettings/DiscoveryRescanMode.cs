using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum DiscoveryRescanMode
    {
        [XmlEnum("1")]
        SkipKnownIPs,

        [XmlEnum("0")]
        RescanKnownIPs
    }
}
