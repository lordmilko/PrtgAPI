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
        protected static XElement GetXmlInternal(string response, int channelId, string basicMatchRegex, string nameRegex, Func<string, string> nameTransformer)
        {
            //original channel code:
            //                Name = Regex.Replace(input, ".Replace($"_{channelId}", ""),


            

            var inputXml = GetInputXml(response, basicMatchRegex, nameRegex, nameTransformer);
            var ddlXml = GetDropDownListXml(response, nameRegex);
            
            var elm = new XElement("properties", inputXml, ddlXml);
            return elm;
        }

        private static List<XElement> GetInputXml(string response, string basicMatchRegex, string nameRegex, Func<string, string> nameTransformer)
        {
            var matches = Regex.Matches(response, basicMatchRegex);
            var inputs = (matches.Cast<Match>().Select(match => match.Value)).ToList();

            var properties = GetProperties(inputs, nameRegex, nameTransformer);

            var props = FilterInputTags(properties).Select(v => new XElement($"injected_{v.Value.Name}", v.Value.Value));

            return props.ToList();
        }

        private static List<XElement> GetDropDownListXml(string response, string nameRegex)
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
                Radio = Regex.Match(input, "radio").Success,
                Checked = Regex.Match(input, "checked").Success,
                Hidden = Regex.Match(input, "type=\"hidden\"").Success
            }).Where(p => !p.Hidden).ToList();

            return properties;
        }

        private static List<DropDownList> GetLists(List<string> lists, string nameRegex)
        {
            List<DropDownList> blah = new List<DropDownList>();

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
            }

            return blah;

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

        internal static Dictionary<string, Input> FilterInputTags(List<Input> properties)
        {
            var dictionary = new Dictionary<string, Input>();

            foreach (var prop in properties)
            {
                if (!dictionary.ContainsKey(prop.Name))
                    dictionary.Add(prop.Name, prop);
                else
                {
                    if (prop.Radio && dictionary[prop.Name].Radio) //if theyre both radio buttons
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
                        throw new NotImplementedException("We had two channel properties that arent radio buttons, or two properties with the same name with only one of them a radio");
                        //crash! either we have a duplicate of two things that arent radios, or a thing that is a radio and something that isnt a radio
                    }
                }
            }

            return dictionary;
        }
    }
}
