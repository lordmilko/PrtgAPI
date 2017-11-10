using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Request;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support
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

        public Task<HttpResponseMessage> GetSync(string address)
        {
            if (!synchronous)
                return realWebClient.GetSync(address);
            else
            {
                var exception = new HttpRequestException("Outer Exception", new WebException("Inner Exception"));

                throw exception;
            }
        }

        public Task<HttpResponseMessage> GetAsync(string address)
        {
            if (successfulUrls.Count < 1)
            {
                successfulUrls.Add(address);
                return realWebClient.GetAsync(address);
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
