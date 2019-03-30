using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PrtgAPI.CodeGenerator.MethodBuilder.Model;

namespace PrtgAPI.CodeGenerator.MethodBuilder.Builders
{
    internal class MethodXmlDocBuilder
    {
        MethodConfig methodConfig;
        XmlHelper xmlHelper;

        public MethodXmlDocBuilder(MethodConfig methodConfig)
        {
            this.methodConfig = methodConfig;
            xmlHelper = new XmlHelper(methodConfig);
        }

        public MethodXmlDoc GetXmlDoc()
        {
            var summary = GetSummary();
            var genericArgs = GetGenericArgs();
            var parameters = GetParameters();
            var exceptions = GetExceptions();
            var returnDescription = GetReturnDescription();

            return new MethodXmlDoc(summary, genericArgs, parameters, exceptions, returnDescription);
        }

        private string[] GetSummary()
        {
            var elm = XElement.Parse(methodConfig.MethodDef.Summary.OuterXml);

            var summary = xmlHelper.ParseResourceNodes(elm);

            if (methodConfig.MethodType == MethodType.Asynchronous)
                summary[0] = "Asynchronously " + summary[0][0].ToString().ToLower() + summary[0].Substring(1);

            if (methodConfig.MethodType == MethodType.Stream)
                summary[0] = summary[0].Replace("Retrieves", "Streams");

            if (methodConfig.IsTokenInterface && methodConfig.RequiresTokenSummary)
            {
                var endSentence = summary[0].IndexOf(". ");

                if (endSentence == -1)
                {
                    if (summary[0].EndsWith("."))
                        endSentence = summary[0].Length - 1;
                    else
                    {
                        if (summary[0].EndsWith(".<para/>"))
                            endSentence = summary[0].LastIndexOf(".<para/>");
                        else
                            throw xmlHelper.ThrowMissing("period at the end of the first line in its summary");
                    }
                }

                summary[0] = summary[0].Insert(endSentence, " " + methodConfig.DocumentConfig.Resources["CancellationToken"].TrimEnd('.'));
            }

            var list = new List<string>();
            list.Add("/// <summary>");
            list.AddRange(summary.Select(l => "/// " + l.TrimStart(' ')));
            list.Add("/// </summary>");

            return list.ToArray();
        }

        private string[] GetGenericArgs()
        {
            var lines = new List<string>();

            foreach (var param in methodConfig.MethodDef.GenericArgs)
            {
                lines.Add($"/// <typeparam name=\"{param.Name}\">{param.Description}</typeparam>");
            }

            return lines.ToArray();
        }

        private string[] GetParameters()
        {
            var parameters = methodConfig.GetApplicableParameters();

            var list = new List<string>();

            foreach(var param in parameters)
            {
                var paramName = methodConfig.Method.GetValue(param.Name).TrimStart('@');

                var desc = param.Description;

                if (methodConfig.MethodType == MethodType.Stream && param.StreamDescription != null)
                    desc = param.StreamDescription;

                var description = methodConfig.Method.GetValue(desc).Split(new[] { "\\n" }, StringSplitOptions.None);

                if (description == null || description.Length == 1 && string.IsNullOrEmpty(description[0]))
                    throw xmlHelper.ThrowMissing($"description on parameter '{param.Name}'");

                list.AddRange(GetMultiLineXmlDoc(description, $"param name=\"{paramName}\"", "param"));
            }

            return list.ToArray();
        }

        private string[] GetExceptions()
        {
            var list = new List<string>();

            if (methodConfig.MethodDef.Exception != null)
            {
                var type = methodConfig.MethodDef.Exception.Type;
                var description = methodConfig.Method.GetValue(methodConfig.MethodDef.Exception.Description);

                list.Add($"/// <exception cref=\"{type}\">{description}</exception>");
            }

            return list.ToArray();
        }

        private string[] GetReturnDescription()
        {
            if (methodConfig.MethodDef.ReturnDescription == null && methodConfig.MethodDef.ReturnType != "void" && methodConfig.MethodDef.ReturnType != "Task")
                throw xmlHelper.ThrowMissing("return type description");

            if (methodConfig.MethodDef.ReturnDescription != null)
            {
                var elm = XElement.Parse(methodConfig.MethodDef.ReturnDescription.OuterXml);

                var inner = xmlHelper.ParseResourceNodes(elm);

                return GetMultiLineXmlDoc(inner, "returns").ToArray();
            }

            return new string[] { };
        }

        private IEnumerable<string> GetMultiLineXmlDoc(string[] lines, string openTag, string closeTag = null)
        {
            if (closeTag == null)
                closeTag = openTag;

            if (!lines.Last().EndsWith("."))
                xmlHelper.ThrowMissing($"trailing period on the line '{lines.Last()}'");

            if (lines.Length == 1)
            {
                yield return $"/// <{openTag}>{lines[0]}</{closeTag}>";
            }
            else
            {
                for (var i = 0; i < lines.Length; i++)
                {
                    if (i == 0)
                        yield return $"/// <{openTag}>{lines[i]}";
                    else if (i == lines.Length - 1)
                        yield return $"/// {lines[i]}</{closeTag}>";
                    else
                        yield return $"/// {lines[i]}";
                }
            }
        }
    }
}
