using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PrtgAPI
{
    internal class PrtgWebClient : IWebClient
    {
        private WebClient syncClient = new WebClient();
        private HttpClient asyncClient = new HttpClient();

        public string DownloadString(string address)
        {
            using (var client = new System.Net.WebClient())
            {
                return client.DownloadString(address);
            }
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