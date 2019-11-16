using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PrtgAPI.CodeGenerator.CSharp;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator.Model
{
    internal class RegionImpl : IElementImpl
    {
        public string Name { get; }

        public bool GroupOverloads { get; }

        public ReadOnlyCollection<IElementImpl> Elements { get; }

        private RegionImpl[] Regions => Elements.OfType<RegionImpl>().ToArray();

        private MethodImpl[] MethodImpls => Elements.OfType<MethodImpl>().ToArray();

        private InlineMethodDef[] InlineMethodDefs => Elements.OfType<InlineMethodDef>().ToArray();

        public RegionImpl(RegionImplXml regionImpl)
        {
            Name = regionImpl.Name;
            GroupOverloads = regionImpl.GroupOverloads;

            var elements = new List<IElementImpl>();

            foreach (var element in regionImpl.Elements)
            {
                if (element is RegionImplXml)
                    elements.Add(new RegionImpl((RegionImplXml) element));
                else if (element is MethodImplXml)
                    elements.Add(new MethodImpl((MethodImplXml) element));
                else if (element is InlineMethodDefXml)
                    elements.Add(new InlineMethodDef((InlineMethodDefXml) element));
                else
                    throw new NotImplementedException($"Don't know how to handle XML object of type '{element.GetType()}'.");
            }

            Elements = elements.ToReadOnlyList();
        }

        public Region Serialize(DocumentConfig documentConfig)
        {
            var serializedElements = new List<IElement>();

            foreach (var element in Elements)
            {
                if (element is RegionImpl)
                    serializedElements.Add(((RegionImpl) element).Serialize(documentConfig));
                else if (element is MethodImpl)
                    serializedElements.AddRange(MethodImpl.Serialize((MethodImpl) element, documentConfig));
                else if (element is InlineMethodDef)
                    serializedElements.AddRange(((InlineMethodDef) element).Serialize((InlineMethodDef)element, documentConfig));
                else
                    throw new NotImplementedException($"Don't know how to serialize object of type '{element.GetType()}'.");
            }

            return new Region(Name, serializedElements.ToReadOnlyList(), GroupOverloads);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
