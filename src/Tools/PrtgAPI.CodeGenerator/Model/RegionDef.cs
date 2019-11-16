using System;
using System.Collections.Generic;
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

    internal class RegionDef : IRegion, IElementDef
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

        public ReadOnlyCollection<IElementDef> Elements { get; set; }

        private RegionDef[] Regions => Elements.OfType<RegionDef>().ToArray();

        private MethodDef[] Methods => Elements.OfType<MethodDef>().ToArray();

        public string After => regionDefXml.After;

        public MethodType Type => regionDefXml.Type;

        public bool IsTokenRegion { get; }

        public bool HasTokenRegion { get; set; }

        public bool CancellationToken => regionDefXml.CancellationToken;

        public RegionDef(RegionDefXml region, bool tokenRegion = false)
        {
            IsTokenRegion = tokenRegion;

            regionDefXml = region;

            var defs = new List<IElementDef>();

            foreach (var element in region.Elements)
            {
                if (element is RegionDefXml)
                    defs.Add(new RegionDef((RegionDefXml) element, tokenRegion));
                else if (element is MethodDefXml)
                    defs.Add(new MethodDef((MethodDefXml) element));
                else
                    throw new NotImplementedException($"Don't know how to process element definition of type '{element.GetType()}'.");
            }

            Elements = defs.ToReadOnlyList();
        }

        public RegionDef(RegionDef originalRegion, bool tokenRegion = false, ReadOnlyCollection<IElementDef> elements = null)
        {
            regionDefXml = originalRegion.regionDefXml;

            Elements = (elements ?? originalRegion.Elements) ?? new ReadOnlyCollection<IElementDef>(new List<IElementDef>());

            HasTokenRegion = originalRegion.HasTokenRegion;

            IsTokenRegion = tokenRegion;
        }

        public Region Serialize(IMethodImpl method, DocumentConfig documentConfig)
        {
            if (Type == MethodType.Query && !method.Query)
                return null;

            var serialized = new List<IElement>();

            foreach (var element in Elements)
            {
                if (element is RegionDef)
                    serialized.Add(((RegionDef) element).Serialize(method, documentConfig));
                else if (element is MethodDef)
                    serialized.AddRange(((MethodDef) element).Serialize(method, documentConfig, this));
                else
                    throw new NotImplementedException($"Don't know how to serialize element of type '{element.GetType()}'.");
            }

            return new Region(Name, serialized.ToReadOnlyList(), false);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}