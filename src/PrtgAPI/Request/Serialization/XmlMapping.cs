using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using PrtgAPI.Reflection.Cache;

namespace PrtgAPI.Request.Serialization
{
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{AttributeValue[0],nq}")]
    class XmlMapping
    {
        public Type Type { get; private set; }
        public PropertyCache PropertyCache { get; private set; }
        public string[] AttributeValue { get; private set; }
        public XmlAttributeType AttributeType { get; private set; }

        private XmlMapping(Type type, PropertyCache cache, string[] value, XmlAttributeType attributeType)
        {
            if (value.Length == 0)
                throw new ArgumentException("Array length cannot be null.", nameof(value));

            Type = type;
            PropertyCache = cache;
            AttributeValue = value;
            AttributeType = attributeType;
        }

        public XElement GetSingleXElementAttributeValue(XElement elm)
        {
            XElement value = null;

            for (var i = 0; i < AttributeValue.Length; i++)
            {
                var x = elm.Element(AttributeValue[i]);

                if (!string.IsNullOrEmpty(x?.Value))
                {
                    value = x;
                    break;
                }
            }

            return value;
        }

        public XAttribute GetSingleXAttributeAttributeValue(XElement elm)
        {
            XAttribute value = null;

            for (var i = 0; i < AttributeValue.Length; i++)
            {
                var x = elm.Attribute(AttributeValue[i]);

                if (!string.IsNullOrEmpty(x?.Value))
                {
                    value = x;
                    break;
                }
            }

            return value;
        }

        internal static List<XmlMapping> GetMappings(Type type)
        {
            var properties = ReflectionCacheManager.Get(type).Properties;

            var mappings = new List<XmlMapping>();

            foreach (var prop in properties)
            {
                if (FindXmlAttribute<XmlElementAttribute>(prop, mappings, type, a => a.ElementName, XmlAttributeType.Element))
                    continue;
                if (FindXmlAttribute<XmlAttributeAttribute>(prop, mappings, type, a => a.AttributeName, XmlAttributeType.Attribute))
                    continue;
                if (FindXmlAttribute<XmlTextAttribute>(prop, mappings, type, a => null, XmlAttributeType.Text))
                    continue;
            }

            return mappings;
        }

        private static bool FindXmlAttribute<TAttribute>(PropertyCache propertyCache, List<XmlMapping> mappings, Type type, Func<TAttribute, string> name, XmlAttributeType enumType) where TAttribute : Attribute
        {
            var attributes = propertyCache.GetAttributes<TAttribute>();

            if (attributes.Length > 0)
            {
                var attribs = attributes.Cast<TAttribute>();

                mappings.Add(new XmlMapping(type, propertyCache, attribs.Select(name).ToArray(), enumType));
                return true;
            }

            return false;
        }
    }
}