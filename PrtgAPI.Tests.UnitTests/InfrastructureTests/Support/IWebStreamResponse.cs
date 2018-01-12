using System.Threading.Tasks;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support
{
    interface IWebStreamResponse : IWebResponse
    {
        Task<string> GetResponseTextStream(string address);
    }
}
