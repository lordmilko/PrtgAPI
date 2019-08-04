using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PrtgAPI.Request
{
    internal interface IWebClient
    {
        Task<HttpResponseMessage> SendSync(PrtgRequestMessage request, CancellationToken token);

        Task<HttpResponseMessage> SendAsync(PrtgRequestMessage request, CancellationToken token);
    }
}