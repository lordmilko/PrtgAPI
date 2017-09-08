using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum GraphType
    {
        [XmlEnum("0")]
        Independent,

        [XmlEnum("1")]
        Stacked
    }
}
