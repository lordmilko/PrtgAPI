using System.Xml;

namespace PrtgAPI.Request.Serialization
{
    internal class XmlExpressionSerializer : IXmlSerializer
    {
        public T Deserialize<T>(XmlReader reader, bool validateValueTypes = true)
        {
            return new XmlExpressionSerializerImpl(typeof(T), reader).Deserialize<T>(validateValueTypes);
        }

        public void Update<T>(XmlReader reader, T target)
        {
            new XmlExpressionSerializerImpl(typeof(T), reader, false).Update(target);
        }

        public object DeserializeObjectProperty(ObjectProperty property, string rawValue)
        {
            var serializer = new XmlExpressionSerializerImpl(typeof(ObjectProperty), null);

            return serializer.DeserializeObjectProperty(property, rawValue);
        }

        public object DeserializeObjectProperty(ObjectPropertyInternal property, string rawValue)
        {
            var serializer = new XmlExpressionSerializerImpl(typeof(ObjectPropertyInternal), null);

            return serializer.DeserializeObjectProperty(property, rawValue);
        }
    }
}
