using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum DebugMode
    {
        [XmlEnum("0")]
        Discard,

        [XmlEnum("1")]
        WriteToDisk
    }
}
