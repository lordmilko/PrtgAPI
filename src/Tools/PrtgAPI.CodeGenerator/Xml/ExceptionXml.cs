using System.Xml.Serialization;

namespace PrtgAPI.CodeGenerator.Xml
{
    [XmlRoot("Exception")]
    public class ExceptionXml : IException
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("description")]
        public string Description { get; set; }

        public override string ToString()
        {
            return Type;
        }

        public ExceptionXml GetXml()
        {
            return this;
        }
    }
}
