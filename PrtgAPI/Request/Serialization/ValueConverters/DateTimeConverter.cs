namespace PrtgAPI.Request.Serialization.ValueConverters
{
    //todo: need a unit test that says all datetime/datetime? properties have this converter
    class DateTimeConverter : DoubleValueConverter, IZeroPaddingConverter
    {
        internal static DateTimeConverter Instance = new DateTimeConverter();

        protected DateTimeConverter()
        {
        }

        public override string Serialize(double value) => Pad(value, true);

        public override double Deserialize(double value) => value;

        protected override double SerializeWithinType(double value) => value;

        public string Pad(object value, bool pad)
        {
            var val = SerializeWithinType((double)value);

            if (pad)
                return val.ToString().PadRight(16, '0');

            return val.ToString();
        }
    }
}
