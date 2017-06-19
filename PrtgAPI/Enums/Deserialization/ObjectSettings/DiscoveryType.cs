using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    enum DiscoveryType
    {
        [XmlEnum("0")]
        Manual,

        [XmlEnum("1")]
        AutomaticStandard,

        [XmlEnum("2")]
        AutomaticDetailed,

        [XmlEnum("3")]
        AutomaticTemplate
    }
}
