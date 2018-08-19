using System.Threading.Tasks;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class BasicResponse : IWebStreamResponse
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
            return Task.FromResult(responseText);
        }
    }
}
