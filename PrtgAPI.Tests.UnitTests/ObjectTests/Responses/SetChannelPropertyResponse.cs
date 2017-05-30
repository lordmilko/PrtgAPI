using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Responses
{
    class SetChannelPropertyResponse : IWebResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        private ChannelProperty property;
        private int channelId;
        private object value;

        public SetChannelPropertyResponse(ChannelProperty property, int channelId, object value)
        {
            this.property = property;
            this.channelId = channelId;
            this.value = value;
        }

        public string GetResponseText(string address)
        {
            var queries = UrlHelpers.CrackUrl(address);
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
                AssertCollectionLength(address, queries, 2);
                KeyExistsWithCorrectValue(queries, "limitmode", "1");
                KeyExistsWithCorrectValue(queries, "limitmaxerror", value);
            }
            else
                throw new NotImplementedException($"Test code for property '{property}' is not implemented.");

            return "OK";
        }

        public Task<string> GetResponseTextStream(string address)
        {
            throw new NotImplementedException();
        }

        private void AssertCollectionLength(string address, NameValueCollection collection, int count)
        {
            Assert.IsTrue(collection.Count == count, $"The URL '{address}' had {collection.Count} queries, however it is only supposed to have {count}.");
        }

        private void KeyExistsWithCorrectValue(NameValueCollection collection, string prefix, object v)
        {
            var key = $"{prefix}_{channelId}";

            Assert.IsTrue(collection[key] != null, $"Query '{key}' did not exist in the URL.");
            Assert.IsTrue(collection[key] == v.ToString(), $"Query '{key}' had value '{collection[key]}' instead of '{v}'");
        }
    }
}
