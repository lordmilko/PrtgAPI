using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrtgAPI.Objects.Undocumented
{
    internal class IsolatedPropertySettings
    {
        [XmlElement("injected_inherittriggers")]
        public bool? InheritTriggers { get; set; }
    }
}
