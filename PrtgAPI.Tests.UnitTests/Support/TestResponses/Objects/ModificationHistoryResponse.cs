using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class ModificationHistoryResponse : BaseResponse<ModificationHistoryItem>
    {
        public ModificationHistoryResponse(params ModificationHistoryItem[] items) : base("history", items)
        {
        }

        public override XElement GetItem(ModificationHistoryItem item)
        {
            var xml = new XElement("item",
                new XElement("datetime", item.DateTime),
                new XElement("datetime_raw", item.DateTimeRaw),
                new XElement("user", item.User),
                new XElement("message", item.Message),
                new XElement("message_raw", item.MessageRaw)
            );

            return xml;
        }
    }
}
