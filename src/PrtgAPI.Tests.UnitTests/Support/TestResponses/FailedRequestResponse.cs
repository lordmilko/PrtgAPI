using System.Net;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class FailedRequestResponse : IWebStatusResponse
    {
        private string responseText;
        private string addr;

        public FailedRequestResponse(HttpStatusCode statusCode, string responseText, string address)
        {
            StatusCode = statusCode; this.responseText = responseText;
            addr = address;
        }

        public string GetResponseText(ref string address)
        {
            if (addr != null)
                address = addr;

            return responseText;
        }

        public HttpStatusCode StatusCode { get; set; }
    }
}
