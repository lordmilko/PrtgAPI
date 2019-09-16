using System;
using System.Linq;
using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class NotificationTriggerTranslationResponse : BaseResponse<NotificationTriggerItem>
    {
        private NotificationTriggerItem[][] xml;
        private NotificationTriggerJsonItem[][] json;

        private int jsonIndex = -1;
        private int xmlIndex = -1;

        private bool isEnglish;

        public NotificationTriggerTranslationResponse(NotificationTriggerItem xml, NotificationTriggerJsonItem json, bool isEnglish) :
            this(new[] { new[] { xml } }, new[] { new[] { json } }, isEnglish)
        {
        }

        public NotificationTriggerTranslationResponse(NotificationTriggerItem[][] xml, NotificationTriggerJsonItem[][] json, bool isEnglish) : base("triggers", new NotificationTriggerItem[] {})
        {
            this.xml = xml;
            this.json = json;
            this.isEnglish = isEnglish;
        }

        public override string GetResponseText(ref string address)
        {
            var function = MultiTypeResponse.GetFunction(address);

            switch (function)
            {
                case nameof(XmlFunction.TableData):
                    return GetTableText(address);
                case nameof(JsonFunction.Triggers):
                    if (json.Length == 1)
                        jsonIndex = 0;
                    else
                        jsonIndex++;

                    return new NotificationTriggerJsonResponse(json[jsonIndex]).GetResponseText(ref address);
                case nameof(HtmlFunction.ObjectData):
                    return GetObjectDataResponse(address).GetResponseText(ref address);
                case nameof(HtmlFunction.ChannelEdit):
                    return new ChannelResponse().GetResponseText(ref address);
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
                    if (xml.Length == 1)
                        xmlIndex = 0;
                    else
                        xmlIndex++;

                    Items = xml[xmlIndex].ToList();

                    return base.GetResponseText(ref address);
                case Content.Channels:
                    return new ChannelResponse(new[] {new ChannelItem()}).GetResponseText(ref address);
                case Content.Objects:
                    if (Convert.ToInt32(components["filter_objid"]) >= 4000)
                        return new ObjectResponse(new SensorItem()).GetResponseText(ref address);
                    return new ObjectResponse(new DeviceItem()).GetResponseText(ref address);
                case Content.Notifications:
                case Content.Schedules:
                    return new NotificationActionResponse(new NotificationActionItem("301"), new NotificationActionItem("302"), new NotificationActionItem("303")).GetResponseText(ref address);
                default:
                    throw new NotImplementedException($"Unknown content '{content}' requested from {nameof(NotificationTriggerResponse)}");
            }
        }

        private IWebResponse GetObjectDataResponse(string address)
        {
            var components = UrlUtilities.CrackUrl(address);

            var objectType = components["objecttype"]?.DescriptionToEnum<ObjectType>();

            if (objectType == null && components["id"] == "810")
                objectType = ObjectType.WebServerOptions;

            switch (objectType)
            {
                case ObjectType.WebServerOptions:
                    return new WebServerOptionsResponse(isEnglish);
                case ObjectType.Notification:
                case ObjectType.Schedule:
                    return new NotificationActionResponse(new NotificationActionItem("301"), new NotificationActionItem("302"), new NotificationActionItem("303"));
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
