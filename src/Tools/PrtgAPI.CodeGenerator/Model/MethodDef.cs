using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using PrtgAPI.CodeGenerator.CSharp;
using PrtgAPI.CodeGenerator.MethodBuilder;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator.Model
{
    internal class MethodDef : IMethodDef, IInsertableDefinition, IElementDef
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

        public XmlElement TokenBodyElement { get; }

        public bool NeedsStream { get; }

        public bool NeedsAsync { get; }

        public string[] TemplateParameters => TemplateParametersRaw?.Split(',');

        public string After => methodDefXml.After;

        public TokenMode TokenMode { get; }

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

        private XElement tokenBody;

        public XElement TokenBody
        {
            get
            {
                if (tokenBody == null)
                    tokenBody = XElement.Parse(TokenBodyElement.OuterXml);

                return tokenBody;
            }
        }


        public MethodDef(MethodDefXml methodDefXml) : this(methodDefXml, name: null)
        {
        }

        private MethodDef(IMethodDef original, string name = null, string returnType = null, XmlElement summary = null, XmlElement returnDescription = null, IException exception = null,
            ReadOnlyCollection<GenericArg> genericArgs = null, ReadOnlyCollection<Parameter> parameters = null, XmlElement bodyElement = null, XmlElement syncBodyElement = null,
            XmlElement asyncBodyElement = null, XmlElement syncAsyncBodyElement = null, XmlElement streamBodyElement = null,
            XmlElement tokenBodyElement = null, string overload = null, bool? needsStream = null, bool? streamSerial = null, bool? needsAsync = null, TokenMode tokenMode = TokenMode.None)
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
            TokenBodyElement = tokenBodyElement ?? (XmlElement)original.TokenBodyElement?.CloneNode(true);
            Overload = overload ?? original.Overload;
            NeedsStream = needsStream.HasValue ? needsStream.Value : original.NeedsStream;
            NeedsAsync = needsAsync.HasValue ? needsAsync.Value : original.NeedsAsync;
            TokenMode = tokenMode != TokenMode.None ? tokenMode : original.TokenMode;
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
                customDef.TokenBodyElement,
                customDef.Overload,
                tokenMode: customDef.TokenMode
            );
        }

        private static List<Parameter> MergeParameters(MethodDef original, MethodDef customDef)
        {
            var parameters = original.Parameters.ToList();

            if (customDef.Parameters != null)
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

        public List<Method> Serialize(IMethodImpl method, DocumentConfig documentConfig, IRegion region = null)
        {
            List<Method> methods = new List<Method>();

            foreach (var type in method.Types)
            {
                var methodConfig = new MethodConfig(method, this, type, documentConfig, region);

                if (!methodConfig.MethodRequired)
                    continue;

                methods.Add(BuildMethod(methodConfig));

                //Query methods don't require any other request types
                if (methodConfig.MethodType == MethodType.Query || methodConfig.MethodType == MethodType.Watch)
                    break;

                if (methodConfig.NeedsTokenInterfaceOverload)
                {
                    var tokenConfig = methodConfig.GetTokenOverloadConfig();

                    methods.Add(BuildMethod(tokenConfig));
                }
            }

            return methods;
        }

        private Method BuildMethod(MethodConfig methodConfig)
        {
            var w = new SourceWriter();

            MethodDef def = this;

            var builder = new MethodRunner(methodConfig, w);

            builder.WriteMethod();

            return new Method(methodConfig.Method.Name, methodConfig.MethodType, w.ToString());
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