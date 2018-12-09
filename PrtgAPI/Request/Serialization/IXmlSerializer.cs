using System.Xml;

namespace PrtgAPI.Request.Serialization
{
    interface IXmlSerializer
    {
        T Deserialize<T>(XmlReader reader, bool validateValueTypes);

        object DeserializeObjectProperty(ObjectProperty property, string rawValue);

        object DeserializeObjectProperty(ObjectPropertyInternal property, string rawValue);

        void Update<T>(XmlReader reader, T target);
    }
}
