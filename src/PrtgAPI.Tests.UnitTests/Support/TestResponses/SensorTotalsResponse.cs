using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class SensorTotalsResponse : IWebResponse
    {
        protected SensorTotalsItem item;

        public SensorTotalsResponse(SensorTotalsItem item)
        {
            this.item = item;
        }

        public string GetResponseText(ref string address)
        {
            var xml = new XElement("data",
                new XElement("prtg-version", item.PrtgVersion),
                new XElement("summary", item.Summary),
                new XElement("upsens", item.UpSens),
                new XElement("downsens", item.DownSens),
                new XElement("warnsens", item.WarnSens),
                new XElement("downacksens", item.DownAckSens),
                new XElement("partialdownsens", item.PartialDownSens),
                new XElement("unusualsens", item.UnusualSens),
                new XElement("pausedsens", item.PausedSens),
                new XElement("undefinedsens", item.UndefinedSens),
                new XElement("totalsens", item.TotalSens),
                new XElement("servertime", item.ServerTime)
            );

            return xml.ToString();
        }
    }
}
