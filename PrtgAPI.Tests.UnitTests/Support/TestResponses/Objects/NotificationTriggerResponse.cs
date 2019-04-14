using System;
using System.Xml.Linq;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class NotificationTriggerResponse : BaseResponse<NotificationTriggerItem>
    {
        private ChannelItem[] channels;

        private int notificationActionId = 300;

        public int[] HasSchedule { get; set; }

        internal NotificationTriggerResponse(params NotificationTriggerItem[] triggers) : base("triggers", triggers)
        {
            channels = new[] {new ChannelItem()};
        }

        public NotificationTriggerResponse(NotificationTriggerItem[] triggers, ChannelItem[] channels) : this(triggers)
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
                case nameof(HtmlFunction.ObjectData):
                    return GetObjectDataResponse(address).GetResponseText(ref address);
                default:
                    throw new NotImplementedException($"Unknown function '{function}' passed to {nameof(NotificationTriggerResponse)}");
            }
        }

        private string GetTableText(string address)
        {
            var components = UrlUtilities.CrackUrl(address);

            var content = MultiTypeResponse.GetContent(address);

            switch (content)
            {
                case Content.Triggers:
                    return base.GetResponseText(ref address);
                case Content.Channels:

                    if (Convert.ToInt32(components["id"]) >= 4000)
                        return new ChannelResponse(channels).GetResponseText(ref address);
                    return new ChannelResponse().GetResponseText(ref address);
                case Content.Notifications:
                    return new NotificationActionResponse(new NotificationActionItem("301"), new NotificationActionItem("302"), new NotificationActionItem("303")).GetResponseText(ref address);
                case Content.Schedules:
                    return new ScheduleResponse(new ScheduleItem()).GetResponseText(ref address);
                default:
                    throw new NotImplementedException($"Unknown content '{content}' requested from {nameof(NotificationTriggerResponse)}");
            }
        }

        private IWebResponse GetObjectDataResponse(string address)
        {
            var components = UrlUtilities.CrackUrl(address);

            var objectType = components["objecttype"].ToEnum<ObjectType>();

            switch (objectType)
            {
                case ObjectType.Notification:
                    notificationActionId++;

                    return new NotificationActionResponse(new NotificationActionItem(notificationActionId.ToString()))
                    {
                        HasSchedule = HasSchedule
                    };
                case ObjectType.Schedule:
                    return new ScheduleResponse();
                default:
                    throw new NotImplementedException($"Unknown object type '{objectType}' requested from {nameof(MultiTypeResponse)}");
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
