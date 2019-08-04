using System.Collections.Generic;
using System.Collections.ObjectModel;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator.Model
{
    internal class InlineMethodDef : MethodDef, IMethodImpl
    {
        public InlineMethodDef(InlineMethodDefXml inlineMethodDef) : base(inlineMethodDef)
        {
        }

        public string Type { get; }
        public string Description { get; }
        public bool Stream { get; }
        public bool Query { get; }
        public bool Region { get; }
        public bool PluralRegion { get; }
        public ReadOnlyCollection<RegionDef> Regions { get; }
        public ReadOnlyCollection<MethodDef> MethodDefs { get; }
        public MethodType[] Types
        {
            get
            {
                if (Stream)
                    return new[] { MethodType.Synchronous, MethodType.Asynchronous, MethodType.Stream };

                return new[] { MethodType.Synchronous, MethodType.Asynchronous };
            }
        }

        public string GetValue(string value, bool spaces)
        {
            var chars = new List<char>
            {
                '\t', '\r', '\n'
            };

            if (spaces)
                chars.Add(' ');

            return value.Trim(chars.ToArray());
        }
    }
}