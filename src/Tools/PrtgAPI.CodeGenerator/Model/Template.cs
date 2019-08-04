using System.Collections.ObjectModel;
using System.Linq;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator.Model
{
    internal class Template
    {
        public string Name { get; }

        public ReadOnlyCollection<RegionDef> Regions { get; }

        public ReadOnlyCollection<MethodDef> MethodDefs { get; }

        public Template(TemplateXml template)
        {
            Name = template.Name;
            Regions = template.Regions.Select(r => new RegionDef(r)).ToReadOnlyList();
            MethodDefs = template.MethodDefs.Select(m => new MethodDef(m)).ToReadOnlyList();
        }

        public Template(Template originalTemplate, ReadOnlyCollection<RegionDef> regions,
            ReadOnlyCollection<MethodDef> methods)
        {
            Name = originalTemplate.Name;
            Regions = regions;
            MethodDefs = methods;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
