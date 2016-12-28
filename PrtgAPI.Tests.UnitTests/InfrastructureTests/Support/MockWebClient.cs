using System.Threading.Tasks;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support
{
    class MockWebClient : IWebClient
    {
        IWebResponse response;

        public MockWebClient(IWebResponse response)
        {
            this.response = response;
        }

        public string DownloadString(string address)
        {
            return response.GetResponseText(address);
        }

        public Task<string> DownloadStringTaskAsync(string address)
        {
            return Task.FromResult(response.GetResponseText(address));
        }
    }
}
