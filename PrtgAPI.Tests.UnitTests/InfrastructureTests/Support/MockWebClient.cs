using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

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

        public Task<HttpResponseMessage> GetSync(string address)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response.GetResponseText(address))
            });
            //return await response.GetResponseTextAsync(address);
        }

        public async Task<HttpResponseMessage> GetAsync(string address)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(await response.GetResponseTextStream(address).ConfigureAwait(false))
            };
            //return await response.GetResponseTextAsync(address);
        }
    }
}
