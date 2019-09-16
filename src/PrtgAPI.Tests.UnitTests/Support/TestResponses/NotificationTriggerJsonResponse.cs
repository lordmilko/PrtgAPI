using System.Linq;
using System.Text;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class NotificationTriggerJsonResponse : IWebResponse
    {
        private NotificationTriggerJsonItem[] items;

        public NotificationTriggerJsonResponse(params NotificationTriggerJsonItem[] items)
        {
            this.items = items;
        }

        public string GetResponseText(ref string address)
        {
            var builder = new StringBuilder();

            builder.Append("{ ");
                builder.Append("\"supported\": [");
                    builder.Append("\"state\",");
                    builder.Append("\"speed\",");
                    builder.Append("\"volume\",");
                    builder.Append("\"threshold\",");
                    builder.Append("\"change\"");
                builder.Append("],");
                builder.Append("\"data\": [");

                builder.Append(string.Join(",", items.Select(i => i.ToString())));

                builder.Append("],");
                builder.Append("\"readonly\":false");
            builder.Append("}");

            return builder.ToString();
        }
    }
}
