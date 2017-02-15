using System;
using System.Net;
using System.Threading.Tasks;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support
{
    class PassHashResponse : IWebResponse
    {
        private string response;

        public PassHashResponse(string response = "12345678")
        {
            this.response = response;
        }

        public HttpStatusCode StatusCode { get; set; }

        public string GetResponseText(string address)
        {
            return response;
        }

        public Task<string> GetResponseTextStream(string address)
        {
            throw new NotSupportedException();
        }
    }
}
