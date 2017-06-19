using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    public enum ProbeRestartOption
    {
        [XmlEnum("0")]
        DoNothing,

        [XmlEnum("1")]
        RestartServices,

        [XmlEnum("2")]
        RebootSystem
    }
}
