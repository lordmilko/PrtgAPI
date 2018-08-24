using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests
{
    public class BaseTest
    {
        public static PrtgClient Initialize_Client(IWebResponse response)
        {
            var webClient = new MockWebClient(response);

            var client = new PrtgClient("prtg.example.com", "username", "12345678", AuthMode.PassHash, webClient);
            
            return client;
        }

        protected void Execute(Action<PrtgClient> action, string url, Dictionary<Content, int> countOverride = null)
        {
            var response = GetValidator(url, countOverride);

            var client = Initialize_Client(response);

            action(client);

            response.AssertFinished();
        }

        protected void Execute(Action<PrtgClient> action, string[] url, Dictionary<Content, int> countOverride = null)
        {
            var response = GetValidator(url, countOverride);

            var client = Initialize_Client(response);

            action(client);

            response.AssertFinished();
        }

        protected async Task ExecuteAsync(Func<PrtgClient, Task> action, string url, Dictionary<Content, int> countOverride = null)
        {
            var response = GetValidator(url, countOverride);

            var client = Initialize_Client(response);

            await action(client);

            response.AssertFinished();
        }

        protected async Task ExecuteAsync(Func<PrtgClient, Task> action, string[] url, Dictionary<Content, int> countOverride = null)
        {
            var response = GetValidator(url, countOverride);

            var client = Initialize_Client(response);

            await action(client);

            response.AssertFinished();
        }

        protected T Execute<T>(Func<PrtgClient, T> action)
        {
            var client = Initialize_Client(new MultiTypeResponse());

            return action(client);
        }

        private AddressValidatorResponse GetValidator(object urls, Dictionary<Content, int> countOverride)
        {
            if (urls is string[] || urls is object[])
                return new AddressValidatorResponse(((IEnumerable)urls).Cast<object>().ToArray()) { CountOverride = countOverride };
            else
                return new AddressValidatorResponse(urls?.ToString()) { CountOverride = countOverride };
        }
    }
}
