using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Helpers;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Responses
{
    class SetObjectPropertyResponse : IWebResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        private ObjectProperty property;
        private string expectedValue;

        public SetObjectPropertyResponse(ObjectProperty property, string expectedValue)
        {
            this.property = property;
            this.expectedValue = expectedValue;
        }

        public string GetResponseText(ref string address)
        {
            var queries = UrlHelpers.CrackUrl(address);

            var val = queries[property.GetDescription().ToLower()];

            Assert.IsTrue(val == expectedValue, $"The value of property '{property.ToString().ToLower()}' did not match the expected value. Expected '{expectedValue}', received: '{val}'");

            return "OK";
        }

        public Task<string> GetResponseTextStream(string address)
        {
            throw new NotImplementedException();
        }
    }
}
