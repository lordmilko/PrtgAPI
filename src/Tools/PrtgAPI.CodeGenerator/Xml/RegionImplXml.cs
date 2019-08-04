using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI.CodeGenerator.Xml
{
    /// <summary>
    /// Represents a region defined outside of a template.
    /// </summary>
    [XmlRoot("Region")]
    public class RegionImplXml
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("groupOverloads")]
        public bool GroupOverloads { get; set; }

        [XmlElement("Region")]
        public List<RegionImplXml> Regions { get; set; }

        [XmlElement("MethodImpl")]
        public List<MethodImplXml> MethodImpls { get; set; }

        [XmlElement("InlineMethodDef")]
        public List<InlineMethodDefXml> InlineMethodDefs { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
