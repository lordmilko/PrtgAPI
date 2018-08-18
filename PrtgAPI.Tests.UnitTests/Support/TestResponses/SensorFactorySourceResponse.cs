using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class SensorFactorySourceResponse : MultiTypeResponse
    {
        private Func<string, string> propertyChanger;

        public SensorFactorySourceResponse(Func<string, string> propertyChanger)
        {
            this.propertyChanger = propertyChanger;
        }

        public SensorFactorySourceResponse()
        {
            this.propertyChanger = null;
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(XmlFunction.TableData):
                    return GetTableResponse(address);
                case nameof(HtmlFunction.ObjectData):
                    return new SensorSettingsResponse(propertyChanger);
                case nameof(HtmlFunction.ChannelEdit):
                    return new ChannelResponse(new ChannelItem());
                default:
                    throw GetUnknownFunctionException(function);
            }
        }

        private IWebResponse GetTableResponse(string address)
        {
            var components = UrlHelpers.CrackUrl(address);

            Content content = components["content"].ToEnum<Content>();

            var count = 1;

            var objIds = components.GetValues("filter_objid");

            if (objIds != null)
                count = objIds.Length;

            switch (content)
            {
                case Content.Sensors:
                    return new SensorResponse(Enumerable.Range(0, count).Select(i => new SensorItem()).ToArray());
                case Content.Channels:
                    return new ChannelResponse(new ChannelItem());
                default:
                    throw new NotImplementedException($"Unknown content '{content}' requested from {nameof(SensorFactorySourceResponse)}");
            }
        }
    }
}
