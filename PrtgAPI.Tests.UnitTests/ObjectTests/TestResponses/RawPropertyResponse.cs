using System;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    class RawPropertyResponse : IWebResponse
    {
        private string value;

        public RawPropertyResponse(string value)
        {
            this.value = value;
        }

        public string GetResponseText(ref string address)
        {
            var xml = new XElement("prtg",
                new XElement("version", "1.2.3.4"),
                new XElement("result", value)
            );

            return xml.ToString();
        }

        public Task<string> GetResponseTextStream(string address)
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode StatusCode { get; set; }
    }
}
