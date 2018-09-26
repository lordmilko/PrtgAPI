using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace PrtgAPI.CodeGenerator.Xml
{
    [XmlRoot("Document")]
    public class DocumentXml
    {
        [XmlArray("Templates")]
        [XmlArrayItem("Template")]
        public List<TemplateXml> Templates { get; set; }

        [XmlElement("Methods")]
        public MethodsXml Methods { get; set; }

        [XmlAnyElement("Resources")]
        public XmlElement ResourcesXml { get; set; }

        [XmlArray("CommonParameters")]
        [XmlArrayItem("CommonParameter")]
        public List<CommonParameterXml> CommonParameters { get; set; }
    }
}
