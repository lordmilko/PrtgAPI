using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PrtgAPI.CodeGenerator.MethodBuilder
{
    internal class XmlHelper
    {
        private MethodConfig methodConfig;

        public XmlHelper(MethodConfig methodConfig)
        {
            this.methodConfig = methodConfig;
        }

        public string[] ParseResourceNodes(XElement elm)
        {
            return CombineNodes(elm, ProcessResourceContainerXElement);
        }

        public string[] CombineNodes(XElement xml, Func<NodeBuilder, XElement, List<XNode>, int, string> getElementText)
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

                        var trimmed = methodConfig.Method.GetValue(l, false);

                        if (Regex.Match(trimmed, "^ +$").Success)
                            trimmed = string.Empty;

                        trimmed = trimmed.Replace("{Request}", methodConfig.MethodType.ToString());

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
            if (hasMoreLines && line.EndsWith(";"))
            {
                if (lineIndex + 1 == lines.Length - 1)
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

        private string ProcessResourceContainerXElement(NodeBuilder nodeBuilder, XElement elm, List<XNode> nodes, int index)
        {
            if (elm.Name.LocalName == "Resource")
            {
                var name = GetAttribute(elm, "name");
                var condition = GetAttribute(elm, "condition");
                var mode = GetAttribute(elm, "mode");

                string val;

                if (!methodConfig.DocumentConfig.Resources.TryGetValue(name, out val))
                    throw new InvalidOperationException($"'{name}' is not a valid property");

                var resource = methodConfig.Method.GetValue(val);

                switch (condition)
                {
                    case "Stream":
                        if (methodConfig.MethodType != MethodType.Stream)
                            return string.Empty;
                        break;
                    case "CancellationToken":
                        if (!methodConfig.IsTokenInterface)
                            return string.Empty;
                        break;
                    case "none":
                        break;
                    default:
                        throw new NotImplementedException($"Don't know how to handle resource for condition '{condition}'");
                }

                switch (mode)
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

        private int? GetWhitespaceToTrim(List<XNode> nodes)
        {
            var textNodes = nodes.OfType<XText>().ToList();

            //Get spaces bigger than one, since we might have a ternary expression between two call elements, which is one space (condition ? <Call/> : <Call/>)
            var minimumSpaces = textNodes.SelectMany(t => t.Value.Split('\n')).Select(t => Regex.Match(t.ToString(), "^ +[^\\s]").Value.Length - 1).Where(v => v > 1)
                .Select(v => (int?)v)
                .Min();

            return minimumSpaces;
        }

        public string GetAttribute(XElement elm, string name)
        {
            var attrib = elm.Attribute(name);

            if (attrib == null)
                throw new InvalidOperationException($"Attribute '{name}' is missing from XML '{elm}'");

            return attrib.Value;
        }

        public bool? GetBooleanAttribute(XElement elm, string name)
        {
            var attrib = elm.Attribute(name);

            if (attrib == null)
                return null;

            return Convert.ToBoolean(attrib.Value);
        }

        public bool GetBooleanAttributeOrDefault(XElement elm, string name) => GetBooleanAttribute(elm, name) ?? false;

        public Exception ThrowMissing(string field)
        {
            throw new InvalidOperationException($"MethodDef '{methodConfig.MethodDef}' with parameters {string.Join(", ", methodConfig.MethodDef.Parameters.Select(p => $"'{p}'"))} does not have a {field}");
        }
    }
}
