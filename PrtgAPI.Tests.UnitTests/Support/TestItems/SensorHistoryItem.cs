namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    public class SensorHistoryItem
    {
        public string DateTime { get; set; }

        public string DateTimeRaw { get; set; }

        public SensorHistoryChannelItem[] Channels { get; set; }

        public string Coverage { get; set; }

        public string CoverageRaw { get; set; }

        public SensorHistoryItem(string datetime = "22/10/2017 3:19:54 PM", string datetimeRaw = "43030.1804871528", SensorHistoryChannelItem[] channels = null, string coverage = "100 %", string coverageRaw = "0000010000")
        {
            if (channels == null)
                channels = GetDefaultChannelItems();

            DateTime = datetime;
            DateTimeRaw = datetimeRaw;
            Channels = channels;
            Coverage = coverage;
            CoverageRaw = coverageRaw;
        }

        internal static SensorHistoryChannelItem[] GetDefaultChannelItems()
        {
            return new[]
            {
                new SensorHistoryChannelItem(),
                new SensorHistoryChannelItem("1", "Available Memory", "1,053 MByte", "1104646144.0000")
            };
        }
    }
}
