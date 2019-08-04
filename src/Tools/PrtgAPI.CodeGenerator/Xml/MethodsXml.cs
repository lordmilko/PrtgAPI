using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI.CodeGenerator.Xml
{
    [XmlRoot("Methods")]
    public class MethodsXml
    {
        [XmlElement("Region")]
        public List<RegionImplXml> Regions { get; set; }
    }
}
