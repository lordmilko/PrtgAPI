using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Helpers;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class PrtgClientConnectionTests : BasePrtgClientTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Logic_Client_NullCredentials()
        {
            var server = $"http://{Settings.Server}";
            string username = null;
            string password = null;

            var client = new PrtgClient(server, username, password);
        }

        [TestMethod]
        public void Logic_Client_InvalidCredentials()
        {
            var server = $"http://{Settings.Server}";
            string username = "a";
            string password = "a";

            try
            {
                var client = new PrtgClient(server, username, password);
                Assert2.Fail("Invalid credentials were specified however an exception was not thrown");
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message != "Could not authenticate to PRTG; the specified username and password were invalid.")
                {
                    Assert2.Fail(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Assert2.Fail(ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Logic_Client_NullServer()
        {
            string server = null;

            var client = new PrtgClient(server, Settings.UserName, Settings.Password);
        }

        [TestMethod]
        public void Logic_Client_InvalidServer()
        {
            string server = "a";

            try
            {
                var client = new PrtgClient(server, Settings.UserName, Settings.Password);
            }
            catch (WebException ex)
            {
                if (ex.Message != $"The remote name could not be resolved: '{server}'")
                    Assert2.Fail($"Request did not fail with expected error message: {ex.Message}");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PrtgRequestException))]
        public void Logic_Client_InvalidRequest()
        {
            var client = new PrtgClient(Settings.ServerWithProto, Settings.UserName, Settings.Password);
            client.DeleteObject(0);
        }

        [TestMethod]
        [ExpectedException(typeof(PrtgRequestException))]
        public async Task Logic_Client_InvalidRequestAsync()
        {
            var client = new PrtgClient(Settings.ServerWithProto, Settings.UserName, Settings.Password);
            await client.DeleteObjectAsync(0);
        }

        [TestMethod]
        public void Logic_Client_ConnectWithHttp()
        {
            var server = $"http://{Settings.Server}";

            var client = new PrtgClient(server, Settings.UserName, Settings.Password);
        }

        [TestMethod]
        public void Logic_Client_ConnectWithHttps()
        {
            var server = $"https://{Settings.Server}";

            try
            {
                var localClient = new PrtgClient(server, Settings.UserName, Settings.Password);
            }
            catch (WebException ex)
            {
                if (ex.Message != "Server rejected HTTPS connection on port 443. Please confirm expected server protocol and port, PRTG Core Service is running and that any SSL certificate is trusted")
                {
                    throw;
                }
            }
        }

        [TestMethod]
        public async Task Logic_Client_ConnectWithHttps_Async()
        {
            var server = $"https://{Settings.Server}";

            var localClient = new PrtgClient(server, Settings.UserName, client.PassHash, AuthMode.PassHash);

            //Get the method
            var engine = localClient.GetInternalField("requestEngine");
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var methods = engine.GetType().GetMethods(flags).Where(m => m.Name == "ExecuteRequestAsync").ToList();
            var method = methods.First(m => m.GetParameters().Any(p => p.ParameterType.Name == "JsonFunction"));

            //Get the enum
            var jsonFunctionEnum = typeof(PrtgClient).Assembly.GetType("PrtgAPI.JsonFunction");
            var getPassHash = Enum.Parse(jsonFunctionEnum, "GetPassHash");

            //Construct the parameters
            var parameters = new Parameters.Parameters
            {
                [Parameter.Password] = Settings.Password
            };

            try
            {
                await (Task<string>)method.Invoke(engine, new[] {getPassHash, parameters, null});
            }
            catch (WebException ex)
            {
                if (ex.Message != "Server rejected HTTPS connection on port 443. Please confirm expected server protocol and port, PRTG Core Service is running and that any SSL certificate is trusted")
                {
                    throw;
                }
            }
        }

        [TestMethod]
        public void Logic_Client_RetryRequest()
        {
            Logic_Client_RetryRequestInternal(localClient => localClient.GetSensors(), false);
        }

        [TestMethod]
        public void Logic_Client_RetryRequest_Async()
        {
            Logic_Client_RetryRequestInternal(localClient =>
            {
                var sensors = localClient.GetSensorsAsync().Result;
            }, true);
        }

        [TestMethod]
        [ExpectedException(typeof(System.TimeoutException))]
        public void Logic_Client_Timeout()
        {
            var localClient = GetTimeoutClient();

            localClient.GetSensors();
        }

        [TestMethod]
        [ExpectedException(typeof(System.TimeoutException))]
        public async Task Logic_Client_Timeout_Async()
        {
            var localClient = GetTimeoutClient();

            await localClient.GetSensorsAsync();
        }

        private PrtgClient GetTimeoutClient()
        {
            var localClient = new PrtgClient(Settings.ServerWithProto, Settings.UserName, Settings.Password);
            var engine = localClient.GetInternalField("requestEngine");
            var webInterface = engine.GetInternalField("webClient");

            var httpClient = new HttpClient
            {
                Timeout = new TimeSpan(0, 0, 0, 0, 1)
            };

            webInterface.GetType().GetField("asyncClient", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(webInterface, httpClient);            

            return localClient;
        }

        private void Logic_Client_RetryRequestInternal(Action<PrtgClient> action, bool isAsync)
        {
            var initialThread = Thread.CurrentThread.ManagedThreadId;

            Impersonator.ExecuteAction(() =>
            {
                var retriesMade = 0;
                var retriesToMake = 3;

                var coreService = new ServiceController("PRTGCoreService", Settings.Server);

                var localClient = new PrtgClient(Settings.ServerWithProto, Settings.UserName, Settings.Password);
                localClient.RetryRequest += (sender, args) =>
                {
                    Logger.LogTestDetail($"Handling retry {retriesMade + 1}");

                    if (!isAsync)
                        Assert2.AreEqual(initialThread, Thread.CurrentThread.ManagedThreadId, "Event was not handled on initial thread");
                    retriesMade++;
                };
                localClient.RetryCount = retriesToMake;

                Logger.LogTestDetail("Stopping PRTG Service");

                coreService.Stop();
                coreService.WaitForStatus(ServiceControllerStatus.Stopped);

                try
                {
                    action(localClient);
                }
                catch (AggregateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.GetType() == typeof (AssertFailedException))
                        throw ex.InnerException;
                }
                catch (WebException)
                {
                }
                finally
                {
                    Logger.LogTestDetail("Starting PRTG Service");
                    coreService.Start();
                    coreService.WaitForStatus(ServiceControllerStatus.Running);

                    Logger.LogTestDetail("Sleeping for 20 seconds");
                    Thread.Sleep(20000);

                    Logger.LogTestDetail("Refreshing and sleeping for 20 seconds");
                    localClient.RefreshObject(Settings.Device);
                    Thread.Sleep(20000);
                }

                Assert2.AreEqual(retriesToMake, retriesMade, "An incorrect number of retries were made.");
            });
        }
    }
}
