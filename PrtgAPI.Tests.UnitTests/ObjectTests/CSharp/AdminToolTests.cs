using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class AdminToolTests : BaseTest
    {
        PrtgClient client = Initialize_Client(new BasicResponse(string.Empty));

        [TestMethod]
        public void AdminTool_BackupConfig_CanExecute()
        {
            client.BackupConfigDatabase();
        }

        [TestMethod]
        public async Task AdminTool_BackupConfig_CanExecuteAsync()
        {
            await client.BackupConfigDatabaseAsync();
        }

        [TestMethod]
        public void AdminTool_ClearCache_CanExecute()
        {
            client.ClearSystemCache(SystemCacheType.General);
            client.ClearSystemCache(SystemCacheType.GraphData);
        }

        [TestMethod]
        public async Task AdminTool_ClearCache_CanExecuteAsync()
        {
            await client.ClearSystemCacheAsync(SystemCacheType.General);
            await client.ClearSystemCacheAsync(SystemCacheType.GraphData);
        }

        [TestMethod]
        public void AdminTool_LoadConfigFiles_CanExecute()
        {
            client.LoadConfigFiles(ConfigFileType.General);
            client.LoadConfigFiles(ConfigFileType.Lookups);
        }

        [TestMethod]
        public async Task AdminTool_LoadConfigFiles_CanExecuteAsync()
        {
            await client.LoadConfigFilesAsync(ConfigFileType.General);
            await client.LoadConfigFilesAsync(ConfigFileType.Lookups);
        }

        [TestMethod]
        public void AdminTool_RestartCore_CanExecute()
        {
            client.RestartCore();
        }

        [TestMethod]
        public async Task AdminTool_RestartCore_CanExecuteAsync()
        {
            await client.RestartCoreAsync();
        }

        [TestMethod]
        public void AdminTool_RestartCore_CanWait()
        {
            RestartCoreStage lastCoreStage = RestartCoreStage.Shutdown;

            var webClient = new MockWebClient(new RestartPrtgCoreResponse());
            var customClient = new PrtgClient(client.Server, client.UserName, client.PassHash, AuthMode.PassHash, webClient);

            customClient.RestartCore(true, stage =>
            {
                lastCoreStage = stage;

                if (stage == RestartCoreStage.Restart)
                    return false;

                return true;
            });

            Assert.AreEqual(RestartCoreStage.Restart, lastCoreStage, "Waiting did not stop after returning false");
        }

        [TestMethod]
        public async Task AdminTool_RestartCore_CanWaitAsync()
        {
            var webClient = new MockWebClient(new RestartPrtgCoreResponse());
            var customClient = new PrtgClient(client.Server, client.UserName, client.PassHash, AuthMode.PassHash, webClient);

            await customClient.RestartCoreAsync(true);
        }

        
        [TestMethod]
        public void AdminTool_RestartProbe_CanExecute()
        {
            client.RestartProbe(1001);
        }

        [TestMethod]
        public async Task AdminTool_RestartProbe_CanExecuteAsync()
        {
            await client.RestartProbeAsync(1001);
        }

        [TestMethod]
        public void AdminTool_RestartProbe_CanWait()
        {
            var webClient = new MockWebClient(new RestartProbeResponse());
            var customClient = new PrtgClient(client.Server, client.UserName, client.PassHash, AuthMode.PassHash, webClient);

            var count = 0;

            customClient.RestartProbe(null, true, probes =>
            {
                count += probes.Count;

                return false;
            });

            Assert.AreEqual(2, count, "Callback was not called expected number of times");
        }

        [TestMethod]
        public async Task AdminTool_RestartProbe_CanWaitAsync()
        {
            var webClient = new MockWebClient(new RestartProbeResponse());
            var customClient = new PrtgClient(client.Server, client.UserName, client.PassHash, AuthMode.PassHash, webClient);

            var count = 0;

            await customClient.RestartProbeAsync(null, true, probes =>
            {
                count += probes.Count;

                return false;
            });

            Assert.AreEqual(2, count, "Callback was not called expected number of times");
        }
    }
}
