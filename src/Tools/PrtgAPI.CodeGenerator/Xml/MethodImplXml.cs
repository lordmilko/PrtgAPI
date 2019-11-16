using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI.CodeGenerator.Xml
{
    /// <summary>
    /// Represents the parameters required to instantiate a method defined in a template.
    /// </summary>
    [XmlRoot("MethodImpl")]
    public class MethodImplXml : IElementImplXml
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("description")]
        public string Description { get; set; }

        [XmlAttribute("extendedDescription")]
        public string ExtendedDescription { get; set; }

        [XmlAttribute("template")]
        public string Template { get; set; }

        [XmlAttribute("stream")]
        public bool Stream { get; set; }

        [XmlAttribute("query")]
        public bool Query { get; set; }

        [XmlAttribute("region")]
        public bool Region { get; set; }

        [XmlAttribute("pluralRegion")]
        public bool PluralRegion { get; set; } = true;

        [XmlElement("Region")]
        public List<RegionDefXml> Regions { get; set; }

        [XmlElement("MethodDef")]
        public List<MethodDefXml> Methods { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
