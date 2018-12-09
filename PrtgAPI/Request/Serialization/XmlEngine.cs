using System.Xml;

namespace PrtgAPI.Request.Serialization
{
    class XmlEngine
    {
        IXmlSerializer serializer;

        public XmlEngine(IXmlSerializer serializer)
        {
            this.serializer = serializer;
        }

        internal TableData<T> DeserializeTable<T>(XmlReader reader, bool validateValueTypes = true)
        {
            return serializer.Deserialize<TableData<T>>(reader, validateValueTypes);
        }

        internal T DeserializeObject<T>(XmlReader reader)
        {
            return serializer.Deserialize<T>(reader, true);
        }

        internal object DeserializeObjectProperty(ObjectProperty property, string rawValue)
        {
            return serializer.DeserializeObjectProperty(property, rawValue);
        }

        internal object DeserializeObjectProperty(ObjectPropertyInternal property, string rawValue)
        {
            return serializer.DeserializeObjectProperty(property, rawValue);
        }
    }
}
