using System;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SetChannelPropertyResponse : MultiTypeResponse
    {
        private ChannelProperty property;
        private int channelId;
        private object value;

        public SetChannelPropertyResponse(ChannelProperty property, int channelId, object value)
        {
            this.property = property;
            this.channelId = channelId;
            this.value = value;
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            if (function == nameof(JsonFunction.GetStatus))
                return new ServerStatusResponse(new ServerStatusItem());
            if (function == nameof(XmlFunction.TableData) || function == nameof(HtmlFunction.ChannelEdit))
                return base.GetResponse(ref address, function);

            var queries = UrlUtilities.CrackUrl(address);
            queries.Remove("id");
            queries.Remove("username");
            queries.Remove("passhash");

            if (property == ChannelProperty.LimitsEnabled)
            {
                if (Convert.ToBoolean(value))
                {
                    AssertCollectionLength(address, queries, 1);
                    KeyExistsWithCorrectValue(queries, "limitmode", Convert.ToInt32(value));
                }
                else
                {
                    KeyExistsWithCorrectValue(queries, "limitmode", Convert.ToInt32(value));
                    KeyExistsWithCorrectValue(queries, "limitmaxerror", string.Empty);
                    KeyExistsWithCorrectValue(queries, "limitmaxwarning", string.Empty);
                    KeyExistsWithCorrectValue(queries, "limitminerror", string.Empty);
                    KeyExistsWithCorrectValue(queries, "limitminwarning", string.Empty);
                    KeyExistsWithCorrectValue(queries, "limiterrormsg", string.Empty);
                    KeyExistsWithCorrectValue(queries, "limitwarningmsg", string.Empty);
                }
            }
            else if (property == ChannelProperty.UpperErrorLimit)
            {
                //If no value was specified, we didn't need to include a factor
                var limitMaxErrorCount = string.IsNullOrEmpty(queries["limitmaxerror_1"]) ? 2 : 3;

                AssertCollectionLength(address, queries, limitMaxErrorCount);

                KeyExistsWithCorrectValue(queries, "limitmode", "1");
                KeyExistsWithCorrectValue(queries, "limitmaxerror", value);
            }
            else
                throw new NotImplementedException($"Test code for property '{property}' is not implemented.");

            return new BasicResponse(string.Empty);
        }

        private void AssertCollectionLength(string address, NameValueCollection collection, int count)
        {
            Assert.IsTrue(collection.Count == count, $"The URL '{address}' had {collection.Count} queries, however it is only supposed to have {count}.");
        }

        private void KeyExistsWithCorrectValue(NameValueCollection collection, string prefix, object v)
        {
            var key = $"{prefix}_{channelId}";

            Assert.IsTrue(collection[key] != null, $"Query '{key}' did not exist in the URL.");
            Assert.IsTrue(collection[key] == v?.ToString(), $"Query '{key}' had value '{collection[key]}' instead of '{v}'");
        }
    }
}
