using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Request;

namespace PrtgAPI.Tests.UnitTests
{
    class MockRetryWebClient : IWebClient
    {
        private List<string> successfulUrls = new List<string>();
        private List<string> ignoreUrls = new List<string>();

        private MockWebClient realWebClient;

        private bool synchronous;

        public MockRetryWebClient(IWebResponse response, bool synchronous)
        {
            realWebClient = new MockWebClient(response);
            this.synchronous = synchronous;
        }

        public Task<HttpResponseMessage> GetSync(string address, CancellationToken token)
        {
            if (!synchronous)
                return realWebClient.GetSync(address, token);
            else
            {
                var exception = new HttpRequestException("Outer Exception", new WebException("Inner Exception"));

                throw exception;
            }
        }

        public Task<HttpResponseMessage> GetAsync(string address, CancellationToken token)
        {
            if (successfulUrls.Count < 1)
            {
                successfulUrls.Add(address);
                return realWebClient.GetAsync(address, token);
            }
            else
            {
                if (ignoreUrls.Count < 2)
                {
                    if (!ignoreUrls.Contains(address))
                        ignoreUrls.Add(address);
                }
                else
                {
                    Assert.IsTrue(ignoreUrls.Contains(address), $"IgnoreUrls did not contain address '{address}'");
                }

                var exception = new HttpRequestException("Outer Exception", new WebException("Inner Exception"));

                throw exception;
            }
        }
    }
}
