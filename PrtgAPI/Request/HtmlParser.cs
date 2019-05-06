using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using PrtgAPI.Html;

namespace PrtgAPI.Request
{
    class HtmlParser
    {
        internal static readonly HtmlParser Default = new HtmlParser();

        internal const string DefaultPropertyPrefix = "injected_";

        internal const string DefaultBasicMatchRegex = "<input.+?name=\".*?\".+?value=\".*?\".*?>";
        internal const string DefaultBackwardsMatchRegex = "<input.+?value=\".*?\".+?name=\".*?\".*?>";
        internal const string DefaultStandardNameRegex = "(.+?name=\")(.+?)(_*\".+)";
        internal const string DefaultDropDownListRegex = "<select.+?>.*?<\\/select>";
        internal const string DefaultTextAreaRegex = "(<textarea.+?>)(.*?)(<\\/textarea>)";
        internal const string DefaultDependencyDiv = "(<div.+?data-inputname=\"dependency_\")(.+?>)";

        #region PropertyPrefix

        private string propertyPrefix;

        [ExcludeFromCodeCoverage]
        public string PropertyPrefix
        {
            get { return propertyPrefix ?? DefaultPropertyPrefix; }
            set { propertyPrefix = value; }
        }

        #endregion
        #region BasicMatchRegex

        private string basicMatchRegex;

        [ExcludeFromCodeCoverage]
        public string BasicMatchRegex
        {
            get { return basicMatchRegex ?? DefaultBasicMatchRegex; }
            set { basicMatchRegex = value; }
        }

        #endregion
        #region BackwardsMatchRegex

        private string backwardsMatchRegex;

        [ExcludeFromCodeCoverage]
        public string BackwardsMatchRegex
        {
            get { return backwardsMatchRegex ?? DefaultBackwardsMatchRegex; }
            set { backwardsMatchRegex = value; }
        }

        #endregion
        #region StandardNameRegex

        private string standardNameRegex;

        [ExcludeFromCodeCoverage]
        public string StandardNameRegex
        {
            get { return standardNameRegex ?? DefaultStandardNameRegex; }
            set { standardNameRegex = value; }
        }

        #endregion
        #region DropDownListRegex

        private string dropDownListRegex;

        [ExcludeFromCodeCoverage]
        public string DropDownListRegex
        {
            get { return dropDownListRegex ?? DefaultDropDownListRegex; }
            set { dropDownListRegex = value; }
        }

        #endregion
        #region TextAreaRegex

        private string textAreaRegex;

        [ExcludeFromCodeCoverage]
        public string TextAreaRegex
        {
            get { return textAreaRegex ?? DefaultTextAreaRegex; }
            set { textAreaRegex = value; }
        }

        #endregion
        #region DependencyDiv

        private string dependencyDiv;

        [ExcludeFromCodeCoverage]
        public string DependencyDiv
        {
            get { return dependencyDiv ?? DefaultDependencyDiv; }
            set { dependencyDiv = value; }
        }

        #endregion

        internal XElement GetXml(PrtgResponse response)
        {
            var str = response.StringValue;

            var inputXml = GetInputXml(str);
            var ddlXml = GetDropDownListXml(str);
            var textXml = GetTextAreaXml(str);
            var dependencyXml = GetDependency(str); //if the dependency xml is null does that cause an issue for the xelement we create below?

            var elm = new XElement("properties", inputXml, ddlXml, textXml, dependencyXml);
            return elm;
        }

        internal Dictionary<string, string> GetDictionary(PrtgResponse response)
        {
            var xml = GetXml(response);

            var dictionary = xml.Descendants().ToDictionary(x => x.Name.ToString().Substring(PropertyPrefix.Length), e => e.Value);

            return dictionary;
        }

        internal XElement GetDependency(string response)
        {
            var match = Regex.Match(response, DependencyDiv);

            if (match.Success)
            {
                var idStr = Regex.Replace(match.Value, "(.+data-selid=\")(.+?)(\".+)", "$2");
                var id = idStr == "null" ? null : (int?)Convert.ToInt32(idStr);

                return new XElement($"{PropertyPrefix}dependencyvalue", id);
            }

            return null;
        }

        internal List<Input> GetInput(string response, string matchRegex = null, string nameRegex = null, Func<string, string> nameTransformer = null)
        {
            if (nameTransformer == null)
                nameTransformer = v => v;

            if (matchRegex == null)
                matchRegex = BasicMatchRegex;

            if (nameRegex == null)
                nameRegex = StandardNameRegex;

            var matches = Regex.Matches(response, matchRegex);
            var inputs = (matches.Cast<Match>().Select(match => match.Value)).ToList();

            var properties = GetProperties(inputs, nameRegex, nameTransformer);

            return properties;
        }

        internal List<Input> GetFilteredInputs(string response, string nameRegex = null)
        {
            var inputs = GetInput(response, nameRegex: nameRegex);

            var filtered = FilterInputTags(inputs).Select(i => i.Value).ToList();

            return filtered;
        }

        internal List<XElement> GetInputXml(string response, string matchRegex = null, string nameRegex = null, Func<string, string> nameTransformer = null)
        {
            if (matchRegex == null)
                matchRegex = BasicMatchRegex;

            if (nameRegex == null)
                nameRegex = StandardNameRegex;

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

                        list.Add(new XElement($"{PropertyPrefix}{prop.Value.Name}", val));
                    }
                    else
                    {
                        list.Add(new XElement($"{PropertyPrefix}{prop.Value.Name}", prop.Value.Value));
                    }
                }
                catch (Exception ex)
                {
                    throw new HttpDeserializationException($"An error occurred while attempting to deserialize property '{prop.Key}': {ex.Message}.", ex);
                }
            }

            return list;
        }

        internal List<DropDownList> GetDropDownList(string response, string nameRegex = null, Func<string, string> nameTransformer = null)
        {
            if (nameTransformer == null)
                nameTransformer = v => v;

            if (nameRegex == null)
                nameRegex = StandardNameRegex;

            var ddl = Regex.Matches(response, DropDownListRegex, RegexOptions.Singleline);
            var lists = ddl.Cast<Match>().Select(match => match.Value).ToList();

            var listObjs = GetLists(lists, nameRegex, nameTransformer);

            return listObjs;
        }

        internal List<XElement> GetDropDownListXml(string response, string nameRegex = null, Func<string, string> nameTransformer = null)
        {
            if (nameTransformer == null)
                nameTransformer = v => v;

            if (nameRegex == null)
                nameRegex = StandardNameRegex;

            var listObjs = GetDropDownList(response, nameRegex, nameTransformer);

            if (listObjs.Any(l => l.Options.Any(o => o.Selected)))
            {
                var xml = listObjs.Select(l => new XElement($"{PropertyPrefix}{l.Name}", l.Options.FirstOrDefault(o => o.Selected)?.Value));
                return xml.ToList();
            }

            return null;
        }

        internal Dictionary<string, string> GetTextAreaFields(string response, string nameRegex = null)
        {
            if (nameRegex == null)
                nameRegex = StandardNameRegex;

            //todo: what if there are none: will that cause an exception? (e.g. when doing sensor settings properties)

            var text = Regex.Matches(response, TextAreaRegex, RegexOptions.Singleline);
            var matches = (text.Cast<Match>().Select(match => match.Value)).ToList();

            var namesAndValues = matches.Select(m => new
            {
                Name = Regex.Replace(m, nameRegex, "$2", RegexOptions.Singleline),
                Value = Regex.Replace(m, TextAreaRegex, "$2", RegexOptions.Singleline)
            }).ToDictionary(i => i.Name, i => i.Value);

            return namesAndValues;
        }

        internal List<XElement> GetTextAreaXml(string response, string nameRegex = null)
        {
            var text = GetTextAreaFields(response, nameRegex);

            var xml = text.Select(n => new XElement($"{PropertyPrefix}{n.Key}", n.Value)).ToList();

            return xml;
        }

        private List<Input> GetProperties(List<string> inputs, string nameRegex, Func<string, string> nameTransformer)
        {
            var properties = inputs.Select(input => new Input
            {
                Name = nameTransformer(Regex.Replace(input, nameRegex, "$2")).Replace("/", "_").Replace(" ", "_"), // Forward slash and space are not valid characters for an XElement name
                Value = WebUtility.HtmlDecode(Regex.Replace(input, "(.+?value=\")(.*?)(\".+)", "$2")), //todo: should we maybe be decoding the value for all other input types? (text, ddl). test put \\ and " in a sensor factor definition and see if prtg encoded it
                Type = GetInputType(input),
                Checked = Regex.Match(input, "checked").Success,
                Hidden = Regex.Match(input, "type=\"hidden\"").Success,
                Html = input
            }).ToList();

            return properties; //todo: allow hidden items, and in the filter if theres a conflict overwrite the hidden one
        }

        private InputType GetInputType(string input)
        {
            if (Regex.Match(input, "type=\"radio\"").Success)
                return InputType.Radio;
            if (Regex.Match(input, "type=\"checkbox\"").Success)
                return InputType.Checkbox;
            return InputType.Other;
        }

        private List<DropDownList> GetLists(List<string> lists, string nameRegex, Func<string, string> nameTransformer = null)
        {
            if (nameTransformer == null)
                nameTransformer = v => v;

            List<DropDownList> ddls = new List<DropDownList>();

            foreach (var list in lists)
            {
                DropDownList ddl = new DropDownList
                {
                    Name = nameTransformer(Regex.Replace(list, nameRegex, "$2", RegexOptions.Singleline)),
                    Options = new List<Option>(),
                    Html = list
                };

                var matches = Regex.Matches(list, "<option.+?>.+?<\\/option>", RegexOptions.Singleline)
                    .Cast<Match>()
                    .Select(m => m.Value);

                foreach (var match in matches)
                {
                    ddl.Options.Add(new Option
                    {
                        Value = Regex.Replace(match, "(.+?value=\")(.*?)(\".+)", "$2"),
                        Selected = Regex.Match(match, "selected").Success,
                        Html = match
                    });
                }

                ddls.Add(ddl);
            }

            return ddls.Where(ddl => ddl.Name != "channel").ToList();
        }

        private Dictionary<string, Input> FilterInputTags(List<Input> properties)
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

        private void ReplaceExistingItem(Dictionary<string, Input> dictionary, Input prop)
        {
            //If our new item has the same as our existing item of the same name
            if (prop.Type == dictionary[prop.Name].Type) 
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
                //We have two properties with different types. if the existing object is hidden and the replacement isn't, the replacement
                //supercedes it
                if (!prop.Hidden && dictionary[prop.Name].Hidden)
                    dictionary[prop.Name] = prop;

                //If both properties are hidden but are of different types, or the challenge is hidden but the existing one ISN'T (inverse of the above), we don't care
                else if (prop.Hidden && dictionary[prop.Name].Hidden || prop.Hidden && !dictionary[prop.Name].Hidden)
                {
                    //Don't care
                }
                else
                    throw new NotImplementedException($"Two properties were found with the name '{prop.Name}' but had different types: '{prop.Type}' ({prop.Html}), '{dictionary[prop.Name].Type}' ({dictionary[prop.Name].Html}).");
            }
        }
    }
}
