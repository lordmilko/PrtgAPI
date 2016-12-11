using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum WmiMode
    {
        [XmlEnum("0")]
        Default,

        [XmlEnum("1")]
        Alternative
    }
}
