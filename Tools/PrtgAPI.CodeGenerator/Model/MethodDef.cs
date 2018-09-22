using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using PrtgAPI.CodeGenerator.CSharp;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator.Model
{
    internal class MethodDef : IMethodDef, IInsertableDefinition
    {
        MethodDefXml methodDefXml;

        public string Name { get; }

        public string ReturnType { get; }

        public XmlElement ReturnDescription { get; }

        public string Template => methodDefXml.Template;

        public string TemplateParametersRaw => methodDefXml.TemplateParametersRaw;

        public string Overload { get; }

        public XmlElement Summary { get; }

        public IException Exception { get; }

        public ReadOnlyCollection<GenericArg> GenericArgs { get; }

        IList<IGenericArg> IMethodDef.GenericArgs => GenericArgs.Cast<IGenericArg>().ToList();

        public ReadOnlyCollection<Parameter> Parameters { get; }

        IList<IParameter> IMethodDef.Parameters => Parameters.Cast<IParameter>().ToList();

        public XmlElement BodyElement { get; }

        public XmlElement SyncBodyElement { get; }

        public XmlElement AsyncBodyElement { get; }

        public XmlElement SyncAsyncBodyElement { get; }

        public XmlElement StreamBodyElement { get; }

        public bool NeedsStream { get; }

        public bool NeedsAsync { get; }

        public string[] TemplateParameters => TemplateParametersRaw?.Split(',');

        public string After => methodDefXml.After;

        private XElement body;

        public XElement Body
        {
            get
            {
                if (body == null)
                    body = XElement.Parse(BodyElement.OuterXml);

                return body;
            }
        }

        private XElement syncBody;

        public XElement SyncBody
        {
            get
            {
                if (syncBody == null)
                    syncBody = XElement.Parse(SyncBodyElement.OuterXml);

                return syncBody;
            }
        }

        private XElement asyncBody;

        public XElement AsyncBody
        {
            get
            {
                if (asyncBody == null)
                    asyncBody = XElement.Parse(AsyncBodyElement.OuterXml);

                return asyncBody;
            }
        }

        private XElement syncAsyncBody;

        public XElement SyncAsyncBody
        {
            get
            {
                if (syncAsyncBody == null)
                    syncAsyncBody = XElement.Parse(SyncAsyncBodyElement.OuterXml);

                return syncAsyncBody;
            }
        }

        private XElement streamBody;

        public XElement StreamBody
        {
            get
            {
                if (streamBody == null)
                    streamBody = XElement.Parse(StreamBodyElement.OuterXml);

                return streamBody;
            }
        }

        public MethodDef(MethodDefXml methodDefXml) : this(methodDefXml, name: null)
        {
        }

        private MethodDef(IMethodDef original, string name = null, string returnType = null, XmlElement summary = null, XmlElement returnDescription = null, IException exception = null,
            ReadOnlyCollection<GenericArg> genericArgs = null, ReadOnlyCollection<Parameter> parameters = null, XmlElement bodyElement = null, XmlElement syncBodyElement = null,
            XmlElement asyncBodyElement = null, XmlElement syncAsyncBodyElement = null,
            XmlElement streamBodyElement = null, string overload = null, bool? needsStream = null, bool? streamSerial = null, bool? needsAsync = null)
        {
            methodDefXml = original.GetMethodDefXml();

            Name = name ?? original.Name;
            ReturnType = returnType ?? original.ReturnType;
            Summary = summary ?? original.Summary;
            ReturnDescription = returnDescription ?? original.ReturnDescription;
            Exception = exception ?? (original.Exception != null ? new Exception(original.Exception) : null);
            GenericArgs = genericArgs != null && genericArgs.Count > 0 ? genericArgs : original.GenericArgs?.Select(a => new GenericArg(a)).ToReadOnlyList();
            Parameters = parameters != null && parameters.Count > 0 ? parameters : original.Parameters?.Select(p => new Parameter(p)).ToReadOnlyList();
            BodyElement = bodyElement ?? (XmlElement)original.BodyElement?.CloneNode(true);
            SyncBodyElement = syncBodyElement ?? (XmlElement)original.SyncBodyElement?.CloneNode(true);
            AsyncBodyElement = asyncBodyElement ?? (XmlElement)original.AsyncBodyElement?.CloneNode(true);
            SyncAsyncBodyElement = syncAsyncBodyElement ?? (XmlElement)original.SyncAsyncBodyElement?.CloneNode(true);
            StreamBodyElement = streamBodyElement ?? (XmlElement)original.StreamBodyElement?.CloneNode(true);
            Overload = overload ?? original.Overload;
            NeedsStream = needsStream.HasValue ? needsStream.Value : original.NeedsStream;
            NeedsAsync = needsAsync.HasValue ? needsAsync.Value : original.NeedsAsync;
        }

        public static MethodDef Merge(MethodDef original, MethodDef customDef)
        {
            return new MethodDef(original,
                customDef.Name,
                customDef.ReturnType,
                customDef.Summary,
                customDef.ReturnDescription,
                customDef.Exception,
                customDef.GenericArgs,
                MergeParameters(original, customDef).ToReadOnlyList(),
                customDef.BodyElement,
                customDef.SyncAsyncBodyElement,
                customDef.AsyncBodyElement,
                customDef.SyncAsyncBodyElement,
                customDef.StreamBodyElement,
                customDef.Overload
            );
        }

        private static List<Parameter> MergeParameters(MethodDef original, MethodDef customDef)
        {
            var parameters = original.Parameters.ToList();

            if(customDef.Parameters != null)
            {
                foreach (var parameter in customDef.Parameters)
                {
                    var previousParameter = parameter.After == "*" ? parameters.LastOrDefault() : parameters.First(p => p.Name == parameter.After);
                    var index = previousParameter != null ? parameters.IndexOf(previousParameter) : -1;

                    parameters.Insert(index + 1, parameter);
                }
            }

            return parameters;
        }

        public List<Method> Serialize(IMethodImpl method, Config config, IRegion region = null)
        {
            List<Method> methods = new List<Method>();

            foreach (var type in method.Types)
            {
                if (type == MethodType.Asynchronous && !NeedsAsync)
                    continue;

                if (type == MethodType.Stream && !NeedsStream)
                    continue;

                var w = new SourceWriter();

                MethodDef def = this;

                var builder = new MethodBuilder(method, def, config, w, type);

                builder.WriteMethod();

                methods.Add(new Method(method.Name, region?.Type == MethodType.Query ? MethodType.Query : type, w.ToString()));
            }

            return methods;
        }

        public static List<PropertyInfo> GetProperties()
        {
            var properties = typeof(MethodDef).GetProperties().Where(p =>
            {
                var attribs = p.GetCustomAttributes(false);

                return attribs.Any(a =>
                    a is XmlElementAttribute || a is XmlAttributeAttribute || a is XmlAnyElementAttribute);
            });

            return properties.ToList();
        }

        public override string ToString()
        {
            if (Parameters != null)
                return Name + "(" + string.Join(", ", Parameters.Where(p => !p.StreamOnly)) + ")";

            return Name;
        }

        public MethodDefXml GetMethodDefXml()
        {
            return methodDefXml;
        }
    }
}