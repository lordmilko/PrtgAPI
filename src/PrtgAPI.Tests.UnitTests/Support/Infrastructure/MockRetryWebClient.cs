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

        public Task<HttpResponseMessage> SendSync(PrtgRequestMessage request, CancellationToken token)
        {
            if (!synchronous)
                return realWebClient.SendSync(request, token);
            else
            {
                var exception = new HttpRequestException("Outer Exception", new WebException("Inner Exception"));

                throw exception;
            }
        }

        public Task<HttpResponseMessage> SendAsync(PrtgRequestMessage request, CancellationToken token)
        {
            if (successfulUrls.Count < 1)
            {
                successfulUrls.Add(request.Url);
                return realWebClient.SendAsync(request, token);
            }
            else
            {
                if (ignoreUrls.Count < 2)
                {
                    if (!ignoreUrls.Contains(request.Url))
                        ignoreUrls.Add(request.Url);
                }
                else
                {
                    Assert.IsTrue(ignoreUrls.Contains(request.Url), $"IgnoreUrls did not contain address '{request.ToString()}'");
                }

                var exception = new HttpRequestException("Outer Exception", new WebException("Inner Exception"));

                throw exception;
            }
        }
    }
}
