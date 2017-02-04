using System.Net;
using System.Threading.Tasks;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support
{
    public interface IWebResponse
    {
        string GetResponseText(string address);
        Task<string> GetResponseTextStream(string address);

        HttpStatusCode StatusCode { get; set; }
    }
}
