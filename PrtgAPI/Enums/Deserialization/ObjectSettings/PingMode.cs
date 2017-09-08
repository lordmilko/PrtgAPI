using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum PingMode
    {
        [XmlEnum("0")]
        SinglePing,

        [XmlEnum("1")]
        MultiPing
    }
}
