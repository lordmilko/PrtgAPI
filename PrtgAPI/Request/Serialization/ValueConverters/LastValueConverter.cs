using System;

namespace PrtgAPI.Request.Serialization.ValueConverters
{
    class LastValueConverter : DoubleValueConverter, IZeroPaddingConverter
    {
        internal static LastValueConverter Instance = new LastValueConverter();

        protected LastValueConverter()
        {
        }

        private const int Multiplier = 10;

        public override string Serialize(double value) => Pad(value, true);

        protected override double SerializeWithinType(double value)
        {
            var whole = Math.Floor(value);

            var @decimal = value - whole;

            var result = ((int)(whole * Multiplier)) + @decimal;

            return result;
        }

        public override double Deserialize(double value) => value;

        public string Pad(object value, bool pad)
        {
            var d = SerializeWithinType((double)value);

            if (!pad)
            {
                if (d % 1 == 0)
                {
                    //It's an integer
                    return value.ToString();
                }

                return d.ToString("#.0000");
            }

            return d.ToString("#.0000").PadLeft(21, '0');
        }
    }
}
