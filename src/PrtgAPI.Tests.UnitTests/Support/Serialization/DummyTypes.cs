using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Tests.UnitTests.Support.Serialization
{
    class CustomType
    {
        [XmlElement("propertyname")]
        [XmlElement("injected_propertyname")]
        public int Property { get; set; }
    }

    interface IDummy<T>
    {
        T Property { get; set; }
    }

    class DummyElement<T> : IDummy<T>
    {
        [XmlElement("property")]
        [StandardSplittableString]
        public T Property { get; set; }
    }

    [XmlRoot("property")]
    class DummyElementRoot
    {
        [XmlElement("value")]
        public bool Value { get; set; }
    }

    class DummyElementValueConvertableAndUnConvertable
    {
        [XmlElement("property")]
        [PropertyParameter(PrtgAPI.Property.Id)]
        public double? Property { get; set; }

        [XmlElement("propertyConvertable")]
        [PropertyParameter(PrtgAPI.Property.Uptime)]
        public double? PropertyConvertable { get; set; }
    }

    class DummyElementValueConverter<T>
    {
        [XmlElement("property")]
        [PropertyParameter(PrtgAPI.Property.Uptime)]
        public T Property { get; set; }
    }

    class DummyAttribute<T> : IDummy<T>
    {
        [XmlAttribute("property")]
        public T Property { get; set; }
    }

    class DummyText<T>
    {
        [XmlText]
        public T Value { get; set; }
    }

    class DummyUpdate<T1, T2>
    {
        [XmlElement("objid")]
        [PropertyParameter(Property.Id)]
        public T1 Property1 { get; set; }

        [XmlElement("property")]
        public T2 Property2 { get; set; }
    }

    public enum PartialMissingXmlEnum
    {
        [XmlEnum("WithAttribute")]
        WithAttribute,

        WithoutAttribute
    }
}
