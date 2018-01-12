using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PrtgAPI.Request
{
    [ExcludeFromCodeCoverage]
    internal class PrtgWebClient : IWebClient
    {
        private WebClient syncClient = new WebClient();
        private HttpClient asyncClient = new HttpClient();

        private HttpClientHandler handler = new HttpClientHandler();
        private CookieContainer cookies = new CookieContainer();

        public PrtgWebClient()
        {
            handler.CookieContainer = cookies;
            asyncClient = new HttpClient(handler);
        }

        public Task<HttpResponseMessage> GetSync(string address)
        {
            return GetAsync(address);
        }

        public Task<HttpResponseMessage> GetAsync(string address)
        {
            return asyncClient.GetAsync(address);
        }
    }
}