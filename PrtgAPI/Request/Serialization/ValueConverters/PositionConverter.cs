namespace PrtgAPI.Request.Serialization.ValueConverters
{
    class PositionConverter : ZeroPaddingConverter
    {
        private const int Multiplier = 10;

        public override int SerializeT(int value) => value * Multiplier;

        public override int Deserialize(int value) => value / Multiplier;
    }
}
