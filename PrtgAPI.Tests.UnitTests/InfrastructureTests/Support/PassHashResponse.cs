using System;
using System.Net;
using System.Threading.Tasks;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support
{
    class PassHashResponse : IWebResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string GetResponseText(string address)
        {
            return "12345678";
        }

        public Task<string> GetResponseTextStream(string address)
        {
            throw new NotSupportedException();
        }
    }
}
