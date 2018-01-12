using System;
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
    }
}
