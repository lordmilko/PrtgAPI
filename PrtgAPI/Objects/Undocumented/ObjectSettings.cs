using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using PrtgAPI.Html;

namespace PrtgAPI
{
    /// <summary>
    /// Settings that apply to Objects within PRTG.
    /// </summary>
    public class ObjectSettings
    {
        internal static string prefix = "injected_";

        internal static string basicMatchRegex = "<input.+?name=\".*?\".+?value=\".*?\".*?>";
        internal static string standardNameRegex = "(.+?name=\")(.+?)(_*\".+)";

        internal static XElement GetXml(string response)
        {
            var inputXml = GetInputXml(response);
            var ddlXml = GetDropDownListXml(response);
            var textXml = GetTextAreaXml(response);
            var dependencyXml = GetDependency(response); //if the dependency xml is null does that cause an issue for the xelement we create below?

            var elm = new XElement("properties", inputXml, ddlXml, textXml, dependencyXml);
            return elm;
        }

        internal static Dictionary<string, string> GetDictionary(string response)
        {
            var xml = GetXml(response);

            var dictionary = xml.Descendants().ToDictionary(x => x.Name.ToString().Substring(prefix.Length), e => e.Value);

            return dictionary;
        }

        internal static XElement GetDependency(string response)
        {
            var basicMatch = "(<div.+?data-inputname=\"dependency_\")(.+?>)";

            var match = Regex.Match(response, basicMatch);

            if (match.Success)
            {
                var idStr = Regex.Replace(match.Value, "(.+data-selid=\")(.+?)(\".+)", "$2");
                var id = idStr == "null" ? null : (int?)Convert.ToInt32(idStr);

                return new XElement($"{prefix}dependencyvalue", id);
            }

            return null;
        }

        internal static List<Input> GetInput(string response, string matchRegex = null, string nameRegex = null, Func<string, string> nameTransformer = null)
        {
            if (nameTransformer == null)
                nameTransformer = v => v;

            if (matchRegex == null)
                matchRegex = basicMatchRegex;

            if (nameRegex == null)
                nameRegex = standardNameRegex;

            var matches = Regex.Matches(response, matchRegex);
            var inputs = (matches.Cast<Match>().Select(match => match.Value)).ToList();

            var properties = GetProperties(inputs, nameRegex, nameTransformer);

            return properties;
        }

        internal static List<Input> GetFilteredInputs(string response, string nameRegex = null)
        {
            var inputs = GetInput(response, nameRegex: nameRegex);

            var filtered = FilterInputTags(inputs).Select(i => i.Value).ToList();

            return filtered;
        }

        internal static List<XElement> GetInputXml(string response, string matchRegex = null, string nameRegex = null, Func<string, string> nameTransformer = null)
        {
            if (matchRegex == null)
                matchRegex = basicMatchRegex;

            if (nameRegex == null)
                nameRegex = standardNameRegex;

            var properties = GetInput(response, matchRegex, nameRegex, nameTransformer);

            var props = FilterInputTags(properties);

            var list = new List<XElement>();

            foreach (var prop in props)
            {
                try
                {
                    if (prop.Value.Type == InputType.Checkbox)
                    {
                        object val = prop.Value.Value;

                        int intVal;

                        if (int.TryParse(prop.Value.Value, out intVal))
                            val = Convert.ToInt32(prop.Value.Checked);
                        else
                        {
                            //We have a multi option checkbox but nothing was selected
                            if (!prop.Value.Checked)
                                val = string.Empty;
                        }

                        list.Add(new XElement($"{prefix}{prop.Value.Name}", val));
                    }
                    else
                    {
                        list.Add(new XElement($"{prefix}{prop.Value.Name}", prop.Value.Value));
                    }
                }
                catch(Exception ex)
                {
                    throw new HttpDeserializationException($"An error occurred while attempting to deserialize property '{prop.Key}': {ex.Message}", ex);
                }
            }

            return list;
        }

        internal static List<DropDownList> GetDropDownList(string response, string nameRegex = null, Func<string, string> nameTransformer = null)
        {
            if (nameTransformer == null)
                nameTransformer = v => v;

            if (nameRegex == null)
                nameRegex = standardNameRegex;

            var ddl = Regex.Matches(response, "<select.+?>.*?<\\/select>", RegexOptions.Singleline);
            var lists = ddl.Cast<Match>().Select(match => match.Value).ToList();

            var listObjs = GetLists(lists, nameRegex, nameTransformer);

            return listObjs;
        }

        internal static List<XElement> GetDropDownListXml(string response, string nameRegex = null, Func<string, string> nameTransformer = null)
        {
            if (nameTransformer == null)
                nameTransformer = v => v;

            if (nameRegex == null)
                nameRegex = standardNameRegex;

            var listObjs = GetDropDownList(response, nameRegex, nameTransformer);

            if (listObjs.Any(l => l.Options.Any(o => o.Selected)))
            {
                var xml = listObjs.Select(l => new XElement($"{prefix}{l.Name}", l.Options.FirstOrDefault(o => o.Selected)?.Value));
                return xml.ToList();
            }

            return null;
        }

        internal static Dictionary<string, string> GetTextAreaFields(string response, string nameRegex = null)
        {
            if (nameRegex == null)
                nameRegex = standardNameRegex;

            var pattern = "(<textarea.+?>)(.*?)(<\\/textarea>)";

            //todo: what if there are none: will that cause an exception? (e.g. when doing sensor settings properties)

            var text = Regex.Matches(response, pattern, RegexOptions.Singleline);
            var matches = (text.Cast<Match>().Select(match => match.Value)).ToList();

            var namesAndValues = matches.Select(m => new
            {
                Name = Regex.Replace(m, nameRegex, "$2", RegexOptions.Singleline),
                Value = Regex.Replace(m, pattern, "$2", RegexOptions.Singleline)
            }).ToDictionary(i => i.Name, i => i.Value);

            return namesAndValues;
        }

        internal static List<XElement> GetTextAreaXml(string response, string nameRegex = null)
        {
            var text = GetTextAreaFields(response, nameRegex);

            var xml = text.Select(n => new XElement($"{prefix}{n.Key}", n.Value)).ToList();

            return xml;
        }

        private static List<Input> GetProperties(List<string> inputs, string nameRegex, Func<string, string> nameTransformer)
        {
            var properties = inputs.Select(input => new Input
            {
                Name = nameTransformer(Regex.Replace(input, nameRegex, "$2")).Replace("/", "_").Replace(" ", "_"), // Forward slash and space are not valid characters for an XElement name
                Value = HttpUtility.HtmlDecode(Regex.Replace(input, "(.+?value=\")(.*?)(\".+)", "$2")), //todo: should we maybe be decoding the value for all other input types? (text, ddl). test put \\ and " in a sensor factor definition and see if prtg encoded it
                Type = GetInputType(input),
                Checked = Regex.Match(input, "checked").Success,
                Hidden = Regex.Match(input, "type=\"hidden\"").Success
            }).ToList();
            
            return properties; //todo: allow hidden items, and in the filter if theres a conflict overwrite the hidden one
        }

        private static InputType GetInputType(string input)
        {
            if (Regex.Match(input, "radio").Success)
                return InputType.Radio;
            if (Regex.Match(input, "checkbox").Success)
                return InputType.Checkbox;
            return InputType.Other;
        }

        private static List<DropDownList> GetLists(List<string> lists, string nameRegex, Func<string, string> nameTransformer = null)
        {
            if (nameTransformer == null)
                nameTransformer = v => v;

            List<DropDownList> ddls = new List<DropDownList>();

            foreach (var list in lists)
            {
                DropDownList ddl = new DropDownList
                {
                    Name = nameTransformer(Regex.Replace(list, nameRegex, "$2", RegexOptions.Singleline)),
                    Options = new List<Option>()
                };

                var matches = Regex.Matches(list, "<option.+?>.+?<\\/option>", RegexOptions.Singleline)
                    .Cast<Match>()
                    .Select(m => m.Value);

                foreach (var match in matches)
                {
                    ddl.Options.Add(new Option
                    {
                        Value = Regex.Replace(match, "(.+?value=\")(.*?)(\".+)", "$2"),
                        Selected = Regex.Match(match, "selected").Success
                    });
                }

                ddls.Add(ddl);
            }

            return ddls.Where(ddl => ddl.Name != "channel").ToList();

           /*var properties = lists.Select(list => new DropDownList
            {
                //Name = Regex.Replace(list, "(.+?name=\")(.+?)(_*\".+)", "$2", RegexOptions.Singleline), //we might need to leave underscores afterall
                Options = Regex.Matches(list, "<option.+?>.+?<\\/option>", RegexOptions.Singleline)
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .Select(option =>
                        new Option
                        {
                            Value = Regex.Replace(option, "(.+?value=\")(.*?)(\".+)", "$2"),
                            Selected = Regex.Match(option, "selected").Success
                        }
                    ).ToList()
            }).ToList();*/

            //return properties;
        }

        static Dictionary<string, Input> FilterInputTags(List<Input> properties)
        {
            var dictionary = new Dictionary<string, Input>();

            foreach (var prop in properties)
            {
                if (!dictionary.ContainsKey(prop.Name))
                    dictionary.Add(prop.Name, prop);
                else
                {
                    ReplaceExistingItem(dictionary, prop);
                }
            }

            return dictionary;
        }

        private static void ReplaceExistingItem(Dictionary<string, Input> dictionary, Input prop)
        {
            if (prop.Type == dictionary[prop.Name].Type)
                //If our new item has the same as our existing item of the same name
            {
                if (prop.Checked && !dictionary[prop.Name].Checked)
                {
                    //If the new one is checked, replace the existing one
                    dictionary[prop.Name] = prop;
                }
                else
                {
                    if (prop.Checked && dictionary[prop.Name].Checked && !prop.Hidden && !dictionary[prop.Name].Hidden)
                    {
                        dictionary[prop.Name] = new Input
                        {
                            Name = prop.Name,
                            Type = prop.Type,
                            Checked = prop.Checked,
                            Hidden = prop.Hidden,
                            Value = $"{dictionary[prop.Name].Value} {prop.Value}"
                        };
                    }
                }

                //if (prop.Hidden && !dictionary[prop.Name].Hidden)
                //    dictionary[prop.Name] = prop;

                //but there are also HIDDEN ones, we need to allow for that and i think we wanna say if we're hidden replace it? didnt i write this down

                //Otherwise, we're either both checked, or neither are checked, in which case we don't care
            }
            else
            {
                if (!prop.Hidden && dictionary[prop.Name].Hidden)
                    dictionary[prop.Name] = prop;
                else if (prop.Hidden && dictionary[prop.Name].Hidden)
                {
                    //Don't care
                }
                else
                    throw new NotImplementedException($"Two properties were found with the same name but had different types: '{prop.Type}', '{dictionary[prop.Name]}'");
            }
        }
    }
}
