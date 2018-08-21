using System.Threading.Tasks;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support
{
    public interface IWebStreamResponse : IWebResponse
    {
        Task<string> GetResponseTextStream(string address);
    }
}
