namespace PrtgAPI.Request.Serialization.ValueConverters
{
    class PositionConverter : ZeroPaddingConverter
    {
        internal static new PositionConverter Instance = new PositionConverter();

        protected PositionConverter()
        {
        }

        private const int Multiplier = 10;

        public static int SerializePosition(int value)
        {
            return Instance.SerializeWithinType(value);
        }

        protected override int SerializeWithinType(int value) => value * Multiplier;

        public override int Deserialize(int value) => value / Multiplier;
    }
}
