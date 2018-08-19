using System.Text.RegularExpressions;

namespace PrtgAPI.Request.Serialization.ValueConverters
{
    class UrlConverter : ValueConverter<string>
    {
        public override string Serialize(string value) => SerializeT(value);

        public override string Deserialize(string value) => value;

        public override string SerializeT(string value)
        {
            var pattern = "(.+id=)(\\d+)";

            return Regex.Replace(value, pattern, "$2");
        }

        protected override bool Convert(object value, out string outVal)
        {
            outVal = value?.ToString();
            return true;
        }
    }
}
