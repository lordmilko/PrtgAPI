using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrtgAPI.Reflection;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests
{
    public class BaseTest
    {
        //Public for use in PowerShell
        public static PrtgClient Initialize_Client(IWebResponse response, bool switchContext = false)
        {
            var webClient = new MockWebClient(response);
            webClient.SwitchContext = switchContext;

            var client = new PrtgClient("prtg.example.com", "username", "12345678", AuthMode.PassHash, webClient);
            
            return client;
        }

        public static PrtgClient Initialize_ReadOnlyClient(IWebResponse response)
        {
            return Initialize_Client(new ReadOnlyResponse(response));
        }

        internal static PrtgClient Initialize_Client(IWebResponse response, RequestVersion version)
        {
            var client = Initialize_Client(response);

            SetVersion(client, version);

            return client;
        }

        private static void SetVersion(PrtgClient client, RequestVersion version)
        {
            var f = client.GetInternalFieldInfo("version");
            f.SetValue(client, new Version(version.ToString().TrimStart('v').Replace('_', '.')));
        }

        /// <summary>
        /// Validates the request URL of a specified action.
        /// </summary>
        /// <param name="action">Action to perform.</param>
        /// <param name="url">URL that should be created.</param>
        /// <param name="countOverride">Override for the number of objects that should be created.</param>
        /// <param name="version">The client version to use. If no version is specified, the default will be used.</param>
        internal void Execute(Action<PrtgClient> action, string url, Dictionary<Content, int> countOverride = null, Dictionary<Content, BaseItem[]> itemOverride = null, RequestVersion? version = null)
        {
            var response = GetValidator(url, countOverride, itemOverride);

            var client = version != null ? Initialize_Client(response, version.Value) : Initialize_Client(response);

            action(client);

            response.AssertFinished();
        }

        /// <summary>
        /// Validates the request URL of a specified action where multiple URLs are executed.
        /// </summary>
        /// <param name="action">Action to perform.</param>
        /// <param name="url">URLs that should be created.</param>
        /// <param name="countOverride">Override for the number of objects that should be created.</param>
        /// <param name="version">The client version to use. If no version is specified, the default will be used.</param>
        internal void Execute(Action<PrtgClient> action, string[] url, Dictionary<Content, int> countOverride = null, Dictionary<Content, BaseItem[]> itemOverride = null, RequestVersion? version = null)
        {
            var response = GetValidator(url, countOverride, itemOverride);

            var client = version != null ? Initialize_Client(response, version.Value) : Initialize_Client(response);

            action(client);

            response.AssertFinished();
        }

        internal async Task ExecuteAsync(Func<PrtgClient, Task> action, string url, Dictionary<Content, int> countOverride = null, Dictionary<Content, BaseItem[]> itemOverride = null, RequestVersion? version = null)
        {
            var response = GetValidator(url, countOverride, itemOverride);

            var client = version != null ? Initialize_Client(response, version.Value) : Initialize_Client(response);

            await action(client);

            response.AssertFinished();
        }

        internal async Task ExecuteAsync(Func<PrtgClient, Task> action, string[] url, Dictionary<Content, int> countOverride = null, Dictionary<Content, BaseItem[]> itemOverride = null, RequestVersion? version = null, Action<AddressValidatorResponse> additionalItems = null)
        {
            var response = GetValidator(url, countOverride, itemOverride);

            additionalItems?.Invoke(response);

            var client = version != null ? Initialize_Client(response, version.Value) : Initialize_Client(response);

            await action(client);

            response.AssertFinished();
        }

        protected T Execute<T>(Func<PrtgClient, T> action)
        {
            var client = Initialize_Client(new MultiTypeResponse());

            return action(client);
        }

        private AddressValidatorResponse GetValidator(object urls, Dictionary<Content, int> countOverride, Dictionary<Content, BaseItem[]> itemOverride)
        {
#pragma warning disable 618
            if (urls is string[] || urls is object[])
                return new AddressValidatorResponse(((IEnumerable)urls).Cast<object>().ToArray())
                {
                    CountOverride = countOverride,
                    ItemOverride = itemOverride
                };
            else
                return new AddressValidatorResponse(urls?.ToString())
                {
                    CountOverride = countOverride,
                    ItemOverride = itemOverride
                };
#pragma warning restore 618
        }
    }
}
