using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class SystemInfoTests : BasePrtgClientTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Data_SystemInformation_RetrievesFromProbe()
        {
            var info = client.GetSystemInfo(Settings.Probe);

            AssertNoInfo(info);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SystemInformation_RetrievesFromGroup()
        {
            var info = client.GetSystemInfo(Settings.Group);

            AssertNoInfo(info);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SystemInformation_RetrievesFromDevice()
        {
            var info = client.GetSystemInfo(Settings.Device);

            Assert.IsTrue(info.System.Count > 0, "Did not have any system information");
            Assert.IsTrue(info.Hardware.Count > 0, "Did not have any hardware information");
            Assert.IsTrue(info.Software.Count > 0, "Did not have any software information");
            Assert.IsTrue(info.Processes.Count > 0, "Did not have any process information");
            Assert.IsTrue(info.Services.Count > 0, "Did not have any service information");
            Assert.IsTrue(info.Users.Count > 0, "Did not have any user information");
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SystemInformation_RetrievesFromSensor()
        {
            var info = client.GetSystemInfo(Settings.UpSensor);

            AssertNoInfo(info);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SystemInformation_RetrievesFromNotificationAction()
        {
            var info = client.GetSystemInfo(Settings.NotificationAction);

            AssertNoInfo(info);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SystemInformation_RetrievesFromNewDevice()
        {
            Device device = null;

            try
            {
                ClearNextObjectInfo();

                device = client.AddDevice(Settings.Probe, "systemInfoDevice", "8.8.8.8");

                var info = client.GetSystemInfo(device.Id);

                AssertNoInfo(info);
            }
            finally
            {
                if (device != null)
                    client.RemoveObject(device.Id);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SystemInformation_RetrievesFromInvalidId()
        {
            AssertEx.Throws<PrtgRequestException>(() => client.GetSystemInfo(2), "Sorry, there is no object with the specified id.");
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SystemInformation_RetrievesFromNewNonexistantDevice()
        {
            Device device = null;

            try
            {
                ClearNextObjectInfo();

                device = client.AddDevice(Settings.Probe, "systemInfoDevice", "200.200.200.200");

                var info = client.GetSystemInfo(device.Id);

                AssertNoInfo(info);
            }
            finally
            {
                if (device != null)
                    client.RemoveObject(device.Id);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_SystemInfo_ReadOnlyUser()
        {
            var info = readOnlyClient.GetSystemInfo(Settings.Device);

            var properties = info.GetType().GetProperties();

            foreach (var prop in properties)
                AssertEx.AllPropertiesRetrieveValues(prop.GetValue(info));
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Data_SystemInfo_ReadOnlyUserAsync()
        {
            var info = await readOnlyClient.GetSystemInfoAsync(Settings.Device);

            var properties = info.GetType().GetProperties();

            foreach (var prop in properties)
                AssertEx.AllPropertiesRetrieveValues(prop.GetValue(info));
        }

        private void ClearNextObjectInfo()
        {
            var lastObj = client.GetObjects().OrderByDescending(o => o.Id).First();

            var nextId = lastObj.Id;

            bool deleted = false;

            do
            {
                nextId++;

                deleted = Impersonator.ExecuteAction(() =>
                {
                    if (Directory.Exists(ServerManager.PrtgSystemInformationDatabase))
                    {
                        var files = Directory.EnumerateFiles(ServerManager.PrtgSystemInformationDatabase, "*.*", SearchOption.AllDirectories).Where(f => new FileInfo(f).Name.StartsWith($"Device {nextId}.")).ToList();

                        if (files.Count > 0)
                            Logger.LogTestDetail($"Removing System Information for previously created object ID {nextId}");

                        foreach (var file in files)
                            File.Delete(file);

                        if (files.Count > 0)
                            return true;
                    }

                    return false;
                });
            } while (deleted);
        }

        private void AssertNoInfo(SystemInfo info)
        {
            AssertEmptyList(info.System);
            AssertEmptyList(info.Hardware);
            AssertEmptyList(info.Software);
            AssertEmptyList(info.Processes);
            AssertEmptyList(info.Services);
            AssertEmptyList(info.Users);
        }

        private void AssertEmptyList<T>(List<T> list)
        {
            Assert.AreEqual(0, list.Count);
        }
    }
}
