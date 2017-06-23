using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum LinuxLoginMode
    {
        [XmlEnum("0")]
        Password,

        [XmlEnum("1")]
        PrivateKey
    }
}
