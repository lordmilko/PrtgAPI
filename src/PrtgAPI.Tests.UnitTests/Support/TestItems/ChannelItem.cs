namespace PrtgAPI.Tests.UnitTests.Support.TestItems
{
    public class ChannelItem : BaseItem
    {
        public string LastValue { get; set; }
        public string LastValueRaw { get; set; }
        public string SensorId { get; set; }
        public string GraphVisibility { get; set; }
        public string TableVisibility { get; set; }
        public string ColorMode { get; set; }
        public string LineColor { get; set; }
        public string PercentDisplay { get; set; }
        public string PercentValue { get; set; }
        public string LineWidth { get; set; }
        public string ValueMode { get; set; }
        public string DecimalMode { get; set; }
        public string DecimalPlaces { get; set; }
        public string SpikeFilterEnabled { get; set; }
        public string SpikeFilterMax { get; set; }
        public string SpikeFilterMin { get; set; }
        public string VerticalAxisScaling { get; set; }
        public string VerticalAxisMax { get; set; }
        public string VerticalAxisMin { get; set; }
        public string LimitsEnabled { get; set; }
        public string UpperErrorLimit { get; set; }
        public string LowerWarningLimit { get; set; }
        public string ErrorLimitMessage { get; set; }
        public string WarningLimitMessage { get; set; }
        public string ObjIdRaw { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public ChannelItem(string lastValue = "26 %", string lastValueRaw = "0000000000000260.0000",
            string objId = "1",
            string objIdRaw = "0000000001", string name = "Percent Available Memory", string type = null)
        {
            LastValue = lastValue;
            LastValueRaw = lastValueRaw;
            ObjId = objId;
            ObjIdRaw = objIdRaw;
            Name = name;
            Type = type;
        }
    }
}
