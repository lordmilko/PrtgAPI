using System.Xml.Serialization;

namespace PrtgAPI.CodeGenerator.Xml
{
    [XmlRoot("GenericArg")]
    public class GenericArgXml : IGenericArg
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("description")]
        public string Description { get; set; }

        [XmlAttribute("constraint")]
        public string Constraint { get; set; }
    }
}
