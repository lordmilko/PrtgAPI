using System;
using System.Collections.Generic;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class SetNotificationTriggerResponse : MultiTypeResponse
    {
        public SetNotificationTriggerResponse()
        {
        }

        public SetNotificationTriggerResponse(Dictionary<Content, int> countOverride)
        {
            CountOverride = countOverride;
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(HtmlFunction.EditSettings):
                    return new BasicResponse(string.Empty);
                case nameof(XmlFunction.TableData):
                    return GetTableResponse(address);
                case nameof(JsonFunction.Triggers):
                    return GetSupportedTriggersResponse();
                case nameof(HtmlFunction.ChannelEdit):
                    return new ChannelResponse(new ChannelItem());
                case nameof(HtmlFunction.RemoveSubObject):
                    return new BasicResponse(string.Empty);
                case nameof(HtmlFunction.EditNotification):
                case nameof(HtmlFunction.ObjectData):
                    return GetObjectDataResponse(address);
                default:
                    throw GetUnknownFunctionException(function);
            }
        }

        private IWebResponse GetTableResponse(string address)
        {
            var components = UrlUtilities.CrackUrl(address);

            Content content = components["content"].DescriptionToEnum<Content>();

            switch (content)
            {
                case Content.Sensors:
                    if (components["filter_objid"] == "1")
                        return new SensorResponse();
                    if (CountOverride != null && CountOverride[Content.Sensors] == 0)
                        return new SensorResponse();
                    return new SensorResponse(new SensorItem());
                case Content.Channels:
                    return new ChannelResponse(new ChannelItem());
                case Content.Triggers:
                    return new NotificationTriggerResponse(NotificationTriggerItem.StateTrigger());
                case Content.Notifications:
                    return new NotificationActionResponse(new NotificationActionItem("301"));
                case Content.Schedules:
                    return new ScheduleResponse(new ScheduleItem());
                case Content.Objects:
                    if (components["filter_objid"] == "4000")
                        return new ObjectResponse(new SensorItem());

                    return new ObjectResponse(new DeviceItem());
                default:
                    throw new NotImplementedException($"Unknown content '{content}' requested from {nameof(SetNotificationTriggerResponse)}");
            }
        }

        private IWebResponse GetObjectDataResponse(string address)
        {
            var components = UrlUtilities.CrackUrl(address);

            var objectType = components["objecttype"].ToEnum<ObjectType>();

            switch (objectType)
            {
                case ObjectType.Notification:
                    return new NotificationActionResponse(new NotificationActionItem());
                case ObjectType.Schedule:
                    return new ScheduleResponse();
                default:
                    throw new NotImplementedException($"Unknown object type '{objectType}' requested from {nameof(MultiTypeResponse)}");
            }
        }

        private IWebResponse GetSupportedTriggersResponse()
        {
            return new BasicResponse("{ \"supported\": [\"threshold\"] }");
        }
    }
}
