using System;
using System.Xml;
using System.Xml.Linq;
using PrtgAPI.Parameters.Helpers;

namespace PrtgAPI.Request.Serialization
{
    internal class XmlReflectionSerializer : IXmlSerializer
    {
        public T Deserialize<T>(XmlReader reader, bool validateValueTypes = true)
        {
            return (T)new XmlReflectionSerializerImpl(typeof(T)).Deserialize(XDocument.Load(reader), null, validateValueTypes);
        }

        public object DeserializeObjectProperty(ObjectProperty property, string rawValue) =>
            DeserializeObjectPropertyInternal(property, rawValue);

        public object DeserializeObjectProperty(ObjectPropertyInternal property, string rawValue) =>
            DeserializeObjectPropertyInternal(property, rawValue);

        private object DeserializeObjectPropertyInternal(Enum property, string rawValue)
        {
            var cache = ObjectPropertyParser.GetPropertyInfoViaTypeLookup(property);
            var rawName = ObjectPropertyParser.GetObjectPropertyNameViaCache(property, cache);

            return XmlReflectionSerializerImpl.DeserializeRawPropertyValue(property, rawName, rawValue);
        }

        public void Update<T>(XmlReader reader, T target)
        {
            var serializer = new XmlReflectionSerializerImpl(target.GetType());

            serializer.DeserializeExisting(XDocument.Load(reader), target);
        }
    }
}
