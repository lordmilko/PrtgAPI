using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests
{
    [TestClass]
    public class PrtgClientConnectionTests
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
                Assert.Fail("Invalid credentials were specified however an exception was not thrown");
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message != "Could not authenticate to PRTG; the specified username and password were invalid.")
                {
                    Assert.Fail(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Logic_Client_NullServer()
        {
            string server = null;

            var client = new PrtgClient(server, Settings.Username, Settings.Password);
        }

        [TestMethod]
        public void Logic_Client_InvalidServer()
        {
            string server = "a";

            try
            {
                var client = new PrtgClient(server, Settings.Username, Settings.Password);
            }
            catch (WebException ex)
            {
                if (ex.Message != $"The remote name could not be resolved: '{server}'")
                    Assert.Fail();
            }
            catch (Exception)
            {
                Assert.Fail();
            }
            
        }

        [TestMethod]
        [ExpectedException(typeof(PrtgRequestException))]
        public void Logic_Client_InvalidRequest()
        {
            var client = new PrtgClient(Settings.ServerWithProto, Settings.Username, Settings.Password);
            client.Delete(0);
        }

        [TestMethod]
        [ExpectedException(typeof(PrtgRequestException))]
        public async Task Logic_Client_InvalidRequestAsync()
        {
            var client = new PrtgClient(Settings.ServerWithProto, Settings.Username, Settings.Password);
            await client.DeleteAsync(0);
        }

        [TestMethod]
        public void Logic_Client_ConnectWithHttp()
        {
            var server = $"http://{Settings.Server}";

            var client = new PrtgClient(server, Settings.Username, Settings.Password);
        }

        [TestMethod]
        public void Logic_Client_ConnectWithHttps()
        {
            var server = $"https://{Settings.Server}";

            try
            {
                var client = new PrtgClient(server, Settings.Username, Settings.Password);
            }
            catch (WebException ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.Message.StartsWith("No connection could be made because the target machine actively refused it"))
                    {
                        if(Settings.Protocol != HttpProtocol.HTTP)
                            Assert.Fail($"{ex.Message}. This may indicate your PRTG Server does not accept HTTPS or that your certificate is invalid. If your server does not accept HTTPS please change your Protocol in Settings.cs");
                    }
                    else
                        throw;
                }
                else
                    throw;
            }
        }

        [TestMethod]
        public void Logic_Client_RetryRequest()
        {
            Logic_Client_RetryRequestInternal(client => client.GetSensors());
        }

        [TestMethod]
        public void Logic_Client_RetryRequest_Async()
        {
            Logic_Client_RetryRequestInternal(client =>
            {
                var sensors = client.GetSensorsAsync().Result;
            });
        }

        private void Logic_Client_RetryRequestInternal(Action<PrtgClient> action)
        {
            Impersonator.ExecuteAction(() =>
            {
                var retriesMade = 0;
                var retriesToMake = 3;

                var coreService = new ServiceController("PRTGCoreService", Settings.Server);

                var client = new PrtgClient(Settings.ServerWithProto, Settings.Username, Settings.Password);
                client.RetryRequest += (sender, args) =>
                {
                    retriesMade++;
                };
                client.RetryCount = retriesToMake;

                coreService.Stop();
                coreService.WaitForStatus(ServiceControllerStatus.Stopped);

                try
                {
                    action(client);
                }
                catch
                {
                }

                coreService.Start();
                coreService.WaitForStatus(ServiceControllerStatus.Running);

                Thread.Sleep(20000);

                client.CheckNow(Settings.Device);

                Thread.Sleep(20000);

                Assert.AreEqual(retriesToMake, retriesMade);
            });
        }
    }
}
