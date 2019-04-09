using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using PrtgAPI.CodeGenerator.MethodBuilder.Model;

namespace PrtgAPI.CodeGenerator.MethodBuilder.Builders
{
    internal class MethodBodyBuilder
    {
        MethodConfig methodConfig;
        XmlHelper xmlHelper;

        public MethodBodyBuilder(MethodConfig methodConfig)
        {
            this.methodConfig = methodConfig;
            xmlHelper = new XmlHelper(methodConfig);
        }

        public MethodBody GetMethodBody(MethodHeader header)
        {
            if (methodConfig.HasTokenInterfaceOverload)
                return CallTokenOverload(header);
            else
                return CallNormalBody();
        }

        private MethodBody CallTokenOverload(MethodHeader header)
        {
            var args = header.Parameters.Select(p => p.Name).ToList();
            args.Add("CancellationToken.None");

            var body = CallEscapedMethod(header.MethodName, string.Join(", ", args));

            return new MethodBody(true, false, new[] { body });
        }

        private MethodBody CallNormalBody()
        {
            var elm = GetBodyXml();

            var isExpression = Convert.ToBoolean(xmlHelper.GetAttribute(elm, "expression"));
            var singleLine = xmlHelper.GetBooleanAttribute(elm, "singleLine");

            if (methodConfig.IsTokenInterface)
                singleLine = false;

            var body = xmlHelper.CombineNodes(elm, (b, e, l, i) => ProcessXElement(b, e, l, i, isExpression));

            return new MethodBody(isExpression, singleLine, body);
        }

        private XElement GetBodyXml()
        {
            switch (methodConfig.MethodType)
            {
                case MethodType.Synchronous:
                case MethodType.Asynchronous:
                    if (methodConfig.IsTokenInterface && methodConfig.MethodDef.TokenBodyElement != null)
                        return methodConfig.MethodDef.TokenBody;

                    if (methodConfig.MethodType == MethodType.Synchronous && methodConfig.MethodDef.SyncBodyElement != null)
                        return methodConfig.MethodDef.SyncBody;

                    if (methodConfig.MethodType == MethodType.Asynchronous && methodConfig.MethodDef.AsyncBodyElement != null)
                        return methodConfig.MethodDef.AsyncBody;

                    if (methodConfig.MethodDef.SyncAsyncBodyElement != null)
                        return methodConfig.MethodDef.SyncAsyncBody;
                    break;
                case MethodType.Stream:
                    if (methodConfig.MethodDef.StreamBodyElement != null)
                        return methodConfig.MethodDef.StreamBody;
                    break;
            }

            if (methodConfig.MethodDef.BodyElement == null)
                throw xmlHelper.ThrowMissing($"body for method type {methodConfig.MethodType}");

            return methodConfig.MethodDef.Body;
        }

        private string ProcessXElement(NodeBuilder nodeBuilder, XElement elm, List<XNode> nodes, int index, bool isExpression)
        {
            if (elm.Name.LocalName == "Call")
            {
                var str = CallMethod(elm, index < nodes.Count - 1 && isExpression, index < nodes.Count - 1 ? nodes[index + 1] : null);

                if (isExpression || index < nodes.Count - 1)
                    return str;
                else
                    return str + Environment.NewLine;
            }
            else
                throw new NotImplementedException($"Don't know how to handle body element '{elm.Name.LocalName}'");
        }

        private string CallMethod(XElement elm, bool notLast, XNode nextNode)
        {
            var name = xmlHelper.GetAttribute(elm, "name");
            var args = methodConfig.Method.GetValue(xmlHelper.GetAttribute(elm, "args"));
            var genericArgs = elm.Attribute("genericArgs")?.Value;

            args = ResolveInnerCalls(elm, args);

            if (elm.Attribute("alias") == null || xmlHelper.GetBooleanAttribute(elm, "needsToken"))
            {
                if (methodConfig.IsTokenInterface)
                {
                    if (methodConfig.RequiresNamedTokenArgument)
                        args += ", token: token";
                    else
                    {
                        if (methodConfig.RequiresPositionalTokenArgument)
                            args += ", token";
                    }
                }
                else
                {
                    if (methodConfig.TokenMode == TokenMode.MandatoryCall)
                        args += ", CancellationToken.None";
                    else if (methodConfig.TokenMode == TokenMode.MandatoryNamedCall)
                        args += ", token: CancellationToken.None";
                }
            }

            var needsSurroundingBrackets = false;

            if (methodConfig.MethodType == MethodType.Asynchronous)
            {
                needsSurroundingBrackets = notLast && (nextNode?.ToString().StartsWith(".") == true || nextNode?.ToString().StartsWith("?.") == true);
            }

            return CallUnescapedMethod(name, genericArgs, args, needsSurroundingBrackets);
        }

        private string ResolveInnerCalls(XElement elm, string args)
        {
            var children = elm.Elements().ToList();

            var pattern = "{Alias:(.+?)}";

            var calls = Regex.Matches(args, pattern);

            foreach (Match match in calls)
            {
                var alias = Regex.Replace(match.Value, pattern, "$1");

                var childCall = children.FirstOrDefault(c => c.Name.LocalName == "Call" && c.Attribute("alias")?.Value == alias);

                if (childCall == null)
                    throw new InvalidOperationException($"A call for alias '{alias}' could not be found");

                var method = CallMethod(childCall, false, null);

                args = args.Replace(match.Value, method);
            }

            return args;
        }

        private string CallUnescapedMethod(string name, string genericArgs, string args, bool needsSurroundingBrackets = false)
        {
            switch (methodConfig.MethodType)
            {
                case MethodType.Synchronous:
                    args = args.Replace("{Request}", "").Replace("{RequestLambda}", "");
                    break;
                case MethodType.Asynchronous:
                    args = args.Replace("{Request}", "Async").Replace("{RequestLambda}", "async ");
                    break;
            }

            var methodName = methodConfig.GetMethodName(name, genericArgs);

            return CallEscapedMethod(methodName, args, needsSurroundingBrackets);
        }

        private string CallEscapedMethod(string name, string args, bool needsSurroundingBrackets = false)
        {
            switch (methodConfig.MethodType)
            {
                case MethodType.Synchronous:
                case MethodType.Stream:
                case MethodType.Query:
                case MethodType.Watch:
                    return $"{name}({args})";
                case MethodType.Asynchronous:
                    var str = $"await {name}({args}).ConfigureAwait(false)";

                    if (needsSurroundingBrackets)
                        str = "(" + str + ")";

                    return str;
                default:
                    throw new NotImplementedException($"Don't know how to call method with method type '{methodConfig.MethodType}'");
            }
        }
    }
}
