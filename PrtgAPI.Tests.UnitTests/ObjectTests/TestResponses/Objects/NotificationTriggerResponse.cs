using System;
using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class NotificationTriggerResponse : BaseResponse<NotificationTriggerItem>
    {
        private ChannelItem[] channels;

        internal NotificationTriggerResponse(params NotificationTriggerItem[] triggers) : base("triggers", triggers)
        {
            channels = new[] {new ChannelItem()};
        }

        internal NotificationTriggerResponse(NotificationTriggerItem[] triggers, ChannelItem[] channels) : this(triggers)
        {
            this.channels = channels;
        }

        public override string GetResponseText(ref string address)
        {
            var function = MultiTypeResponse.GetFunction(address);

            switch (function)
            {
                case nameof(XmlFunction.TableData):
                    return GetTableText(address);
                case nameof(HtmlFunction.ChannelEdit):
                    return new ChannelResponse(channels).GetResponseText(ref address);
                case nameof(HtmlFunction.EditNotification):
                    return new NotificationActionResponse(new NotificationActionItem()).GetResponseText(ref address);
                default:
                    throw new NotImplementedException($"Unknown function '{function}' passed to {nameof(NotificationTriggerResponse)}");
            }
        }

        private string GetTableText(string address)
        {
            var content = MultiTypeResponse.GetContent(address);

            switch (content)
            {
                case Content.Triggers:
                    return base.GetResponseText(ref address);
                case Content.Channels:
                    return new ChannelResponse(channels).GetResponseText(ref address);
                default:
                    throw new NotImplementedException($"Unknown content '{content}' requested from {nameof(NotificationTriggerResponse)}");
            }
        }

        public override XElement GetItem(NotificationTriggerItem item)
        {
            var xml = new XElement("item",
                new XElement("content", item.Content),
                new XElement("objid", item.ObjId)
            );

            return xml;
        }
    }
}
