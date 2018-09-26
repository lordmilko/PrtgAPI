using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PrtgAPI.Request
{
    internal interface IWebClient
    {
        Task<HttpResponseMessage> GetSync(string address, CancellationToken token);

        Task<HttpResponseMessage> GetAsync(string address, CancellationToken token);
    }
}