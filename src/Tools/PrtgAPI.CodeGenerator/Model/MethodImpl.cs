using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PrtgAPI.CodeGenerator.CSharp;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator.Model
{
    internal class MethodImpl : IMethodImpl
    {
        MethodImplXml methodImplXml;

        public string Name => methodImplXml.Name;

        public string Type => methodImplXml.Type;

        public string Description => methodImplXml.Description;

        public string ExtendedDescription => methodImplXml.ExtendedDescription;

        public string Template => methodImplXml.Template;

        public bool Stream => methodImplXml.Stream;

        public bool Query => methodImplXml.Query;

        public bool Region => methodImplXml.Region;

        public bool PluralRegion => methodImplXml.PluralRegion;

        public ReadOnlyCollection<RegionDef> Regions { get; }

        public ReadOnlyCollection<MethodDef> MethodDefs { get; }

        public MethodImpl(MethodImplXml methodImplXml)
        {
            this.methodImplXml = methodImplXml;

            Regions = methodImplXml.Regions.Select(r => new RegionDef(r)).ToReadOnlyList();
            MethodDefs = methodImplXml.Methods.Select(m => new MethodDef(m)).ToReadOnlyList();
        }

        public MethodType[] Types
        {
            get
            {
                if (Stream)
                    return new[] { MethodType.Synchronous, MethodType.Asynchronous, MethodType.Stream };

                return new[] { MethodType.Synchronous, MethodType.Asynchronous };
            }
        }

        public static Tuple<Region, List<Method>> Serialize(IMethodImpl method, DocumentConfig documentConfig)
        {
            var template = documentConfig.Templates.FirstOrDefault(t => t.Name == method.Template);

            if (template == null)
                throw new InvalidOperationException($"Template '{method.Template}' does not exist");

            template = new TemplateEvaluator(method, template, documentConfig.Templates).ResolveAll();

            var finalRegions = template.Regions.Select(r => r.Serialize(method, documentConfig)).Where(r => r != null).ToReadOnlyList();
            var finalMethods = template.MethodDefs.SelectMany(m => m.Serialize(method, documentConfig)).ToList();

            if (method.Region)
            {
                var regionName = (method.Description ?? method.Name);

                if (method.PluralRegion)
                    regionName += "s";

                return Tuple.Create(new Region(regionName, finalRegions, finalMethods.ToReadOnlyList(), false), new List<Method>());
            }
            else
                return Tuple.Create<Region, List<Method>>(null, finalMethods);
        }

        public string GetValue(string value, bool spaces)
        {
            if (value == null)
                return null;

            if (ExtendedDescription != null)
                value = value.Replace("{extendedDescription}", ExtendedDescription);
            else
                value = value.Replace("{extendedDescription}", "{description}");

            var chars = new List<char>
            {
                '\t', '\r', '\n'
            };

            if (spaces)
                chars.Add(' ');

            var newVal = value.Replace("{Name}", Name).Replace("{name}", Name.ToLower()).Replace("{type}", Type).Replace("{type}", Type).Trim(chars.ToArray());

            if (!string.IsNullOrEmpty(Description))
                newVal = newVal.Replace("{description}", Description.ToLower());
            else
                newVal = newVal.Replace("{description}", Name.ToLower());

            return newVal;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}