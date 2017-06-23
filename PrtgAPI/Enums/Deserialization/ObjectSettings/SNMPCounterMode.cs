using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum SNMPCounterMode
    {
        [XmlEnum("0")]
        Use64BitIfAvailable,

        [XmlEnum("1")]
        Use32BitOnly
    }
}
