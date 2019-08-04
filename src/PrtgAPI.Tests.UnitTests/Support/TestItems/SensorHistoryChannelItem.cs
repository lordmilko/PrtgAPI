namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    public class SensorHistoryChannelItem
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public string ValueRaw { get; set; }

        public SensorHistoryChannelItem(string id = "0", string name = "Percent Available Memory", string value = "51 %", string valueRaw = "51.0000")
        {
            Id = id;
            Name = name;
            Value = value;
            ValueRaw = valueRaw;
        }
    }
}
