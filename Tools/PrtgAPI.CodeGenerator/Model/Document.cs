using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator.Model
{
    internal class Document
    {
        public ReadOnlyCollection<Template> Templates { get; }

        public Methods Methods { get; }

        public Dictionary<string, string> Resources { get; }

        public Document(DocumentXml doc)
        {
            Templates = doc.Templates.Select(t => new Template(t)).ToReadOnlyList();
            Methods = new Methods(doc.Methods);
            Resources = XElement.Parse(doc.ResourcesXml.OuterXml).Elements().ToDictionary(e => e.Name.LocalName, e => e.Value);
        }
    }
}