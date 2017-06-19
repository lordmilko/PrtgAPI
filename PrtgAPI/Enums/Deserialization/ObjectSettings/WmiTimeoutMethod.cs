using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum WmiTimeoutMethod
    {
        [XmlEnum("1")]
        OnePointFiveTimesInterval,

        [XmlEnum("0")]
        Manual
    }
}
