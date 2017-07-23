using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how PRTG should process overflow values.
    /// </summary>
    public enum SNMPOverflowMode
    {
        [XmlEnum("0")]
        Ignore,

        [XmlEnum("1")]
        DontIgnore
    }
}
