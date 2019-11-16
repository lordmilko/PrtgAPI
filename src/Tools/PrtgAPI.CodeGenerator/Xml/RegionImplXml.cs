using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace PrtgAPI.CodeGenerator.Xml
{
    /// <summary>
    /// Represents a region defined outside of a template.
    /// </summary>
    [XmlRoot("Region")]
    public class RegionImplXml : IElementImplXml
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("groupOverloads")]
        public bool GroupOverloads { get; set; }

        [XmlElement("Region", typeof(RegionImplXml))]
        [XmlElement("MethodImpl", typeof(MethodImplXml))]
        [XmlElement("InlineMethodDef", typeof(InlineMethodDefXml))]
        public List<object> Elements { get; set; }

        private RegionImplXml[] Regions => Elements.OfType<RegionImplXml>().ToArray();

        private MethodImplXml[] MethodImpls => Elements.OfType<MethodImplXml>().ToArray();

        private InlineMethodDefXml[] InlineMethodDefs => Elements.OfType<InlineMethodDefXml>().ToArray();

        public override string ToString()
        {
            return Name;
        }
    }
}
