using System;
using System.Threading.Tasks;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

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

        protected void Execute(Action<PrtgClient> action, string url)
        {
            var client = Initialize_Client(new AddressValidatorResponse(url));

            action(client);
        }

        protected async Task ExecuteAsync(Func<PrtgClient, Task> action, string url)
        {
            var client = Initialize_Client(new AddressValidatorResponse(url));

            await action(client);
        }

        protected T Execute<T>(Func<PrtgClient, T> action)
        {
            var client = Initialize_Client(new MultiTypeResponse());

            return action(client);
        }
    }
}
