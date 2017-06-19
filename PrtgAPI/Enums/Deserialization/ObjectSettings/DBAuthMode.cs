using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum DBAuthMode
    {
        [XmlEnum("0")]
        Windows,

        [XmlEnum("1")]
        SQL
    }
}
