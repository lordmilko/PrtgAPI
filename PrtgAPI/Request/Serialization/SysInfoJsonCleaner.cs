using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PrtgAPI.Request.Serialization
{
    class SysInfoJsonCleaner
    {
        const string objectPattern = "{\"\":\"*.+?\"*}(,|])";
        const string propertyNamePattern = "\"\":";

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

            if(firstMatch.Success)
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

            for (var i = properties.Count - 1; i >= 0; i--)
            {
                CleanProperty(properties[i], obj, i);
            }
        }

        private void CleanProperty(Match property, Match obj, int propertyIndex)
        {
            var absoluteIndex = GetAbsolutePropertyIndex(property, obj);

            var name = columns[propertyIndex];

            ReplaceAt(absoluteIndex, property.Length, $"\"{name}\":");
        }

        private int GetAbsolutePropertyIndex(Match property, Match obj)
        {
            //Given the position of a property in an object, get the absolute position of that property
            //in the original string
            return obj.Index + property.Index;
        }

        private void ReplaceAt(int index, int length, string str)
        {
            builder.Remove(index, length);
            builder.Insert(index, str);
        }
    }
}
