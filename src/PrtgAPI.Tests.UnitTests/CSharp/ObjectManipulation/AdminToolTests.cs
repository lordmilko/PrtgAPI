using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class AdminToolTests : BaseTest
    {
        [UnitTest]
        [TestMethod]
        public void AdminTool_BackupConfig_CanExecute() =>
            Execute(c => c.BackupConfigDatabase(), "api/savenow.htm");

        [UnitTest]
        [TestMethod]
        public async Task AdminTool_BackupConfig_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.BackupConfigDatabaseAsync(), "api/savenow.htm");

        [UnitTest]
        [TestMethod]
        public void AdminTool_ClearCache_CanExecute()
        {
            Execute(c => c.ClearSystemCache(SystemCacheType.General), "api/clearcache.htm");
            Execute(c => c.ClearSystemCache(SystemCacheType.GraphData), "api/recalccache.htm");
        }

        [UnitTest]
        [TestMethod]
        public async Task AdminTool_ClearCache_CanExecuteAsync()
        {
            await ExecuteAsync(async c => await c.ClearSystemCacheAsync(SystemCacheType.General), "api/clearcache.htm");
            await ExecuteAsync(async c => await c.ClearSystemCacheAsync(SystemCacheType.GraphData), "api/recalccache.htm");
        }

        [UnitTest]
        [TestMethod]
        public void AdminTool_LoadConfigFiles_CanExecute()
        {
            Execute(c => c.LoadConfigFiles(ConfigFileType.General), "api/reloadfilelists.htm");
            Execute(c => c.LoadConfigFiles(ConfigFileType.Lookups), "api/loadlookups.htm");
        }

        [UnitTest]
        [TestMethod]
        public async Task AdminTool_LoadConfigFiles_CanExecuteAsync()
        {
            await ExecuteAsync(async c => await c.LoadConfigFilesAsync(ConfigFileType.General), "api/reloadfilelists.htm");
            await ExecuteAsync(async c => await c.LoadConfigFilesAsync(ConfigFileType.Lookups), "api/loadlookups.htm");
        }

        [UnitTest]
        [TestMethod]
        public void AdminTool_RestartCore_CanExecute()
        {
            Execute(c => c.RestartCore(), "api/restartserver.htm");
        }

        [UnitTest]
        [TestMethod]
        public async Task AdminTool_RestartCore_CanExecuteAsync() =>
            await ExecuteAsync(c => c.RestartCoreAsync(), "api/restartserver.htm");

        [UnitTest]
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

        [UnitTest]
        [TestMethod]
        public async Task AdminTool_RestartCore_CanWaitAsync()
        {
            var standardClient = Initialize_Client(new BasicResponse(string.Empty));
            var webClient = new MockWebClient(new RestartPrtgCoreResponse());
            var customClient = new PrtgClient(standardClient.Server, standardClient.UserName, standardClient.PassHash, AuthMode.PassHash, webClient);

            await customClient.RestartCoreAsync(true);
        }

        
        [UnitTest]
        [TestMethod]
        public void AdminTool_RestartProbe_CanExecute() =>
            Execute(c => c.RestartProbe(1001), "api/restartprobes.htm?id=1001");

        [UnitTest]
        [TestMethod]
        public async Task AdminTool_RestartProbe_CanExecuteAsync() =>
            await ExecuteAsync(c => c.RestartProbeAsync(1001), "api/restartprobes.htm?id=1001");

        [UnitTest]
        [TestMethod]
        public void AdminTool_RestartProbe_CanWait()
        {
            var customClient = GetRestartProbeClient();

            var count = 0;

            customClient.RestartProbe(null, true, probes =>
            {
                count += probes.Length;

                return false;
            });

            Assert.AreEqual(2, count, "Callback was not called expected number of times");
        }

        [UnitTest]
        [TestMethod]
        public async Task AdminTool_RestartProbe_CanWaitAsync()
        {
            var customClient = GetRestartProbeClient();

            var count = 0;

            await customClient.RestartProbeAsync(null, true, probes =>
            {
                count += probes.Length;

                return false;
            });

            Assert.AreEqual(2, count, "Callback was not called expected number of times");
        }

        [UnitTest]
        [TestMethod]
        public void AdminTool_RestartProbe_NoArguments()
        {
            Execute(c => c.RestartProbe(), "restartprobes.htm?username");
        }

        [UnitTest]
        [TestMethod]
        public async Task AdminTool_RestartProbe_NoArgumentsAsync()
        {
            await ExecuteAsync(async c => await c.RestartProbeAsync(), "restartprobes.htm?username");
        }

        [UnitTest]
        [TestMethod]
        public void AdminTool_RestartProbe_EmptyArray()
        {
            Execute(c => c.RestartProbe(new int[] {}), "restartprobes.htm?username");
        }

        [UnitTest]
        [TestMethod]
        public async Task AdminTool_RestartProbe_EmptyArrayAsync()
        {
            await ExecuteAsync(async c => await c.RestartProbeAsync(new int[] {}), "restartprobes.htm?username");
        }

        private PrtgClient GetRestartProbeClient()
        {
            var standardClient = Initialize_Client(new BasicResponse(string.Empty));
            var webClient = new MockWebClient(new RestartProbeResponse());
            var customClient = new PrtgClient(standardClient.Server, standardClient.UserName, standardClient.PassHash, AuthMode.PassHash, webClient);

            return customClient;
        }

        #region Approve Probe

        [UnitTest]
        [TestMethod]
        public void AdminTool_ApproveProbe_AllTypes()
        {
            Action<int, ProbeApproval, string> execute = (id, action, str) =>
            {
                var urls = new[]
                {
                    UnitRequest.GetObjectProperty(id, "authorized"),
                    UnitRequest.Get($"api/probestate.htm?id={id}&action={str}")
                };

                Execute(c => c.ApproveProbe(id, action), urls);
            };

            execute(1001, ProbeApproval.Allow, "allow");
            execute(1001, ProbeApproval.Deny, "deny");
            execute(1001, ProbeApproval.AllowAndDiscover, "allowanddiscover");
        }

        [UnitTest]
        [TestMethod]
        public async Task AdminTool_ApproveProbe_AllTypesAsync()
        {
            Func<int, ProbeApproval, string, Task> execute = async (id, action, str) =>
            {
                var urls = new[]
                {
                    UnitRequest.GetObjectProperty(id, "authorized"),
                    UnitRequest.Get($"api/probestate.htm?id={id}&action={str}")
                };

                await ExecuteAsync(async c => await c.ApproveProbeAsync(id, action), urls);
            };

            await execute(1001, ProbeApproval.Allow, "allow");
            await execute(1001, ProbeApproval.Deny, "deny");
            await execute(1001, ProbeApproval.AllowAndDiscover, "allowanddiscover");
        }

        [UnitTest]
        [TestMethod]
        public void AdminTool_ApproveProbe_AlreadyApproved()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            AssertEx.Throws<InvalidOperationException>(
                () => client.ApproveProbe(1002, ProbeApproval.Allow),
                "probe has already been approved"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task AdminTool_ApproveProbe_AlreadyApprovedAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.ApproveProbeAsync(1002, ProbeApproval.Allow),
                "probe has already been approved"
            );
        }

        [UnitTest]
        [TestMethod]
        public void AdminTool_ApproveProbe_NotAProbe()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            AssertEx.Throws<InvalidOperationException>(
                () => client.ApproveProbe(9001, ProbeApproval.Allow),
                "does not appear to be a probe"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task AdminTool_ApproveProbe_NotAProbeAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.ApproveProbeAsync(9001, ProbeApproval.Allow),
                "does not appear to be a probe"
            );
        }

        [UnitTest]
        [TestMethod]
        public void AdminTool_ApproveProbe_NotAProbe_NonEnglish()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            AssertEx.Throws<InvalidOperationException>(
                () => client.ApproveProbe(9002, ProbeApproval.Allow),
                "does not appear to be a probe"
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task AdminTool_ApproveProbe_NotAProbe_NonEnglishAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await AssertEx.ThrowsAsync<InvalidOperationException>(
                async () => await client.ApproveProbeAsync(9001, ProbeApproval.Allow),
                "does not appear to be a probe"
            );
        }

        #endregion
    }
}
