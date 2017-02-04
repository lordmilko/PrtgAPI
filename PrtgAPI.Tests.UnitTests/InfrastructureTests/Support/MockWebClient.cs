using System;
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
            if (response.StatusCode == 0)
                response.StatusCode = HttpStatusCode.OK;

            var message = new HttpResponseMessage(response.StatusCode)
            {
                Content = new StringContent(response.GetResponseText(address))
            };

            try
            {
                message.EnsureSuccessStatusCode();
                return Task.FromResult(message);
            }
            catch (Exception ex)
            {
                var task = Task.FromException<HttpResponseMessage>(ex);
                return task;
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string address)
        {
            if (response.StatusCode == 0)
                response.StatusCode = HttpStatusCode.OK;

            return new HttpResponseMessage(response.StatusCode)
            {
                Content = new StringContent(await response.GetResponseTextStream(address).ConfigureAwait(false))
            };
        }
    }
}
