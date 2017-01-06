using System.Threading.Tasks;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support
{
    public interface IWebResponse
    {
        string GetResponseText(string address);
        Task<string> GetResponseTextAsync(string address);
    }
}
