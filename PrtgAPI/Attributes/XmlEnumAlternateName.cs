using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace PrtgAPI.Attributes
{
    internal class XmlEnumAlternateName : XmlEnumAttribute
    {
        public string Name { get; set; }

        internal XmlEnumAlternateName(string name) : base(name)
        {
            Name = name;
        }
    }
}
