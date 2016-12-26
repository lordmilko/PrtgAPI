using System.Threading.Tasks;

namespace PrtgAPI
{
    internal class WebClient : IWebClient
    {
        public string DownloadString(string address)
        {
            using (var client = new System.Net.WebClient())
            {
                return client.DownloadString(address);
            }
        }

        public Task<string> DownloadStringTaskAsync(string address)
        {
            using (var client = new System.Net.WebClient())
            {
                return client.DownloadStringTaskAsync(address);
            }
            
        }
    }
}