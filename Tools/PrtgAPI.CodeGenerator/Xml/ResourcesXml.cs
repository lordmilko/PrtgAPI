using System.Xml.Serialization;

namespace PrtgAPI.CodeGenerator.Xml
{
    public class ResourcesXml
    {
        [XmlElement("StreamSummary")]
        public string StreamSummary { get; set; }

        [XmlElement("SerialSummary")]
        public string SerialSummary { get; set; }

        [XmlElement("StreamReturnDescription")]
        public string StreamReturnDescription { get; set; }
        
        [XmlElement("SerialReturnDescription")]
        public string SerialReturnDescription { get; set; }
    }
}
