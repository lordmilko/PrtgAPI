using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    public class BaseTest
    {
        public static PrtgClient Initialize_Client(IWebResponse response)
        {
            var webClient = new MockWebClient(response);

            var client = new PrtgClient("prtg.example.com", "username", "12345678", AuthMode.PassHash, webClient);

            return client;
        }
    }
}
