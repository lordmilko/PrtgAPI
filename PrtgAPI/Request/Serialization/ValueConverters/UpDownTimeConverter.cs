using System.Globalization;

namespace PrtgAPI.Request.Serialization.ValueConverters
{
    class UpDownTimeConverter : DoubleValueConverter, IZeroPaddingConverter
    {
        private const int Multiplier = 10000;

        public override string Serialize(double value) => Pad(value, true);

        public override double SerializeT(double value) => value * Multiplier;

        public override double Deserialize(double value) => value / Multiplier;

        public string Pad(object value, bool pad)
        {
            var val = SerializeT((double)value).ToString(CultureInfo.InvariantCulture);

            if (pad)
                return val.PadLeft(15, '0');

            return val;
        }
    }
}
