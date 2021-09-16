using System.Linq;
using System.Text;
using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class SensorHistoryReportResponse : IWebResponse
    {
        private bool hasResponse;

        public SensorHistoryReportResponse(bool hasResponse)
        {
            this.hasResponse = hasResponse;
        }

        public string GetResponseText(ref string address)
        {
            var builder = new StringBuilder();

            builder.Append("<table cellspacing=0 class=\" table hoverable statehistory\" id=\"table_statereporttable\">\r\n");
            builder.Append("<colgroup><col class=\"col-status\"/><col class=\"col-datetime\"/></colgroup>\r\n");
            builder.Append("<thead class=\"headerswithlinks\"><tr><th>Status</th><th>Date Time</th></tr></thead>\r\n<tbody>");

            if (hasResponse)
            {
                //New
                builder.Append("<tr class=\"odd\">");
                builder.Append("<td class=\"col-status\" data-th=\"Status\"><div class=\"colorflag colorflag_statusunknown\">&nbsp;</div>&nbsp;Unknown</td>");
                builder.Append("<td class=\"col-datetime\" data-th=\"Date Time\"><nobr>8/05/2021 7:31:16 PM − 8/05/2021 7:31:34 PM <span class=\"statehistorybar\">");
                builder.Append("<span style=\"background-color:#808282;color:#fff\">&nbsp;&nbsp;</span></span> <span class=\"percent\">(=18s)</span></nobr></td>");
                builder.Append("</tr>");

                //Old
                builder.Append("<tr class=\"even\"><td class=\"col-status\"><div class=\"colorflag\" style=\"background-color:#b4cc38\">&nbsp;</div>&nbsp;Up</td>");
                builder.Append("<td class=\"col-datetime\"><nobr>8/05/2021 7:18:55 PM &minus; 8/05/2021 8:17:55 PM <span class=\"statehistorybar\">");
                builder.Append("<span style=\"background-color:#b4cc38\" >&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span></span> <span class=\"percent\">(=58 m 59 s)</span></nobr></td></tr>\r\n");

                //Old Foreign
                builder.Append("<tr class=\"odd\"><td class=\"col-status\"><div class=\"colorflag\" style=\"background-color:#808282\">&nbsp;</div>&nbsp;不明</td>");
                builder.Append("<td class=\"col-datetime\"><nobr>8/05/2021 8:17:55 PM &minus; 8/05/2021 8:18:55 PM <span class=\"statehistorybar\">");
                builder.Append("<span style=\"background-color:#808282;color:#fff\" ></span></span> <span class=\"percent\">(=59 s)</span></nobr></td></tr>\r\n");
            }
            else
            {
                builder.Append("<tr><td>-</td>\r\n<td>-</td></tr>");
            }
            
            builder.Append("</tbody></table>");

            return builder.ToString();
        }
    }

    public class SensorHistoryResponse : BaseResponse<SensorHistoryItem>
    {
        public ChannelItem[] Channels { get; set; }

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

        public override string GetResponseText(ref string address)
        {
            if (address.Contains(XmlFunction.TableData.GetDescription()) || address.Contains(HtmlFunction.ChannelEdit.GetDescription()))
            {
                return new ChannelResponse(Channels ?? new ChannelItem[0]).GetResponseText(ref address);
            }

            return base.GetResponseText(ref address);
        }
    }
}
