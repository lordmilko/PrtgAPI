using System.Linq;
using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class SensorHistoryResponse : BaseResponse<SensorHistoryItem>
    {
        public SensorHistoryResponse(params SensorHistoryItem[] history) : base ("histdata", history)
        {
        }

        public override XElement GetItem(SensorHistoryItem item)
        {
            var xml = new XElement("item",
                new XElement("datetime", item.DateTime),
                new XElement("datetime_raw", item.DateTimeRaw),
                item.Channels.Select(c => new XElement("value",
                    new XAttribute("channelid", c.Id),
                    new XAttribute("channel", c.Name),
                    c.Value)),
                item.Channels.Select(c => new XElement("value_raw",
                    new XAttribute("channelid", c.Id),
                    new XAttribute("channel", c.Name),
                    c.ValueRaw)),
                new XElement("coverage", item.Coverage),
                new XElement("coverage_raw", item.CoverageRaw)
            );

            return xml;
        }
    }
}
