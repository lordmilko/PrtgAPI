using System.Net.Http;
using System.Threading.Tasks;

namespace PrtgAPI.Request
{
    internal interface IWebClient
    {
        Task<HttpResponseMessage> GetSync(string address);

        Task<HttpResponseMessage> GetAsync(string address);
    }
}