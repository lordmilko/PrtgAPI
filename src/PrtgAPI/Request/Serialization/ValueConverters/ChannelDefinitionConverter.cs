using System.Text.RegularExpressions;

namespace PrtgAPI.Request.Serialization.ValueConverters
{
    class ChannelDefinitionConverter : ValueConverter<string>
    {
        internal static ChannelDefinitionConverter Instance = new ChannelDefinitionConverter();

        public override string Serialize(string value) => Regex.Replace(value, @"\r\n|\n\r|\n|\r", "\r\n");

        public override string Deserialize(string value) => value;

        protected override string SerializeWithinType(string value) => value;

        protected override bool Convert(object value, out string outVal)
        {
            outVal = value?.ToString();
            return true;
        }
    }
}