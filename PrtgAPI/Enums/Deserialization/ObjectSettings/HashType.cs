using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum HashType
    {
        [XmlEnum("authpHMACMD596")]
        MD5,

        [XmlEnum("authpHMACSHA96")]
        SHA
    }
}
