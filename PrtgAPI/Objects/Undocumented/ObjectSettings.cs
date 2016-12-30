using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Html;

namespace PrtgAPI.Objects.Undocumented
{
    public class ObjectSettings
    {
        internal static XElement GetDependency(string response)
        {
            var basicMatch = "(<div.+?data-inputname=\"dependency_\")(.+?>)";

            var match = Regex.Match(response, basicMatch);

            if (match.Success)
            {
                var idStr = Regex.Replace(match.Value, "(.+data-selid=\")(.+?)(\".+)", "$2");
                var id = idStr == "null" ? null : (int?)Convert.ToInt32(idStr);

                return new XElement("injected_dependencyvalue", id);
            }

            return null;
        }

        internal static List<XElement> GetInputXml(string response, string basicMatchRegex, string nameRegex, Func<string, string> nameTransformer)
        {
            var matches = Regex.Matches(response, basicMatchRegex);
            var inputs = (matches.Cast<Match>().Select(match => match.Value)).ToList();

            var properties = GetProperties(inputs, nameRegex, nameTransformer);

            var props = FilterInputTags(properties); //.Select(v => new XElement($"injected_{v.Value.Name}", v.Value.Value));

            var list = new List<XElement>();

            foreach (var prop in props)
            {
                if (prop.Value.Type == InputType.Checkbox)
                {
                    list.Add(new XElement($"injected_{prop.Value.Name}", Convert.ToInt32(prop.Value.Checked)));
                }
                else
                {
                    list.Add(new XElement($"injected_{prop.Value.Name}", prop.Value.Value));
                }
            }

            return list;
        }

        internal static List<XElement> GetDropDownListXml(string response, string nameRegex)
        {
            var ddl = Regex.Matches(response, "<select.+?>.+?<\\/select>", RegexOptions.Singleline);
            var lists = (ddl.Cast<Match>().Select(match => match.Value)).ToList();

            var listObjs = GetLists(lists, nameRegex);

            if (listObjs.Any(l => l.Options.Any(o => o.Selected)))
            {
                var xml = listObjs.Select(l => new XElement($"injected_{l.Name}", l.Options.First(o => o.Selected).Value));
                return xml.ToList();
            }

            return null;
        }

        private static List<Input> GetProperties(List<string> inputs, string nameRegex, Func<string, string> nameTransformer = null)
        {
            if (nameTransformer == null)
                nameTransformer = v => v;

            var properties = inputs.Select(input => new Input
            {
                Name = nameTransformer(Regex.Replace(input, nameRegex, "$2")), //.Replace($"_{channelId}", ""),
                Value = Regex.Replace(input, "(.+?value=\")(.*?)(\".+)", "$2"),
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

        private static List<DropDownList> GetLists(List<string> lists, string nameRegex)
        {
            List<DropDownList> ddls = new List<DropDownList>();

            foreach (var list in lists)
            {
                DropDownList ddl = new DropDownList
                {
                    Name = Regex.Replace(list, nameRegex, "$2", RegexOptions.Singleline),
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

            return ddls;

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
                    //If the new one is checked, replace the existing one
                    dictionary[prop.Name] = prop;

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

        
        /*internal static Dictionary<string, Input> FilterInputTags1(List<Input> properties)
        {
            var dictionary = new Dictionary<string, Input>();

            foreach (var prop in properties)
            {
                if (!dictionary.ContainsKey(prop.Name)) //if the item is new, add it
                    dictionary.Add(prop.Name, prop);
                else                                    //if the item already exists
                {
                    if (prop.Radio && dictionary[prop.Name].Radio) //and the new and existimg items are both radio buttons
                    {
                        if (prop.Checked && !dictionary[prop.Name].Checked) //and we're checked and our existing one isnt, replace it
                            dictionary[prop.Name] = prop;
                        else
                        {
                            if (!prop.Checked && !dictionary[prop.Name].Checked) //neither of us are checked
                            {
                                if (prop.Value != dictionary[prop.Name].Value) //if we have different values
                                {
                                    //todo: bug: if the two values are disconnected, which property are we adding? both? neither
                                    //none of them are ticked; thats cool
                                    //throw new NotImplementedException("Two channel unchecked channel properties had the same name with different values");
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new NotImplementedException(
                            "We had two channel properties that arent radio buttons, or two properties with the same name with only one of them a radio");
                        //crash! either we have a duplicate of two things that arent radios, or a thing that is a radio and something that isnt a radio
                    }
                }
            }

            return dictionary;
        }*/
    }
}
