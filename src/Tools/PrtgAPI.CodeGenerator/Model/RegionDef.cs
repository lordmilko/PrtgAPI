using System.Collections.ObjectModel;
using System.Linq;
using PrtgAPI.CodeGenerator.CSharp;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator.Model
{
    internal interface IRegion : IInsertableDefinition
    {
        MethodType Type { get; }

        bool IsTokenRegion { get; }

        bool HasTokenRegion { get; }
    }

    internal class RegionDef : IRegion
    {
        private RegionDefXml regionDefXml;

        public string Name
        {
            get
            {
                if (IsTokenRegion)
                    return regionDefXml.Name + " (Cancellation Token)";

                return regionDefXml.Name;
            }
        }

        public ReadOnlyCollection<RegionDef> Regions { get; set; }

        public ReadOnlyCollection<MethodDef> MethodDefs { get; }

        public string After => regionDefXml.After;

        public MethodType Type => regionDefXml.Type;

        public bool IsTokenRegion { get; }

        public bool HasTokenRegion { get; set; }

        public bool CancellationToken => regionDefXml.CancellationToken;

        public RegionDef(RegionDefXml region, bool tokenRegion = false)
        {
            IsTokenRegion = tokenRegion;

            regionDefXml = region;
            Regions = region.Regions.Select(r => new RegionDef(r, tokenRegion)).ToReadOnlyList();
            MethodDefs = region.MethodDefs.Select(m => new MethodDef(m)).ToReadOnlyList();
        }

        public RegionDef(RegionDef originalRegion, bool tokenRegion = false, ReadOnlyCollection<RegionDef> regions = null, ReadOnlyCollection<MethodDef> methods = null)
        {
            regionDefXml = originalRegion.regionDefXml;

            Regions = regions ?? originalRegion.Regions; ;
            MethodDefs = methods ?? originalRegion.MethodDefs;
            HasTokenRegion = originalRegion.HasTokenRegion;

            IsTokenRegion = tokenRegion;
        }

        public Region Serialize(IMethodImpl method, DocumentConfig documentConfig)
        {
            if (Type == MethodType.Query && !method.Query)
                return null;

            var regions = Regions.Select(r => r.Serialize(method, documentConfig)).ToReadOnlyList();
            var methods = MethodDefs.SelectMany(m => m.Serialize(method, documentConfig, this)).ToReadOnlyList();

            return new Region(Name, regions, methods, false);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}