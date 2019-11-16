using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace PrtgAPI.CodeGenerator.Xml
{
    /// <summary>
    /// Represents a region defined inside a template
    /// </summary>
    [XmlRoot("Region")]
    public class RegionDefXml : IElementDefXml
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("Region", typeof(RegionDefXml))]
        [XmlElement("MethodDef", typeof(MethodDefXml))]
        public List<object> Elements { get; set; }

        private RegionDefXml[] Regions => Elements.OfType<RegionDefXml>().ToArray();

        private MethodDefXml[] MethodDefs => Elements.OfType<MethodDefXml>().ToArray();

        [XmlAttribute("after")]
        public string After { get; set; }

        [XmlAttribute("type")]
        public MethodType Type { get; set; }

        [XmlAttribute("cancellationToken")]
        public bool CancellationToken { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
