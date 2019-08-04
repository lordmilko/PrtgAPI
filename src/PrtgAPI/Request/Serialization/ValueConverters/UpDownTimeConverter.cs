using System.Globalization;

namespace PrtgAPI.Request.Serialization.ValueConverters
{
    class UpDownTimeConverter : DoubleValueConverter, IZeroPaddingConverter
    {
        internal static UpDownTimeConverter Instance = new UpDownTimeConverter();

        protected UpDownTimeConverter()
        {
        }

        private const int Multiplier = 10000;

        public override string Serialize(double value) => Pad(value, true);

        protected override double SerializeWithinType(double value) => value * Multiplier;

        public override double Deserialize(double value) => value / Multiplier;

        public string Pad(object value, bool pad)
        {
            var val = SerializeWithinType((double)value).ToString(CultureInfo.InvariantCulture);

            if (pad)
                return val.PadLeft(15, '0');

            return val;
        }
    }
}
