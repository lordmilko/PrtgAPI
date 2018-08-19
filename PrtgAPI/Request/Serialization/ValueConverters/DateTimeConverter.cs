namespace PrtgAPI.Request.Serialization.ValueConverters
{
    //todo: need a unit test that says all datetime/datetime? properties have this converter
    class DateTimeConverter : DoubleValueConverter, IZeroPaddingConverter
    {
        public override string Serialize(double value) => Pad(value, true);

        public override double Deserialize(double value) => value;

        public override double SerializeT(double value) => value;

        public string Pad(object value, bool pad)
        {
            var val = SerializeT((double)value);

            if (pad)
                return val.ToString().PadRight(16, '0');

            return val.ToString();
        }
    }
}
