namespace PrtgAPI.Request.Serialization.ValueConverters
{
    class ZeroPaddingConverter : IntValueConverter, IZeroPaddingConverter
    {
        internal static ZeroPaddingConverter Instance = new ZeroPaddingConverter();

        protected ZeroPaddingConverter()
        {
        }

        public override string Serialize(int value) => Pad(value, true);

        public override int Deserialize(int value) => value;

        protected override int SerializeWithinType(int value) => value;

        public string Pad(object value, bool pad)
        {
            var val = SerializeWithinType((int)value);

            if (pad)
                return val.ToString().PadLeft(10, '0');

            return val.ToString();
        }
    }
}
