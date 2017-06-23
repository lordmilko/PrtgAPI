using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum SNMPPortIdentification
    {
        [XmlEnum("10")]
        Automatic,

        [XmlEnum("0")]
        UseIfAlias,

        [XmlEnum("1")]
        UseIfDescr,

        [XmlEnum("3")]
        UseIfName,

        [XmlEnum("100")]
        NoPortUpdate
    }
}
