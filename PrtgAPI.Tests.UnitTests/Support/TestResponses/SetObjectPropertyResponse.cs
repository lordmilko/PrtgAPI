using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Helpers;
using PrtgAPI.Request.Serialization.Cache;
using PrtgAPI.Parameters;
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
                case nameof(JsonFunction.GetStatus):
                    return new ServerStatusResponse(new ServerStatusItem());
                case nameof(JsonFunction.GeoLocator):
                    return new BasicResponse(GetLocationResponse(address));
                default:
                    throw GetUnknownFunctionException(function);
            }
        }

        private string GetSetObjectPropertyResponse(string address)
        {
            var queries = UrlHelpers.CrackUrl(address);

            PropertyCache cache;

            if (typeof(TObjectProperty) == typeof(ObjectProperty))
            {
                cache = BaseSetObjectPropertyParameters<TObjectProperty>.GetPropertyInfoViaTypeLookup((Enum) (object) property);
            }
            else if (typeof (TObjectProperty) == typeof (ChannelProperty))
            {
                cache = BaseSetObjectPropertyParameters<TObjectProperty>.GetPropertyInfoViaPropertyParameter<Channel>((Enum) (object) property);
            }
            else
                throw new NotImplementedException($"Handler for object property type {nameof(TObjectProperty)} is not implemented");

            var queryName = BaseSetObjectPropertyParameters<TObjectProperty>.GetParameterNameStatic((Enum) (object) property, cache);

            if (typeof (TObjectProperty) == typeof (ChannelProperty))
                queryName += "1"; //Channel ID used for tests

            var val = queries[queryName];

            Assert.IsTrue(val == expectedValue, $"The value of property '{property.ToString().ToLower()}' did not match the expected value. Expected '{expectedValue}', received: '{val}'");

            return "OK";
        }

        private string GetLocationResponse(string address)
        {
            var builder = new StringBuilder();

            builder.Append("{");
            builder.Append("   \"results\" : [");
            builder.Append("      {");
            builder.Append("         \"formatted_address\" : \"23 Fleet St, Boston, MA 02113, USA\",");
            builder.Append("         \"geometry\" : {");
            builder.Append("            \"location\" : {");
            builder.Append("               \"lat\" : 42.3643847,");
            builder.Append("               \"lng\" : -71.05279969999999");
            builder.Append("            }");
            builder.Append("         }");
            builder.Append("      }");
            builder.Append("   ]");
            builder.Append("}");

            return builder.ToString();
        }
    }
}
