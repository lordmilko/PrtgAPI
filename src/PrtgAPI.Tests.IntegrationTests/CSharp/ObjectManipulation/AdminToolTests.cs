using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectManipulation
{
    [TestClass]
    public class AdminToolTests : BasePrtgClientTest
    {
        private static string PrtgBackups => $"\\\\{Settings.Server}\\c$\\ProgramData\\Paessler\\PRTG Network Monitor\\Configuration Auto-Backups";

        [TestMethod]
        [IntegrationTest]
        public void Action_BackupConfig_SuccessfullyBacksUpConfig()
        {
            var originalFiles = GetBackupFiles();

            client.BackupConfigDatabase();

            ValidateBackupCreated(originalFiles);
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Action_BackupConfig_SuccessfullyBacksUpConfigAsync()
        {
            var originalFiles = GetBackupFiles();

            await client.BackupConfigDatabaseAsync();

            ValidateBackupCreated(originalFiles);
        }

        public static List<FileInfo> GetBackupFiles() => Impersonator.ExecuteAction(new DirectoryInfo(PrtgBackups).GetFiles).ToList();

        public static void RemoveBackupFile(string fileName) => Impersonator.ExecuteAction(() => File.Delete(fileName));

        private void ValidateBackupCreated(List<FileInfo> originalFiles)
        {
            Logger.LogTestDetail("Pausing for 10 seconds while backup is created");
            Thread.Sleep(10000);

            var newFiles = GetBackupFiles();

            AssertEx.AreEqual(originalFiles.Count + 1, newFiles.Count, "New backup file was not created");

            var diff = newFiles.Select(f => f.FullName).Except(originalFiles.Select(fn => fn.FullName)).ToList();

            AssertEx.AreEqual(1, diff.Count, "Backup file was not successfully created");

            var firstFile = diff.First();

            RemoveBackupFile(firstFile);
        }
    }
}
