using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum SNMPOverflowMode
    {
        [XmlEnum("0")]
        Ignore,

        [XmlEnum("1")]
        DontIgnore
    }
}
