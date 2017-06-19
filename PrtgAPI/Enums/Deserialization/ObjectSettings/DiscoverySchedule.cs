using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum DiscoverySchedule
    {
        [XmlEnum("0")]
        Once,

        [XmlEnum("1")]
        Hourly,

        [XmlEnum("2")]
        Daily,

        [XmlEnum("3")]
        Weekly
    }
}
