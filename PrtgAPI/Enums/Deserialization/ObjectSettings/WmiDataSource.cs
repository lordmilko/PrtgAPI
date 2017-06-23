using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum WmiDataSource
    {
        [XmlEnum("1")]
        PerformanceCountersAndWMI,

        [XmlEnum("2")]
        PerformanceCounters,

        [XmlEnum("3")]
        WMI
    }
}
