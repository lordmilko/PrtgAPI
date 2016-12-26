using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    internal class XmlEnumAlternateName : XmlEnumAttribute
    {
        internal XmlEnumAlternateName(string name) : base(name)
        {
            Name = name;
        }
    }
}
