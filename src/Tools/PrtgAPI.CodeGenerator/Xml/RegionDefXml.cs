using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI.CodeGenerator.Xml
{
    /// <summary>
    /// Represents a region defined inside a template
    /// </summary>
    [XmlRoot("Region")]
    public class RegionDefXml
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("Region")]
        public List<RegionDefXml> Regions { get; set; }

        [XmlElement("MethodDef")]
        public List<MethodDefXml> MethodDefs { get; set; }

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
