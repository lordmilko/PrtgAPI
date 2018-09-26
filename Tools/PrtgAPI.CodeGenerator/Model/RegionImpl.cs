using System.Collections.ObjectModel;
using System.Linq;
using PrtgAPI.CodeGenerator.CSharp;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator.Model
{
    internal class RegionImpl
    {
        public string Name { get; }

        public bool GroupOverloads { get; }

        public ReadOnlyCollection<RegionImpl> Regions { get; }

        public ReadOnlyCollection<MethodImpl> MethodImpls { get; }

        public ReadOnlyCollection<InlineMethodDef> InlineMethodDefs { get; }

        public RegionImpl(RegionImplXml regionImpl)
        {
            Name = regionImpl.Name;
            GroupOverloads = regionImpl.GroupOverloads;
            Regions = regionImpl.Regions.Select(r => new RegionImpl(r)).ToReadOnlyList();
            MethodImpls = regionImpl.MethodImpls.Select(m => new MethodImpl(m)).ToReadOnlyList();
            InlineMethodDefs = regionImpl.InlineMethodDefs.Select(m => new InlineMethodDef(m)).ToReadOnlyList();
        }

        public Region Serialize(DocumentConfig documentConfig)
        {
            var regions = Regions.Select(r => r.Serialize(documentConfig)).ToList();

            var implMethodsAndRegions = MethodImpls.Select(m => MethodImpl.Serialize(m, documentConfig)).ToList();

            regions.AddRange(implMethodsAndRegions.Select(i => i.Item1).Where(v => v != null));

            var implMethods = implMethodsAndRegions.SelectMany(i => i.Item2).ToList();
            var inlineMethods = InlineMethodDefs.SelectMany(m => m.Serialize(m, documentConfig)).ToList();

            return new Region(Name, regions.ToReadOnlyList(), implMethods.Union(inlineMethods).ToReadOnlyList(), GroupOverloads);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}