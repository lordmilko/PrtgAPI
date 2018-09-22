using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI.CodeGenerator.Xml
{
    [XmlRoot("Template")]
    public class TemplateXml
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("Region")]
        public List<RegionDefXml> Regions { get; set; }

        [XmlElement("MethodDef")]
        public List<MethodDefXml> MethodDefs { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
