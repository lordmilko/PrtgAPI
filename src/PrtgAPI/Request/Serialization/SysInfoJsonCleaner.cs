using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PrtgAPI.Request.Serialization
{
    class SysInfoJsonCleaner
    {
        const string objectPattern = "{\"\":\"*.+?\"*}(,|])";
        const string propertyNamePattern = "(\"\":|\"_raw\":)";

        string response;
        StringBuilder builder;
        SysInfoProperty[] columns;

        public SysInfoJsonCleaner(string response, SysInfoProperty[] columns)
        {
            this.response = response;
            builder = new StringBuilder(response);
            this.columns = columns;
        }

        public static string Clean(string response, SysInfoProperty[] columns)
        {
            var cleaner = new SysInfoJsonCleaner(response, columns);

            cleaner.Validate();

            return cleaner.CleanInternal();
        }

        private void Validate()
        {
            var missingPropertyPattern = "\"\":\".+?\",\"_raw\":\"\"";

            var firstMatch = Regex.Match(response, missingPropertyPattern);

            if (firstMatch.Success)
            {
                //We have at least one missing property. Check whether the entire response consists of
                //objects where every property is missing
                var obj = "{" + string.Join(",", Enumerable.Range(0, columns.Length).Select(i => firstMatch.Value)) + "}";

                var objects = Regex.Matches(response, objectPattern).Cast<Match>().Reverse().ToList();

                if (objects.All(o => o.Value.TrimEnd(']', ',') == obj))
                    throw new PrtgRequestException("Failed to receive System Information: content type not supported PRTG Server.");
            }
        }

        private string CleanInternal()
        {
            var objects = Regex.Matches(response, objectPattern).Cast<Match>().Reverse().ToList();

            foreach (var obj in objects)
                CleanObject(obj);

            return builder.ToString();
        }

        private void CleanObject(Match obj)
        {
            var properties = Regex.Matches(obj.Value, propertyNamePattern).Cast<Match>().ToList();
            var normalProperties = properties.Where(p => p.Value == "\"\":").ToList();

            for (var i = properties.Count - 1; i >= 0; i--)
            {
                CleanValue(properties, obj, i);

                if (properties[i].Value != "\"_raw\":")
                {
                    var match = properties[i];
                    CleanProperty(match, obj, normalProperties.IndexOf(match));
                }
            }
        }

        private void CleanProperty(Match property, Match obj, int propertyIndex)
        {
            var absoluteIndex = GetAbsolutePropertyIndex(property, obj);

            var name = columns[propertyIndex];

            ReplaceAt(absoluteIndex, property.Length, $"\"{name}\":");
        }

        /// <summary>
        /// Adds missing quotes from values
        /// </summary>
        /// <param name="properties">A list of matches for the string "": or "_raw":. Position is relative within <paramref name="obj"/>.</param>
        /// <param name="obj">The current object {"":"val1","":"val2"}</param>
        /// <param name="propertyIndex">The index of this property within <paramref name="obj"/> after excluding all _raw properties</param>
        private void CleanValue(List<Match> properties, Match obj, int propertyIndex)
        {
            var propertyName = properties[propertyIndex];

            //The index of the first character in "":"val1"
            var absoluteIndexKeyValue = GetAbsolutePropertyIndex(propertyName, obj);
            var absoluteIndexOfVal = absoluteIndexKeyValue + propertyName.Length;

            var ch = builder[absoluteIndexOfVal];

            if (ch != '"')
            {
                var isLastProperty = propertyIndex == properties.Count - 1;

                if (isLastProperty)
                {
                    var relativeEndBrace = obj.Value.LastIndexOf("}");
                    var absoluteEndBrace = GetAbsolutePosition(relativeEndBrace, obj);
                    var absolutePropertyValueEnd = absoluteEndBrace - 1;

                    var propertyLength = absolutePropertyValueEnd - absoluteIndexOfVal + 1;

                    builder.Insert(absoluteEndBrace, "\"");
                    builder.Insert(absoluteIndexOfVal, "\"");

                    var propertyValue = builder.ToString().Substring(absoluteIndexOfVal - 1, propertyLength + 3);
                }
                else
                {
                    var absoluteNextObjectStart = GetAbsolutePropertyIndex(properties[propertyIndex + 1], obj);            //The absolute position the next object starts at within builder
                    var absolutePropertyValueEnd = absoluteNextObjectStart - 2;                                            //The absolute final character of this object's value before the comma

                    var propertyLength = absolutePropertyValueEnd - absoluteIndexOfVal + 1;

                    builder.Insert(absolutePropertyValueEnd + 1, "\"");
                    builder.Insert(absoluteIndexOfVal, "\"");

                    var propertyValue = builder.ToString().Substring(absoluteIndexOfVal - 1, propertyLength + 5);
                }
            }
        }

        private int GetAbsolutePropertyIndex(Match property, Match obj)
        {
            //Given the position of a property in an object, get the absolute position of that property
            //in the original string
            return obj.Index + property.Index;
        }

        private int GetAbsolutePosition(int relativePos, Match obj)
        {
            return obj.Index + relativePos;
        }

        private void ReplaceAt(int index, int length, string str)
        {
            builder.Remove(index, length);
            builder.Insert(index, str);
        }
    }
}
