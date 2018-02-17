using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class AdminToolTests : BaseTest
    {
        [TestMethod]
        public void AdminTool_BackupConfig_CanExecute() =>
            Execute(c => c.BackupConfigDatabase(), "api/savenow.htm");

        [TestMethod]
        public async Task AdminTool_BackupConfig_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.BackupConfigDatabaseAsync(), "api/savenow.htm");

        [TestMethod]
        public void AdminTool_ClearCache_CanExecute()
        {
            Execute(c => c.ClearSystemCache(SystemCacheType.General), "api/clearcache.htm");
            Execute(c => c.ClearSystemCache(SystemCacheType.GraphData), "api/recalccache.htm");
        }

        [TestMethod]
        public async Task AdminTool_ClearCache_CanExecuteAsync()
        {
            await ExecuteAsync(async c => await c.ClearSystemCacheAsync(SystemCacheType.General), "api/clearcache.htm");
            await ExecuteAsync(async c => await c.ClearSystemCacheAsync(SystemCacheType.GraphData), "api/recalccache.htm");
        }

        [TestMethod]
        public void AdminTool_LoadConfigFiles_CanExecute()
        {
            Execute(c => c.LoadConfigFiles(ConfigFileType.General), "api/reloadfilelists.htm");
            Execute(c => c.LoadConfigFiles(ConfigFileType.Lookups), "api/loadlookups.htm");
        }

        [TestMethod]
        public async Task AdminTool_LoadConfigFiles_CanExecuteAsync()
        {
            await ExecuteAsync(async c => await c.LoadConfigFilesAsync(ConfigFileType.General), "api/reloadfilelists.htm");
            await ExecuteAsync(async c => await c.LoadConfigFilesAsync(ConfigFileType.Lookups), "api/loadlookups.htm");
        }

        [TestMethod]
        public void AdminTool_RestartCore_CanExecute()
        {
            Execute(c => c.RestartCore(), "api/restartserver.htm");
        }

        [TestMethod]
        public async Task AdminTool_RestartCore_CanExecuteAsync() =>
            await ExecuteAsync(c => c.RestartCoreAsync(), "api/restartserver.htm");

        [TestMethod]
        public void AdminTool_RestartCore_CanWait()
        {
            var standardClient = Initialize_Client(new BasicResponse(string.Empty));
            RestartCoreStage lastCoreStage = RestartCoreStage.Shutdown;

            var webClient = new MockWebClient(new RestartPrtgCoreResponse());
            var customClient = new PrtgClient(standardClient.Server, standardClient.UserName, standardClient.PassHash, AuthMode.PassHash, webClient);

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
            var standardClient = Initialize_Client(new BasicResponse(string.Empty));
            var webClient = new MockWebClient(new RestartPrtgCoreResponse());
            var customClient = new PrtgClient(standardClient.Server, standardClient.UserName, standardClient.PassHash, AuthMode.PassHash, webClient);

            await customClient.RestartCoreAsync(true);
        }

        
        [TestMethod]
        public void AdminTool_RestartProbe_CanExecute() =>
            Execute(c => c.RestartProbe(1001), "api/restartprobes.htm?id=1001");

        [TestMethod]
        public async Task AdminTool_RestartProbe_CanExecuteAsync() =>
            await ExecuteAsync(c => c.RestartProbeAsync(1001), "api/restartprobes.htm?id=1001");

        [TestMethod]
        public void AdminTool_RestartProbe_CanWait()
        {
            var standardClient = Initialize_Client(new BasicResponse(string.Empty));
            var webClient = new MockWebClient(new RestartProbeResponse());
            var customClient = new PrtgClient(standardClient.Server, standardClient.UserName, standardClient.PassHash, AuthMode.PassHash, webClient);

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
            var standardClient = Initialize_Client(new BasicResponse(string.Empty));
            var webClient = new MockWebClient(new RestartProbeResponse());
            var customClient = new PrtgClient(standardClient.Server, standardClient.UserName, standardClient.PassHash, AuthMode.PassHash, webClient);

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
