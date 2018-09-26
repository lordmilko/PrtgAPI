using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace PrtgAPI.CodeGenerator.Xml
{
    [XmlRoot("MethodDef")]
    public class MethodDefXml : IMethodDef
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("returns")]
        public string ReturnType { get; set; }

        [XmlAnyElement("ReturnDescription")]
        public XmlElement ReturnDescription { get; set; }

        [XmlAttribute("template")]
        public string Template { get; set; }

        [XmlAttribute("parameters")]
        public string TemplateParametersRaw { get; set; }

        [XmlAttribute("overload")]
        public string Overload { get; set; }

        [XmlAttribute("after")]
        public string After { get; set; }

        [XmlAnyElement("Summary")]
        public XmlElement Summary { get; set; }

        [XmlElement("Exception", typeof(ExceptionXml))]
        public ExceptionXml Exception { get; set; }

        IException IMethodDef.Exception => Exception;

        [XmlElement("GenericArg")]
        public List<GenericArgXml> GenericArgs { get; set; }

        IList<IGenericArg> IMethodDef.GenericArgs => GenericArgs.Cast<IGenericArg>().ToList();

        [XmlElement("Parameter")]
        public List<ParameterXml> Parameters { get; set; }

        IList<IParameter> IMethodDef.Parameters => Parameters.Cast<IParameter>().ToList();

        [XmlAnyElement("Body")]
        public XmlElement BodyElement { get; set; }

        [XmlAnyElement("SyncBody")]
        public XmlElement SyncBodyElement { get; set; }

        [XmlAnyElement("AsyncBody")]
        public XmlElement AsyncBodyElement { get; set; }

        [XmlAnyElement("SyncAsyncBody")]
        public XmlElement SyncAsyncBodyElement { get; set; }

        [XmlAnyElement("StreamBody")]
        public XmlElement StreamBodyElement { get; set; }

        [XmlAnyElement("TokenBody")]
        public XmlElement TokenBodyElement { get; set; }

        [XmlAttribute("needsStream")]
        public bool NeedsStream { get; set; }

        [XmlAttribute("needsAsync")]
        public bool NeedsAsync { get; set; } = true;

        [XmlAttribute("tokenMode")]
        public TokenMode TokenMode { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public MethodDefXml GetMethodDefXml()
        {
            return this;
        }
    }
}
