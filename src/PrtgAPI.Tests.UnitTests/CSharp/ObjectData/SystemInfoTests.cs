using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class SystemInfoTests : BaseTest
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_CanDeserialize_FullSummary()
        {
            var client = GetFullSummaryClient();            

            var info = client.GetSystemInfo(1001);

            AssertFullSummaryResponse(info);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SystemInfo_CanDeserialize_FullSummaryAsync()
        {
            var client = GetFullSummaryClient();

            var info = await client.GetSystemInfoAsync(1001);

            AssertFullSummaryResponse(info);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_CorrectAddresses()
        {
            Execute(
                c => c.GetSystemInfo(40),
                new[] {
                    UnitRequest.Get("api/table.json?id=40&content=sysinfo&columns=_key,_value,_id,_adapter,_receivetime,_displayname&category=system"),
                    UnitRequest.Get("api/table.json?id=40&content=sysinfo&columns=_name,_description,_class,_caption,_state,_serialnumber,_capacity,_receivetime,_displayname&category=hardware"),
                    UnitRequest.Get("api/table.json?id=40&content=sysinfo&columns=_name,_vendor,_version,_date,_size,_receivetime,_displayname&category=software"),
                    UnitRequest.Get("api/table.json?id=40&content=sysinfo&columns=_processid,_caption,_creationdate,_receivetime,_displayname&category=processes"),
                    UnitRequest.Get("api/table.json?id=40&content=sysinfo&columns=_name,_description,_startname,_startmode,_state,_receivetime,_displayname&category=services"),
                    UnitRequest.Get("api/table.json?id=40&content=sysinfo&columns=_domain,_user,_receivetime,_displayname&category=loggedonusers")
                }
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_CanDeserialize_Generic()
        {
            var client = GetFullSummaryClient();

            var deviceId = 1001;

            var system = client.GetSystemInfo<DeviceSystemInfo>(deviceId);
            var hardware = client.GetSystemInfo<DeviceHardwareInfo>(deviceId);
            var software = client.GetSystemInfo<DeviceSoftwareInfo>(deviceId);
            var processes = client.GetSystemInfo<DeviceProcessInfo>(deviceId);
            var services = client.GetSystemInfo<DeviceServiceInfo>(deviceId);
            var users = client.GetSystemInfo<DeviceUserInfo>(deviceId);

            var info = new SystemInfo(deviceId, system, hardware, software, processes, services, users);

            AssertFullSummaryResponse(info);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SystemInfo_CanDeserialize_GenericAsync()
        {
            var client = GetFullSummaryClient();

            var deviceId = 1001;

            var system = await client.GetSystemInfoAsync<DeviceSystemInfo>(deviceId);
            var hardware = await client.GetSystemInfoAsync<DeviceHardwareInfo>(deviceId);
            var software = await client.GetSystemInfoAsync<DeviceSoftwareInfo>(deviceId);
            var processes = await client.GetSystemInfoAsync<DeviceProcessInfo>(deviceId);
            var services = await client.GetSystemInfoAsync<DeviceServiceInfo>(deviceId);
            var users = await client.GetSystemInfoAsync<DeviceUserInfo>(deviceId);

            var info = new SystemInfo(deviceId, system, hardware, software, processes, services, users);

            AssertFullSummaryResponse(info);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_CanDeserialize_NonGeneric()
        {
            var client = GetFullSummaryClient();

            var deviceId = 1001;

            var system = client.GetSystemInfo(deviceId, SystemInfoType.System).Cast<DeviceSystemInfo>().ToList();
            var hardware = client.GetSystemInfo(deviceId, SystemInfoType.Hardware).Cast<DeviceHardwareInfo>().ToList();
            var software = client.GetSystemInfo(deviceId, SystemInfoType.Software).Cast<DeviceSoftwareInfo>().ToList();
            var processes = client.GetSystemInfo(deviceId, SystemInfoType.Processes).Cast<DeviceProcessInfo>().ToList();
            var services = client.GetSystemInfo(deviceId, SystemInfoType.Services).Cast<DeviceServiceInfo>().ToList();
            var users = client.GetSystemInfo(deviceId, SystemInfoType.Users).Cast<DeviceUserInfo>().ToList();

            var info = new SystemInfo(deviceId, system, hardware, software, processes, services, users);

            AssertFullSummaryResponse(info);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SystemInfo_CanDeserialize_NonGenericAsync()
        {
            var client = GetFullSummaryClient();

            var deviceId = 1001;

            var system = (await client.GetSystemInfoAsync(deviceId, SystemInfoType.System)).Cast<DeviceSystemInfo>().ToList();
            var hardware = (await client.GetSystemInfoAsync(deviceId, SystemInfoType.Hardware)).Cast<DeviceHardwareInfo>().ToList();
            var software = (await client.GetSystemInfoAsync(deviceId, SystemInfoType.Software)).Cast<DeviceSoftwareInfo>().ToList();
            var processes = (await client.GetSystemInfoAsync(deviceId, SystemInfoType.Processes)).Cast<DeviceProcessInfo>().ToList();
            var services = (await client.GetSystemInfoAsync(deviceId, SystemInfoType.Services)).Cast<DeviceServiceInfo>().ToList();
            var users = (await client.GetSystemInfoAsync(deviceId, SystemInfoType.Users)).Cast<DeviceUserInfo>().ToList();

            var info = new SystemInfo(deviceId, system, hardware, software, processes, services, users);

            AssertFullSummaryResponse(info);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_ReturnsNoitems()
        {
            var client = Initialize_Client(new SystemInfoResponse());

            var items = client.GetSystemInfo<DeviceSystemInfo>(1001);

            Assert.AreEqual(0, items.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SystemInfo_ReturnsNoitemsAsync()
        {
            var client = Initialize_Client(new SystemInfoResponse());

            var items = await client.GetSystemInfoAsync<DeviceSystemInfo>(1001);

            Assert.AreEqual(0, items.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_AllPropertiesHaveValues()
        {
            AssertAllSystemInfoPropertiesHaveValues<DeviceSystemInfo>(SystemInfoItem.SystemItem(), p =>
            {
                if (p.Name == "Type")
                    return true;

                return false;
            });

            AssertAllSystemInfoPropertiesHaveValues<DeviceHardwareInfo>(SystemInfoItem.HardwareItem(serialNumber: "1234", capacity: "100"));
            AssertAllSystemInfoPropertiesHaveValues<DeviceSoftwareInfo>(SystemInfoItem.SoftwareItem());
            AssertAllSystemInfoPropertiesHaveValues<DeviceProcessInfo>(SystemInfoItem.ProcessItem());
            AssertAllSystemInfoPropertiesHaveValues<DeviceServiceInfo>(SystemInfoItem.ServiceItem());
            AssertAllSystemInfoPropertiesHaveValues<DeviceUserInfo>(SystemInfoItem.UserItem());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_AllPropertiesAreNull()
        {
            AssertAllSystemInfoPropertiesDontHaveValues<DeviceSystemInfo>(SystemInfoItem.SystemItem(null, null, null, null, null, null));
            AssertAllSystemInfoPropertiesDontHaveValues<DeviceHardwareInfo>(SystemInfoItem.HardwareItem(null, null, null, null, null, null, null, null, null));
            AssertAllSystemInfoPropertiesDontHaveValues<DeviceSoftwareInfo>(SystemInfoItem.SoftwareItem(null, null, null, null, null, null, null));
            AssertAllSystemInfoPropertiesDontHaveValues<DeviceProcessInfo>(SystemInfoItem.ProcessItem(null, null, null, null, null));
            AssertAllSystemInfoPropertiesDontHaveValues<DeviceServiceInfo>(SystemInfoItem.ServiceItem(null, null, null, null, null, null, null));
            AssertAllSystemInfoPropertiesDontHaveValues<DeviceUserInfo>(SystemInfoItem.UserItem(null, null, null, null));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_AllPropertiesAreEmpty()
        {
            AssertAllSystemInfoPropertiesDontHaveValues<DeviceSystemInfo>(SystemInfoItem.SystemItem("\"\"", "\"\"", "\"\"", "\"\"", "01-02-2003 21:10:13.425", "\"\""));
            AssertAllSystemInfoPropertiesDontHaveValues<DeviceHardwareInfo>(SystemInfoItem.HardwareItem("\"\"", "\"\"", "\"\"", "\"\"", "\"\"", "\"\"", "\"\"", "01-02-2003 21:10:13.425", "\"\""));
            AssertAllSystemInfoPropertiesDontHaveValues<DeviceSoftwareInfo>(SystemInfoItem.SoftwareItem("\"\"", "\"\"", "\"\"", "\"\"", "\"\"", "01-02-2003 21:10:13.425", "\"\""));
            AssertAllSystemInfoPropertiesDontHaveValues<DeviceProcessInfo>(SystemInfoItem.ProcessItem("\"0\"", "\"\"", "\"\"", "01-02-2003 21:10:13.425", "\"\""));
            AssertAllSystemInfoPropertiesDontHaveValues<DeviceServiceInfo>(SystemInfoItem.ServiceItem("\"\"", "\"\"", "\"\"", "\"\"", "\"\"", "01-02-2003 21:10:13.425", "\"\""));
            AssertAllSystemInfoPropertiesDontHaveValues<DeviceUserInfo>(SystemInfoItem.UserItem("\"\"", "\"\"", "01-02-2003 21:10:13.425", "\"\""));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_NoPropertiesAreStrings()
        {
            GetSystemInfoResponse<DeviceSystemInfo>(SystemInfoItem.SystemItem("1", "2", "3", "4", "01-02-2003 21:10:13.425", "5"));
            GetSystemInfoResponse<DeviceHardwareInfo>(SystemInfoItem.HardwareItem("1", "2", "3", "4", "5", "6", "7", "01-02-2003 21:10:13.425", "8"));
            GetSystemInfoResponse<DeviceSoftwareInfo>(SystemInfoItem.SoftwareItem("1", "2", "3", "\"2017-05-23-00-00-00\"", "5", "01-02-2003 21:10:13.425", "6"));
            GetSystemInfoResponse<DeviceProcessInfo>(SystemInfoItem.ProcessItem("1", "2", "\"2018-08-31 19:36:26\"", "01-02-2003 21:10:13.425", "4"));
            GetSystemInfoResponse<DeviceServiceInfo>(SystemInfoItem.ServiceItem("1", "2", "3", "4", "5", "01-02-2003 21:10:13.425", "6"));
            GetSystemInfoResponse<DeviceUserInfo>(SystemInfoItem.UserItem("1", "2", "01-02-2003 21:10:13.425", "3"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_NotSupported_AllLanguages()
        {
            var english = "{\"prtg-version\":\"14.4.12.3283\",\"treesize\":3,\"sysinfo\":[{\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\"},{\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\"},{\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\",\"\":\"Not found\",\"_raw\":\"\"}]}";
            var german = "{\"prtg-version\":\"14.4.12.3283\",\"treesize\":3,\"sysinfo\":[{\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\"},{\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\"},{\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\",\"\":\"Nicht gefunden\",\"_raw\":\"\"}]}";
            var japanese = "{\"prtg-version\":\"14.4.12.3283\",\"treesize\":3,\"sysinfo\":[{\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\"},{\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\"},{\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\",\"\":\"見つかりません\",\"_raw\":\"\"}]}";

            AssertEx.AssertErrorResponseAllLanguages<PrtgRequestException>(
                english,
                german,
                japanese,
                "Failed to receive System Information: content type not supported PRTG Server.",
                c => c.GetSystemInfo<DeviceSystemInfo>(1001)
            );
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_Software_Version_MajorNumberOnly()
        {
            var obj = GetSystemInfoResponse<DeviceSoftwareInfo>(SystemInfoItem.SoftwareItem(version: "5"));

            Assert.AreEqual(obj.Version.ToString(), "5.0");
        }

        #region Escape Quotes

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_EscapesQuotes_FirstObject_FirstProperty()
        {
            var normal = SystemInfoItem.UserItem();
            var test = SystemInfoItem.UserItem("first", "\"second\"", displayName: "\"third\"");

            ValidateEscapeQuotes(1, 0, test, normal);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_EscapesQuotes_FirstObject_MiddleProperty()
        {
            var normal = SystemInfoItem.UserItem();
            var test = SystemInfoItem.UserItem("\"first\"", "second", displayName: "\"third\"");

            ValidateEscapeQuotes(1, 0, test, normal);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_EscapesQuotes_FirstObject_LastProperty()
        {
            var normal = SystemInfoItem.UserItem();
            var test = SystemInfoItem.UserItem("\"first\"", "\"second\"", displayName: "third");

            ValidateEscapeQuotes(1, 0, test, normal);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_EscapesQuotes_LastObject_FirstProperty()
        {
            var normal = SystemInfoItem.UserItem();
            var test = SystemInfoItem.UserItem("first", "\"second\"", displayName: "\"third\"");

            ValidateEscapeQuotes(0, 1, normal, test);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_EscapesQuotes_LastObject_MiddleProperty()
        {
            var normal = SystemInfoItem.UserItem();
            var test = SystemInfoItem.UserItem("\"first\"", "second", displayName: "\"third\"");

            ValidateEscapeQuotes(0, 1, normal, test);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_EscapesQuotes_LastObject_LastProperty()
        {
            var normal = SystemInfoItem.UserItem();
            var test = SystemInfoItem.UserItem("\"first\"", "\"second\"", displayName: "third");

            ValidateEscapeQuotes(0, 1, normal, test);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_EscapesQuotes_Numbers()
        {
            var item = SystemInfoItem.ProcessItem("\"2002\"");
            var result = GetSystemInfoResponse<DeviceProcessInfo>(item);
            Assert.AreEqual(2002, result.ProcessId);
        }

        private void ValidateEscapeQuotes(int normalIndex, int testIndex, params SystemInfoItem[] items)
        {
            var client = GetSystemInfoClient(items);
            var users = client.GetSystemInfo<DeviceUserInfo>(1001);

            Assert.AreEqual(2, users.Count);

            Assert.AreEqual("PRTG-1", users[normalIndex].Domain);
            Assert.AreEqual("NETWORK SERVICE", users[normalIndex].User);
            Assert.AreEqual("PRTG-1\\NETWORK SERVICE", users[normalIndex].DisplayName);

            Assert.AreEqual("first", users[testIndex].Domain);
            Assert.AreEqual("second", users[testIndex].User);
            Assert.AreEqual("third", users[testIndex].DisplayName);
        }

        #endregion

        #region Property Map

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_System_PropertiesMapCorrectly()
        {
            var key = "\"Key / Adapter\"";
            var value = "\"Value\"";
            var id = "3";
            var adapter = "\"Adapter\"";
            var receiveTime = "01-02-2003 21:10:13.425";
            var displayName = "\"DisplayName\"";

            var obj = GetSystemInfoResponse<DeviceSystemInfo>(
                SystemInfoItem.SystemItem(key, value, id, adapter, receiveTime, displayName)
            );

            AssertAreEqual("Key", obj.Property);
            AssertAreEqual(value, obj.Value);
            AssertAreEqual(Convert.ToInt32(id), obj.Id);
            AssertAreEqual(adapter, obj.Adapter);
            AssertAreEqual(DateTime.ParseExact(receiveTime, "dd-MM-yyyy HH:mm:ss.FFF", CultureInfo.InvariantCulture), obj.LastUpdated);
            AssertAreEqual(displayName, obj.DisplayName);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_Hardware_PropertiesMapCorrectly()
        {
            var name = "\"Name\"";
            var description = "\"Description\"";
            var @class = "\"Class\"";
            var caption = "\"Caption\"";
            var state = "\"<img src=\\\"/images/state_.png\\\"> State\"";
            var serialNumber = "\"SerialNumber\"";
            var capacity = "1234567890123456789";
            var receiveTime = "01-02-2003 21:10:13.425";
            var displayName = "\"DisplayName\"";

            var obj = GetSystemInfoResponse<DeviceHardwareInfo>(
                SystemInfoItem.HardwareItem(name, description, @class, caption,
                    state, serialNumber, capacity, receiveTime, displayName
                )
            );

            AssertAreEqual(name, obj.Name);
            AssertAreEqual(description, obj.Description);
            AssertAreEqual(@class, obj.Class);
            AssertAreEqual(caption, obj.Caption);
            AssertAreEqual("State", obj.State);
            AssertAreEqual(serialNumber, obj.SerialNumber);
            AssertAreEqual(Convert.ToInt64(capacity), obj.Capacity);
            AssertAreEqual(DateTime.ParseExact(receiveTime, "dd-MM-yyyy HH:mm:ss.FFF", CultureInfo.InvariantCulture), obj.LastUpdated);
            AssertAreEqual(displayName, obj.DisplayName);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_Software_PropertiesMapCorrectly()
        {
            var name = "\"Name\"";
            var vendor = "\"Vendor\"";
            var version = "1.2";
            var date = "\"2017-05-23-00-00-00\"";
            var size = "1234";
            var receiveTime = "01-02-2003 21:10:13.425";
            var displayName = "\"DisplayName\"";

            var obj = GetSystemInfoResponse<DeviceSoftwareInfo>(
                SystemInfoItem.SoftwareItem(name, vendor, version, date, size, receiveTime, displayName)
            );

            AssertAreEqual(name, obj.Name);
            AssertAreEqual(vendor, obj.Vendor);
            AssertAreEqual(Version.Parse(version), obj.Version);
            AssertAreEqual(TypeHelpers.StringToDate(date.Trim('"')), obj.InstallDate);
            AssertAreEqual(Convert.ToInt32(size), obj.Size);
            AssertAreEqual(DateTime.ParseExact(receiveTime, "dd-MM-yyyy HH:mm:ss.FFF", CultureInfo.InvariantCulture), obj.LastUpdated);
            AssertAreEqual(displayName, obj.DisplayName);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_Process_PropertiesMapCorrectly()
        {
            var processId = "3";
            var caption = "\"Caption\"";
            var creationDate = "\"2018-08-31 19:36:26\"";
            var receiveTime = "01-02-2003 21:10:13.425";
            var displayName = "\"DisplayName\"";

            var obj = GetSystemInfoResponse<DeviceProcessInfo>(
                SystemInfoItem.ProcessItem(processId, caption, creationDate, receiveTime, displayName)
            );

            AssertAreEqual(Convert.ToInt32(processId), obj.ProcessId);
            AssertAreEqual(caption, obj.Caption);
            AssertAreEqual(DateTime.ParseExact(creationDate.Trim('"'), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), obj.CreationDate);
            AssertAreEqual(DateTime.ParseExact(receiveTime, "dd-MM-yyyy HH:mm:ss.FFF", CultureInfo.InvariantCulture), obj.LastUpdated);
            AssertAreEqual(displayName, obj.DisplayName);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_Service_PropertiesMapCorrectly()
        {
            var name = "\"Name\"";
            var description = "\"Description\"";
            var startName = "\"StartName\"";
            var startMode = "\"StartMode\"";
            var state = "\"<img src=\\\"/images/state_stopped.png\\\"> State\"";
            var receiveTime = "01-02-2003 21:10:13.425";
            var displayName = "\"DisplayName\"";

            var obj = GetSystemInfoResponse<DeviceServiceInfo>(
                SystemInfoItem.ServiceItem(name, description, startName, startMode, state, receiveTime, displayName)
            );

            AssertAreEqual(name, obj.Name);
            AssertAreEqual(description, obj.Description);
            AssertAreEqual(startName, obj.User);
            AssertAreEqual(startMode, obj.StartMode);
            AssertAreEqual("State", obj.State);
            AssertAreEqual(DateTime.ParseExact(receiveTime, "dd-MM-yyyy HH:mm:ss.FFF", CultureInfo.InvariantCulture), obj.LastUpdated);
            AssertAreEqual(displayName, obj.DisplayName);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_User_PropertiesMapCorrectly()
        {
            var domain = "\"Domain\"";
            var user = "\"User\"";
            var receiveTime = "01-02-2003 21:10:13.425";
            var displayName = "\"Domain\\\\User\"";

            var obj = GetSystemInfoResponse<DeviceUserInfo>(
                SystemInfoItem.UserItem(domain, user, receiveTime, displayName)
            );

            AssertAreEqual(domain, obj.Domain);
            AssertAreEqual(user, obj.User);
            AssertAreEqual(DateTime.ParseExact(receiveTime, "dd-MM-yyyy HH:mm:ss.FFF", CultureInfo.InvariantCulture), obj.LastUpdated);
            AssertAreEqual("Domain\\User", obj.DisplayName);
        }

        #endregion

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SystemInfo_ReadOnlyUser()
        {
            var client = Initialize_Client(new SystemInfoResponse());

            var info = client.GetSystemInfo(1001);

            var properties = info.GetType().GetProperties();

            foreach (var prop in properties)
                AssertEx.AllPropertiesRetrieveValues(prop.GetValue(info));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SystemInfo_ReadOnlyUserAsync()
        {
            var client = Initialize_Client(new SystemInfoResponse());

            var info = await client.GetSystemInfoAsync(1001);

            var properties = info.GetType().GetProperties();

            foreach (var prop in properties)
                AssertEx.AllPropertiesRetrieveValues(prop.GetValue(info));
        }

        private PrtgClient GetFullSummaryClient()
        {
            return GetSystemInfoClient(
                SystemInfoItem.SystemItem(), SystemInfoItem.SystemItem(),
                SystemInfoItem.HardwareItem(), SystemInfoItem.HardwareItem(),
                SystemInfoItem.SoftwareItem(), SystemInfoItem.SoftwareItem(),
                SystemInfoItem.ProcessItem(), SystemInfoItem.ProcessItem(),
                SystemInfoItem.ServiceItem(), SystemInfoItem.ServiceItem(),
                SystemInfoItem.UserItem(), SystemInfoItem.UserItem()
            );
        }

        private void AssertFullSummaryResponse(SystemInfo info)
        {
            Assert.AreEqual(1001, info.DeviceId);
            Assert.AreEqual(2, info.System.Count);
            Assert.AreEqual(2, info.Hardware.Count);
            Assert.AreEqual(2, info.Software.Count);
            Assert.AreEqual(2, info.Services.Count);
            Assert.AreEqual(2, info.Processes.Count);
            Assert.AreEqual(2, info.Users.Count);
        }

        private void AssertAllSystemInfoPropertiesHaveValues<T>(SystemInfoItem item, Func<PropertyInfo, bool> customHandler = null) where T : IDeviceInfo
        {
            var client = GetSystemInfoClient(item);

            var obj = client.GetSystemInfo<T>(1001).Single();

            AssertEx.AllPropertiesAreNotDefault(obj, customHandler);
        }

        private void AssertAllSystemInfoPropertiesDontHaveValues<T>(SystemInfoItem item, Func<PropertyInfo, bool> customHandler = null) where T : IDeviceInfo
        {
            var client = GetSystemInfoClient(item);

            Func<PropertyInfo, bool> innerCustomHandler = p =>
            {
                if (p.Name == nameof(IDeviceInfo.DeviceId) || p.Name == nameof(IDeviceInfo.LastUpdated))
                    return true;

                if (p.Name == nameof(IDeviceInfo.Type))
                    return true;

                if (customHandler != null)
                    return customHandler(p);

                return false;
            };

            var obj = client.GetSystemInfo<T>(1001).Single();

            AssertEx.AllPropertiesAreDefault(obj, innerCustomHandler);
        }

        private T GetSystemInfoResponse<T>(SystemInfoItem item) where T : IDeviceInfo
        {
            var client = GetSystemInfoClient(item);

            var obj = client.GetSystemInfo<T>(1001);

            return obj.Single();
        }

        private PrtgClient GetSystemInfoClient(params SystemInfoItem[] items)
        {
            var client = Initialize_Client(new SystemInfoResponse(items));

            return client;
        }

        private void AssertAreEqual<T>(T expected, T value)
        {
            if (expected is string)
                expected = (T)(object)((string)(object)expected).Trim('"');

            Assert.AreEqual(expected, value);
        }
    }
}
