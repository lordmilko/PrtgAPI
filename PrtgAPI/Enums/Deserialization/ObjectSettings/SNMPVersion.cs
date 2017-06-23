using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum SNMPVersion
    {
        [XmlEnum("V1")]
        v1,

        [XmlEnum("V2")]
        v2c,

        [XmlEnum("V3")]
        v3
    }
}
