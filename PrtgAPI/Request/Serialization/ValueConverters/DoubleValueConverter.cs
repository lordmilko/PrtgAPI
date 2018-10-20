namespace PrtgAPI.Request.Serialization.ValueConverters
{
    abstract class DoubleValueConverter : ValueConverter<double>
    {
        protected DoubleValueConverter()
        {
        }

        public override object Deserialize(object value)
        {
            double val;

            if (Convert(value, out val))
                return Deserialize(val);

            return value;
        }

        protected override bool Convert(object value, out double outVal)
        {
            if (value is double)
            {
                outVal = (double)value;
                return true;
            }

            return double.TryParse(value?.ToString(), out outVal);
        }
    }
}