using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using PrtgAPI.CodeGenerator.Model;

namespace PrtgAPI.CodeGenerator
{
    class MethodBuilder
    {
        public IMethodImpl Method { get; }

        public MethodDef MethodDef { get; }

        public Config Config { get; }

        public SourceWriter Writer { get; }

        public MethodType MethodType { get; }

        public MethodBuilder(IMethodImpl method, MethodDef methodDef, Config config, SourceWriter writer, MethodType methodType)
        {
            Method = method;
            MethodDef = methodDef;
            Config = config;
            Writer = writer;
            MethodType = methodType;
        }

        public void WriteMethod()
        {
            WriteXmlDoc();
            WriteHeader();
            WriteBody();
        }

        private void WriteXmlDoc()
        {
            var summary = GetSummary();

            Writer.FullLine($"/// <summary>");

            foreach(var line in summary)
                Writer.FullLine($"/// {line}");

            Writer.FullLine($"/// </summary>");

            foreach(var param in MethodDef.GenericArgs)
            {
                Writer.FullLine($"/// <typeparam name=\"{param.Name}\">{param.Description}</typeparam>");
            }

            foreach (var param in MethodDef.Parameters)
            {
                if (MethodType != MethodType.Stream && param.StreamOnly)
                    continue;

                if (MethodType == MethodType.Stream && param.ExcludeStream)
                    continue;

                var paramName = Method.GetValue(param.Name).TrimStart('@');

                var desc = param.Description;

                if (MethodType == MethodType.Stream && param.StreamDescription != null)
                    desc = param.StreamDescription;

                var description = Method.GetValue(desc).Split(new[] {"\\n"}, StringSplitOptions.None);

                if (description == null || description.Length == 1 && string.IsNullOrEmpty(description[0]))
                    throw ThrowMissing($"description on parameter '{param.Name}'");

                if(description.Length == 1)
                    Writer.FullLine($"/// <param name=\"{paramName}\">{description[0]}</param>");
                else
                {
                    WriteMultiLineXmlDoc(description, $"param name=\"{paramName}\"", "param");
                }
            }

            if (MethodDef.Exception != null)
                Writer.FullLine($"/// <exception cref=\"{MethodDef.Exception.Type}\">{Method.GetValue(MethodDef.Exception.Description)}</exception>");

            if (MethodDef.ReturnDescription == null && MethodDef.ReturnType != "void" && MethodDef.ReturnType != "Task")
                throw ThrowMissing("return type description");

            if(MethodDef.ReturnDescription != null)
            {
                WriteMultiLineXmlDoc(GetReturnDescription(), "returns");
            }
        }

        private void WriteMultiLineXmlDoc(string[] lines, string openTag, string closeTag = null)
        {
            if (closeTag == null)
                closeTag = openTag;

            if (lines.Length == 1)
            {
                Writer.FullLine($"/// <{openTag}>{lines[0]}</{closeTag}>");
                return;
            }

            for (var i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                    Writer.FullLine($"/// <{openTag}>{lines[i]}");
                else if (i == lines.Length - 1)
                    Writer.FullLine($"/// {lines[i]}</{closeTag}>");
                else
                    Writer.FullLine($"/// {lines[i]}");
            }
        }

        private string[] GetReturnDescription()
        {
            var elm = XElement.Parse(MethodDef.ReturnDescription.OuterXml);

            return CombineNodeText(elm, ProcessResourceContainerXElement);
        }

        private void WriteHeader()
        {
            string genericArgs = null;

            if (MethodDef.GenericArgs.Count > 0)
                genericArgs = string.Join(", ", MethodDef.GenericArgs.Select(a => a.Name));

            var methodName = GetMethodName(MethodDef.Name, genericArgs);
            var returnType = GetReturnType();

            Writer.StartLine($"public {returnType} {methodName}(");

            var paramDefs = MethodDef.Parameters.Where(p =>
            {
                if (MethodType != MethodType.Stream && p.StreamOnly)
                    return false;

                if (MethodType == MethodType.Stream && p.ExcludeStream)
                    return false;

                return true;
            }).Select(p =>
            {
                if (string.IsNullOrEmpty(p.Name))
                    throw new InvalidOperationException($"MethodDef '{MethodDef}' has unnamed parameters");

                if (string.IsNullOrEmpty(p.Type))
                    throw new InvalidOperationException($"Parameter '{p.Name}' of methoddef '{MethodDef}' doesn't have a type");

                var @default = p.Default;

                if (MethodType == MethodType.Stream && p.StreamDefault != null)
                    @default = p.StreamDefault;

                return $"{Method.GetValue(p.Type)} {Method.GetValue(p.Name)}" + (@default != null ? $" = {@default}" : string.Empty);
            });

            Writer.Write(string.Join(", ", paramDefs));

            Writer.Write(")");
        }

        private void WriteBody()
        {
            var bodyXml = GetBodyXml();

            var isExpression = Convert.ToBoolean(GetAttribute(bodyXml, "expression"));
            var singleLine = GetBooleanAttribute(bodyXml, "singleLine");

            var bodyText = GetBodyText(bodyXml, isExpression);

            if (isExpression)
            {
                var body = string.Empty;

                try
                {
                    body = bodyText.Single();
                }
                catch (System.Exception ex)
                {
                    throw new InvalidOperationException($"Method '{Method}' with parameters {string.Join(", ", MethodDef.Parameters.Select(p => $"'{p}'"))} has a multi-lined body. Are you sure it is an expression?", ex);
                }

                if (singleLine)
                    Writer.WriteLine($" => {body};");
                else
                {
                    Writer.WriteLine(" =>");
                    Writer.FullLine($"    {body};");
                }
            }
            else
            {
                Writer.WriteLine("");

                Writer.FullLine("{");

                for (var i = 0; i < bodyText.Length; i++)
                {
                    if (bodyText[i] == string.Empty)
                        Writer.WriteLine("");
                    else
                        Writer.FullLine($"    {bodyText[i]}");
                }

                Writer.FullLine("}");
            }
        }

        private XElement GetBodyXml()
        {
            switch (MethodType)
            {
                case MethodType.Synchronous:
                case MethodType.Asynchronous:
                    if (MethodType == MethodType.Synchronous && MethodDef.SyncBodyElement != null)
                        return MethodDef.SyncBody;

                    if (MethodType == MethodType.Asynchronous && MethodDef.AsyncBodyElement != null)
                        return MethodDef.AsyncBody;

                    if (MethodDef.SyncAsyncBodyElement != null)
                        return MethodDef.SyncAsyncBody;
                    break;
                case MethodType.Stream:
                    if (MethodDef.StreamBodyElement != null)
                        return MethodDef.StreamBody;
                    break;
            }

            if (MethodDef.BodyElement == null)
                throw ThrowMissing($"body for method type {MethodType}");

            return MethodDef.Body;
        }

        private string[] GetSummary()
        {
            var summary = CombineNodeText(XElement.Parse(MethodDef.Summary.OuterXml), ProcessResourceContainerXElement);

            if (MethodType == MethodType.Asynchronous)
                summary[0] = "Asynchronously " + summary[0][0].ToString().ToLower() + summary[0].Substring(1);

            if (MethodType == MethodType.Stream)
                summary[0] = summary[0].Replace("Retrieves", "Streams");

            return summary.Select(l => l.TrimStart(' ')).ToArray();
        }

        private string GetMethodName(string name, string genericArgs)
        {
            name = Method.GetValue(name);

            if (name.Contains("{Request}"))
            {
                switch (MethodType)
                {
                    case MethodType.Synchronous:
                        name = name.Replace("{Request}", "Get");
                        break;
                    case MethodType.Asynchronous:
                        name = name.Replace("{Request}", "Get") + "Async";
                        break;
                    case MethodType.Stream:
                        name = name.Replace("{Request}", "Stream");
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                if (MethodType == MethodType.Asynchronous)
                    name += "Async";
            }

            if (genericArgs != null)
                name += $"<{Method.GetValue(genericArgs)}>";

            return name;
        }

        private string[] GetBodyText(XElement xml, bool isExpression)
        {
            return CombineNodeText(xml, (b, e, l, i) => ProcessBodyTextXElement(b, e, l, i, isExpression));
        }

        private string ProcessBodyTextXElement(NodeBuilder nodeBuilder, XElement elm, List<XNode> nodes, int index, bool isExpression)
        {
            if (elm.Name.LocalName == "Call")
            {
                var str = GetBodyMethod(elm, index < nodes.Count - 1 && isExpression, index < nodes.Count - 1 ? nodes[index + 1] : null);

                if (isExpression || index < nodes.Count - 1)
                    return str;
                else
                    return str + "\r\n";
            }
            else
                throw new NotImplementedException();
        }

        private string ProcessResourceContainerXElement(NodeBuilder nodeBuilder, XElement elm, List<XNode> nodes, int index)
        {
            if (elm.Name.LocalName == "Resource")
            {
                var name = GetAttribute(elm, "name");
                var condition = GetAttribute(elm, "condition");
                var mode = GetAttribute(elm, "mode");

                string val;

                if(!Config.Resources.TryGetValue(name, out val))
                    throw new InvalidOperationException($"'{name}' is not a valid property");

                var resource = Method.GetValue(val);

                switch(condition)
                {
                    case "Stream":
                        if (MethodType != MethodType.Stream)
                            return string.Empty;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                switch(mode)
                {
                    case "append":
                        if (resource.StartsWith("<para/>"))
                            return resource;

                        return " " + resource;
                    case "replace":
                        nodeBuilder.Clear();
                        nodeBuilder.Continue = false;
                        return resource;
                    default:
                        throw new NotImplementedException();
                }

                throw new NotImplementedException();
            }
            else
                throw new NotImplementedException();
        }

        class NodeBuilder
        {
            private StringBuilder builder = new StringBuilder();

            public string DebugView => builder.ToString();

            public bool Continue { get; set; } = true;

            public void Append(string value)
            {
                builder.Append(value);
            }

            public void AppendLine()
            {
                builder.AppendLine();
            }

            public void AppendLine(string value)
            {
                builder.AppendLine(value);
            }

            public void Clear()
            {
                builder.Clear();
            }

            public override string ToString()
            {
                return builder.ToString();
            }
        }

        private string[] CombineNodeText(XElement xml, Func<NodeBuilder, XElement, List<XNode>, int, string> getElementText)
        {
            var nodes = xml.Nodes().ToList();

            var nodeBuilder = new NodeBuilder();

            var trimWhitespaceLength = GetWhitespaceToTrim(nodes) ?? 0;

            var spaces = new string(' ', trimWhitespaceLength);

            for (var i = 0; i < nodes.Count && nodeBuilder.Continue; i++)
            {
                if (nodes[i] is XText)
                {
                    var lines = ((XText)nodes[i]).Value.Split('\n');

                    lines = lines.SkipWhile(string.IsNullOrEmpty).Reverse().SkipWhile(string.IsNullOrEmpty)
                        .Reverse().ToArray();

                    for (var j = 0; j < lines.Length; j++)
                    {
                        string l;

                        if (lines[j].StartsWith(spaces))
                            l = lines[j].Substring(trimWhitespaceLength);
                        else
                            l = lines[j];

                        //Is the last line of an expression all spaces? Ignore!
                        if (j == lines.Length - 1)
                        {
                            if (Regex.Match(l, "^ +$").Success)
                                continue;
                        }

                        var trimmed = Method.GetValue(l, false);

                        if (Regex.Match(trimmed, "^ +$").Success)
                            trimmed = string.Empty;

                        nodeBuilder.Append(trimmed);

                        //If this isn't the last line of text and the next line isn't all just spaces, or there are at least two more lines to go, insert a newline
                        if (NodeTextNeedsNewLine(lines, nodes, nodeBuilder, i, j)) //todo: this is wrong.
                            nodeBuilder.AppendLine();
                    }
                }

                if (nodes[i] is XElement)
                {
                    var elm = (XElement)nodes[i];

                    nodeBuilder.Append(getElementText(nodeBuilder, elm, nodes, i));
                }
            }

            return nodeBuilder.ToString().Split('\n').Select(v => v.TrimEnd('\r')).Reverse().SkipWhile(string.IsNullOrEmpty).Reverse().ToArray();
        }

        private bool NodeTextNeedsNewLine(string[] lines, List<XNode> nodes, NodeBuilder nodeBuilder, int nodeIndex, int lineIndex)
        {
            if (!nodeBuilder.Continue)
                return false;

            var line = lines[lineIndex];
            var node = nodes[nodeIndex];

            var hasMoreLines = lineIndex < lines.Length - 1;
            var hasMoreNodes = nodeIndex < nodes.Count - 1;
            Func<int, bool> lineIsAllWhitespace = i => Regex.Match(lines[i], "^ +$").Success || lines[i] == string.Empty;
            Func<int, bool> nodeIsXElement = i => nodes[i] is XElement;

            //Is there a line after me, and if so is the line directly after me not whitespace?
            if (hasMoreLines && lineIsAllWhitespace(lineIndex + 1) == false)
                return true;

            //Is there another node of some kind after me, and did the current line end in a semicolon?
            if (hasMoreNodes && line.EndsWith(";"))
                return true;

            //If we end with a semicolon, if the next line is the last line, if its all whitespace: false. otherwise, true
            if(hasMoreLines && line.EndsWith(";"))
            {
                if(lineIndex + 1 == lines.Length - 1)
                {
                    if (!lineIsAllWhitespace(lineIndex + 1))
                        return true;

                    return false;
                }

                return true;
            }

            if (hasMoreLines && lineIsAllWhitespace(lineIndex))
                return true;

            return false;
        }

        private int? GetWhitespaceToTrim(List<XNode> nodes)
        {
            var textNodes = nodes.OfType<XText>().ToList();

            //Get spaces bigger than one, since we might have a ternary expression between two call elements, which is one space (condition ? <Call/> : <Call/>)
            var minimumSpaces = textNodes.SelectMany(t => t.Value.Split('\n')).Select(t => Regex.Match(t.ToString(), "^ +[^\\s]").Value.Length - 1).Where(v => v > 1)
                .Select(v => (int?)v)
                .Min();

            return minimumSpaces;
        }

        private string GetBodyMethod(XElement elm, bool notLast, XNode nextNode)
        {
            var name = GetAttribute(elm, "name");
            var args = Method.GetValue(GetAttribute(elm, "args"));
            var genericArgs = elm.Attribute("genericArgs")?.Value;

            args = ResolveInnerCalls(elm, args);

            switch (MethodType)
            {
                case MethodType.Synchronous:
                    args = args.Replace("{Request}", "");
                    break;
                case MethodType.Asynchronous:
                    args = args.Replace("{Request}", "Async");
                    break;
            }

            switch (MethodType)
            {
                case MethodType.Synchronous:
                case MethodType.Stream:
                    return $"{GetMethodName(name, genericArgs)}({args})";
                case MethodType.Asynchronous:
                    var str = $"await {GetMethodName(name, genericArgs)}({args}).ConfigureAwait(false)";

                    if (notLast && (nextNode?.ToString().StartsWith(".") == true || nextNode?.ToString().StartsWith("?.") == true))
                        str = "(" + str + ")";

                    return str;
                default:
                    throw new NotImplementedException();
            }
        }

        private string ResolveInnerCalls(XElement elm, string args)
        {
            var children = elm.Elements().ToList();

            var pattern = "{Alias:(.+?)}";

            var calls = Regex.Matches(args, pattern);

            foreach(Match match in calls)
            {
                var alias = Regex.Replace(match.Value, pattern, "$1");

                var childCall = children.FirstOrDefault(c => c.Name.LocalName == "Call" && c.Attribute("alias")?.Value == alias);

                if (childCall == null)
                    throw new InvalidOperationException($"A call for alias '{alias}' could not be found");

                var method = GetBodyMethod(childCall, false, null);

                args = args.Replace(match.Value, method);
            }

            return args;
        }

        private string GetAttribute(XElement elm, string name)
        {
            var attrib = elm.Attribute(name);

            if (attrib == null)
                throw new InvalidOperationException($"Attribute '{name}' is missing from XML '{elm}'");

            return attrib.Value;
        }

        private bool GetBooleanAttribute(XElement elm, string name)
        {
            var attrib = elm.Attribute(name);

            if (attrib == null)
                return false;

            return Convert.ToBoolean(attrib.Value);
        }

        private string GetReturnType()
        {
            var returnType = Method.GetValue(MethodDef.ReturnType);

            if (string.IsNullOrWhiteSpace(returnType))
                throw ThrowMissing("return type");

            if (MethodType == MethodType.Asynchronous)
            {
                if (returnType == "void")
                    return "async Task";

                return $"async Task<{returnType}>";
            }

            if (MethodType == MethodType.Stream)
                return returnType.Replace("List", "IEnumerable");

            return returnType;
        }

        private System.Exception ThrowMissing(string field)
        {
            throw new InvalidOperationException($"MethodDef '{MethodDef}' with parameters {string.Join(", ", MethodDef.Parameters.Select(p => $"'{p}'"))} does not have a {field}");
        }
    }
}
