using System;

namespace PrtgAPI.Request.Serialization.ValueConverters
{
    class ValueLookupConverter : IValueConverter
    {
        internal static ValueLookupConverter Instance = new ValueLookupConverter();

        public object Serialize(object value)
        {
            var none = "|None";

            if (value != null)
            {
                if (value.ToString().Equals("None", StringComparison.OrdinalIgnoreCase))
                    return none;
                else
                    return $"{value}|{value}";
            }
            else
                return none;
        }

        public object Deserialize(object value) => value;
    }
}
