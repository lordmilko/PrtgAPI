using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.ObjectTests.Items;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Responses
{
    public class ModificationHistoryResponse : BaseResponse<ModificationHistoryItem>
    {
        public ModificationHistoryResponse(ModificationHistoryItem[] items) : base("history", items)
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
