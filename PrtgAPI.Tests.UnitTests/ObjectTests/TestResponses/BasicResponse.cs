using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class BasicResponse : IWebResponse
    {
        private string responseText;

        public BasicResponse(string responseText)
        {
            this.responseText = responseText;
        }

        public string GetResponseText(ref string address)
        {
            return responseText;
        }

        public Task<string> GetResponseTextStream(string address)
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode StatusCode { get; set; }
    }
}
