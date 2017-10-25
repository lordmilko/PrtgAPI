using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class SensorHistoryResponse : MultiTypeResponse
    {
        private List<SensorHistoryItem> items;

        public SensorHistoryResponse(params SensorHistoryItem[] history)
        {
            items = history.ToList();
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(XmlFunction.HistoricData):
                    return GetHistoricData();
                case nameof(XmlFunction.TableData):
                    return GetTableResponse(address);
                default:
                    throw GetUnknownFunctionException(function);
            }
        }

        private IWebResponse GetTableResponse(string address)
        {
            var content = GetContent(address);

            switch (content)
            {
                case Content.Sensors:
                    return new SensorResponse(new SensorItem());
                default:
                    throw new NotImplementedException($"Unknown content '{content}' requested from {nameof(SensorHistoryResponse)}");
            }
        }

        private IWebResponse GetHistoricData()
        {
            List<XElement> xmlList = items.Select(GetItem).ToList();

            var xml = new XElement("histdata",
                new XAttribute("listend", 1),
                new XAttribute("totalcount", xmlList.Count),
                new XElement("prtg-version", "1.2.3.4"),
                xmlList
            );

            return new BasicResponse(xml.ToString());
        }

        private XElement GetItem(SensorHistoryItem item)
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
