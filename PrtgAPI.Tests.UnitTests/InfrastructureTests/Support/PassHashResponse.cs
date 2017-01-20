using System;
using System.Threading.Tasks;

namespace PrtgAPI.Tests.UnitTests.InfrastructureTests.Support
{
    class PassHashResponse : IWebResponse
    {
        public string GetResponseText(string address)
        {
            return "12345678";
        }

        public Task<string> GetResponseTextStream(string address)
        {
            throw new NotSupportedException();
        }
    }
}
