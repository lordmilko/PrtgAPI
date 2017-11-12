using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace PrtgAPI.Objects.Deserialization
{
    [ExcludeFromCodeCoverage]
    class XmlMapping
    {
        public Type Type { get; private set; }
        public PropertyInfo Property { get; private set; }
        public string[] AttributeValue { get; private set; }
        public XmlAttributeType AttributeType { get; private set; }

        internal XmlMapping(Type type, PropertyInfo info, string[] value, XmlAttributeType attributeType)
        {
            if (value.Length == 0)
                throw new ArgumentException("Array length cannot be null", nameof(value));

            Type = type;
            Property = info;
            AttributeValue = value;
            AttributeType = attributeType;
        }

        public XElement GetSingleXElementAttributeValue(XElement elm)
        {
            var value = AttributeValue.Select(a => elm.Element(a)).FirstOrDefault(x => x != null);

            return value;
        }

        public XAttribute GetSingleXAttributeAttributeValue(XElement elm)
        {
            var value = AttributeValue.Select(a => elm.Attribute(a)).FirstOrDefault(x => x != null);

            return value;
        }
    }
}