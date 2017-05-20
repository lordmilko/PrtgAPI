using System;
using System.Xml.Serialization;

namespace PrtgAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    internal class XmlEnumAlternateName : XmlEnumAttribute
    {
        internal XmlEnumAlternateName(string name) : base(name)
        {
        }
    }
}
