namespace PrtgAPI.Request.Serialization.ValueConverters
{
    abstract class IntValueConverter : ValueConverter<int>
    {
        protected IntValueConverter()
        {
        }

        public override object Deserialize(object value)
        {
            int val;

            if (Convert(value, out val))
                return Deserialize(val);

            return value;
        }

        protected override bool Convert(object value, out int outVal)
        {
            if (value is int)
            {
                outVal = (int)value;
                return true;
            }

            return int.TryParse(value?.ToString(), out outVal);
        }
    }
}
