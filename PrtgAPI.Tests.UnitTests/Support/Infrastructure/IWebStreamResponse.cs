using System.Threading.Tasks;

namespace PrtgAPI.Tests.UnitTests
{
    public interface IWebStreamResponse : IWebResponse
    {
        Task<string> GetResponseTextStream(string address);
    }
}
