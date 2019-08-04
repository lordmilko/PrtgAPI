using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SetObjectPropertyResponse<TObjectProperty> : MultiTypeResponse
    {
        private TObjectProperty property;
        private string expectedValue;

        public SetObjectPropertyResponse(TObjectProperty property, string expectedValue)
        {
            this.property = property;
            this.expectedValue = expectedValue;
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(HtmlFunction.EditSettings):
                    return new BasicResponse(GetSetObjectPropertyResponse(address));
                case nameof(XmlFunction.TableData):
                case nameof(HtmlFunction.ChannelEdit):
                    return base.GetResponse(ref address, function);
                case nameof(JsonFunction.GetStatus):
                    return new ServerStatusResponse(new ServerStatusItem());
                case nameof(JsonFunction.GeoLocator):
                    return new GeoLocatorResponse();
                default:
                    throw GetUnknownFunctionException(function);
            }
        }

        private string GetSetObjectPropertyResponse(string address)
        {
            var queries = UrlUtilities.CrackUrl(address);

            PropertyCache cache;

            if (typeof(TObjectProperty) == typeof(ObjectProperty))
            {
                cache = ObjectPropertyParser.GetPropertyInfoViaTypeLookup((Enum) (object) property);
            }
            else if (typeof (TObjectProperty) == typeof (ChannelProperty))
            {
                cache = ObjectPropertyParser.GetPropertyInfoViaPropertyParameter<Channel>((Enum) (object) property);
            }
            else
                throw new NotImplementedException($"Handler for object property type {nameof(TObjectProperty)} is not implemented");

            var queryName = ObjectPropertyParser.GetObjectPropertyNameViaCache((Enum) (object) property, cache);

            if (typeof (TObjectProperty) == typeof (ChannelProperty))
                queryName += "1"; //Channel ID used for tests

            var val = queries[queryName];

            Assert.IsTrue(val == expectedValue, $"The value of property '{property.ToString().ToLower()}' did not match the expected value. Expected '{expectedValue}', received: '{val}'");

            return "OK";
        }
    }
}
