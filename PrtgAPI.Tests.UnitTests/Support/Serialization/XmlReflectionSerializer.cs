using System.Xml;
using System.Xml.Linq;
using PrtgAPI.Parameters;

namespace PrtgAPI.Request.Serialization
{
    internal class XmlReflectionSerializer : IXmlSerializer
    {
        public T Deserialize<T>(XmlReader reader, bool validateValueTypes = true)
        {
            return (T)new XmlReflectionSerializerImpl(typeof(T)).Deserialize(XDocument.Load(reader), null, validateValueTypes);
        }

        public object DeserializeObjectProperty(ObjectProperty property, string rawValue)
        {
            var cache = BaseSetObjectPropertyParameters<ObjectProperty>.GetPropertyInfoViaTypeLookup(property);
            var rawName = BaseSetObjectPropertyParameters<ObjectProperty>.GetParameterNameStatic(property, cache);

            return XmlReflectionSerializerImpl.DeserializeRawPropertyValue(property, rawName, rawValue);
        }

        public void Update<T>(XmlReader reader, T target)
        {
            var serializer = new XmlReflectionSerializerImpl(target.GetType());

            serializer.DeserializeExisting(XDocument.Load(reader), target);
        }
    }
}
