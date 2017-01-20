using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PrtgAPI
{
    internal interface IWebClient
    {
        Task<HttpResponseMessage> GetSync(string address);

        Task<HttpResponseMessage> GetAsync(string address);
    }
}