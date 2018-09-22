using System.Collections.ObjectModel;
using System.Linq;
using PrtgAPI.CodeGenerator.CSharp;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator.Model
{
    internal interface IRegion : IInsertableDefinition
    {
        MethodType Type { get; }
    }

    internal class RegionDef : IRegion
    {
        private RegionDefXml regionDefXml;

        public string Name => regionDefXml.Name;

        public ReadOnlyCollection<RegionDef> Regions { get; set; }

        public ReadOnlyCollection<MethodDef> MethodDefs { get; }

        public string After => regionDefXml.After;

        public MethodType Type => regionDefXml.Type;

        public RegionDef(RegionDefXml region)
        {
            regionDefXml = region;
            Regions = new ReadOnlyCollection<RegionDef>(region.Regions.Select(r => new RegionDef(r)).ToList());
            MethodDefs = new ReadOnlyCollection<MethodDef>(region.MethodDefs.Select(m => new MethodDef(m)).ToList());
        }

        public RegionDef(RegionDef originalRegion, ReadOnlyCollection<RegionDef> regions, ReadOnlyCollection<MethodDef> methods)
        {
            regionDefXml = originalRegion.regionDefXml;

            Regions = Regions;
            Regions = regions;
            MethodDefs = methods;
        }

        public Region Serialize(IMethodImpl method, Config config)
        {
            if (Type == MethodType.Query && !method.Query)
                return null;

            var regions = new ReadOnlyCollection<Region>(Regions.Select(r => r.Serialize(method, config)).ToList());
            var methods = new ReadOnlyCollection<Method>(MethodDefs.SelectMany(m => m.Serialize(method, config, this)).ToList());

            return new Region(Name, regions, methods, false);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}