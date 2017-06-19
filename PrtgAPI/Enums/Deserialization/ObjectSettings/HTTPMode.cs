using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum HTTPMode
    {
        [XmlEnum("0")]
        HTTPS,

        [XmlEnum("1")]
        HTTP
    }
}
