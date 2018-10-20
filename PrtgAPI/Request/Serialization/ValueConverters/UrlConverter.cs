using System.Text.RegularExpressions;

namespace PrtgAPI.Request.Serialization.ValueConverters
{
    class UrlConverter : ValueConverter<string>
    {
        internal static UrlConverter Instance = new UrlConverter();

        protected UrlConverter()
        {
        }

        public override string Serialize(string value) => SerializeWithinType(value);

        public override string Deserialize(string value) => value;

        protected override string SerializeWithinType(string value)
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
