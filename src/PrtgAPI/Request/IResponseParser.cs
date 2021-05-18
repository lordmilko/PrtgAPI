using System.Net.Http;
using System.Threading.Tasks;

namespace PrtgAPI.Request
{
    internal interface IResponseParser
    {
        PrtgResponse ParseResponse(HttpResponseMessage requestMessage);

        Task<PrtgResponse> ParseResponseAsync(HttpResponseMessage requestMessage);
    }
}