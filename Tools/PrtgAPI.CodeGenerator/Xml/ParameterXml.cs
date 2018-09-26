using System.Xml.Serialization;

namespace PrtgAPI.CodeGenerator.Xml
{
    [XmlRoot("Parameter")]
    public class ParameterXml : IParameter
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("default")]
        public string Default { get; set; }

        [XmlAttribute("streamDefault")]
        public string StreamDefault { get; set; }

        [XmlAttribute("description")]
        public string Description { get; set; }

        [XmlAttribute("streamDescription")]
        public string StreamDescription { get; set; }

        [XmlAttribute("streamOnly")]
        public bool StreamOnly { get; set; }

        [XmlAttribute("excludeStream")]
        public bool ExcludeStream { get; set; }

        [XmlAttribute("tokenOnly")]
        public bool TokenOnly { get; set; }

        [XmlAttribute("after")]
        public string After { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
