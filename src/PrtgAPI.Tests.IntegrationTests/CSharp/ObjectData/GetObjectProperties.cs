using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;

namespace PrtgAPI.Tests.IntegrationTests.ObjectData
{
    [TestClass]
    public class GetObjectProperties : BasePrtgClientTest
    {
        [TestMethod]
        [IntegrationTest]
        public void Data_GetObjectProperties_RetrievesAllRawProperties()
        {
            var dictionary = client.GetObjectPropertiesRaw(0, ObjectType.Group);

            AssertEx.AreEqual("Root", dictionary["name"], "Failed to retrieve name");
            AssertEx.AreEqual(Settings.Server, dictionary["windowslogindomain"], "Failed to retrieve domain");
            AssertEx.AreEqual(Settings.WindowsUserName.ToLower(), dictionary["windowsloginusername"].ToLower(), "Failed to retrieve username");
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_GetObjectProperties_CanGetIndividualProperties()
        {
            var blacklist = new[]
            {
                ObjectProperty.SNMPCommunityStringV1,
                ObjectProperty.SNMPCommunityStringV2,
                ObjectProperty.WindowsPassword,
                ObjectProperty.LinuxPassword,
                ObjectProperty.LinuxPrivateKey,
                ObjectProperty.SSHElevationPassword,
                ObjectProperty.VMwarePassword,
                ObjectProperty.SNMPv3Password,
                ObjectProperty.SNMPv3EncryptionKey,
                ObjectProperty.DBPassword,
                ObjectProperty.AmazonSecretKey,
                ObjectProperty.SqlServerQuery,
                ObjectProperty.SqlProcessingMode,
                ObjectProperty.InheritTriggers,
                ObjectProperty.Comments,
                ObjectProperty.BandwidthVolumeUnit,
                ObjectProperty.BandwidthSpeedUnit,
                ObjectProperty.BandwidthTimeUnit,
                ObjectProperty.MemoryUsageUnit,
                ObjectProperty.DiskSizeUnit,
                ObjectProperty.FileSizeUnit,
                ObjectProperty.LocationName
            }.ToList();

            var objectProperties = Enum.GetValues(typeof(ObjectProperty)).Cast<ObjectProperty>().Where(p => !blacklist.Contains(p)).ToList();

            var props = GetPropertiesForAnalysis(blacklist);

            Analyze(props, objectProperties);

            try
            {
                if (objectProperties.Count > 0)
                {
                    SetMissingProperties(objectProperties);

                    //Get all properties again, then exclude all properties we processed before
                    var props2 = GetPropertiesForAnalysis(blacklist).Where(p => props.All(p2 => p.Item1 != p2.Item1)).ToList();

                    Analyze(props2, objectProperties);
                }
            }
            finally
            {
                ServerManager.RepairConfig();
                ServerManager.WaitForObjects();
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_GetObjectProperties_Throws_RetrievingAnInvalidProperty()
        {
            Action action = () => client.GetObjectPropertyRaw(Settings.UpSensor, "banana");

            if (IsEnglish)
            {
                AssertEx.Throws<PrtgRequestException>(
                    action,
                    "A value for property 'banana' could not be found"
                );
            }
            else
                action();
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_GetObjectProperties_IllegalXmlCharacter_NormalEndpoint()
        {
            TestIllegalXmlCharacter("first & second", Settings.CommentSensor, ObjectProperty.Name, s => s.Name);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_GetObjectProperties_IllegalXmlCharacter_AlternateEndpoint()
        {
            TestIllegalXmlCharacter("first & second", Settings.CommentSensor, ObjectProperty.Comments, s => s.Comments);
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_GetObjectProperties_IllegalHtmlCharacter_NormalProperty()
        {
            TestIllegalXmlCharacter("Z:\\", Settings.UpSensor, ObjectProperty.Name, s => s.Name);
        }

        private void TestIllegalXmlCharacter(string str, int objectId, ObjectProperty property, Func<Sensor, string> getValue)
        {
            var originalProperty = client.GetObjectProperty<string>(objectId, property);
            var originalValue = getValue(client.GetSensor(objectId));

            AssertEx.AreEqual(originalValue, originalProperty, "Original value did not match original property value");

            try
            {
                client.SetObjectProperty(objectId, property, str);

                var newProperty = client.GetObjectProperty<string>(objectId, property);
                var newValue = getValue(client.GetSensor(objectId));

                Assert.AreNotEqual(originalProperty, newProperty);
                Assert.AreNotEqual(newValue, originalValue);

                Assert.AreEqual(newProperty, newValue);
            }
            finally
            {
                client.SetObjectProperty(objectId, property, originalValue);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_GetObjectProperties_IllegalXmlCharacter_RawProperty()
        {
            TestIllegalCharacter_RawProperty("first & second");
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_GetObjectProperties_IllegalHtmlCharacter_RawProperty()
        {
            TestIllegalCharacter_RawProperty("Z:\\");
        }

        private void TestIllegalCharacter_RawProperty(string str)
        {
            var originalProperty = client.GetObjectPropertyRaw(Settings.UpSensor, "name_");
            var originalValue = client.GetSensor(Settings.UpSensor).Name;

            AssertEx.AreEqual(originalValue, originalProperty, "Original value did not match original property value");

            try
            {
                client.SetObjectPropertyRaw(Settings.UpSensor, "name_", str);

                var newProperty = client.GetObjectPropertyRaw(Settings.UpSensor, "name_");
                var newValue = client.GetSensor(Settings.UpSensor).Name;

                Assert.AreNotEqual(originalProperty, newProperty);
                Assert.AreNotEqual(newValue, originalValue);

                Assert.AreEqual(newProperty, newValue);
            }
            finally
            {
                client.SetObjectPropertyRaw(Settings.UpSensor, "name_", originalValue);
            }
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_ObjectProperties_ReadOnlyUser()
        {
            Action<Action> action = f => AssertEx.Throws<InvalidOperationException>(f, "Cannot retrieve properties for read-only");

            action(() => readOnlyClient.GetSensorProperties(Settings.UpSensor));
            action(() => readOnlyClient.GetDeviceProperties(Settings.Device));
            action(() => readOnlyClient.GetGroupProperties(Settings.Group));
            action(() => readOnlyClient.GetProbeProperties(Settings.Probe));
        }

        [TestMethod]
        [IntegrationTest]
        public async Task Data_ObjectProperties_ReadOnlyUserAsync()
        {
            Func<Func<Task>, Task> action = f => AssertEx.ThrowsAsync<InvalidOperationException>(f, "Cannot retrieve properties for read-only");

            await action(async () => await readOnlyClient.GetSensorPropertiesAsync(Settings.UpSensor));
            await action(async () => await readOnlyClient.GetDevicePropertiesAsync(Settings.Device));
            await action(async () => await readOnlyClient.GetGroupPropertiesAsync(Settings.Group));
            await action(async () => await readOnlyClient.GetProbePropertiesAsync(Settings.Probe));
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_ObjectProperties_SplitsSpaceArray()
        {
            TestSplittableString("first second", new[] { "first", "second" });
        }

        [TestMethod]
        [IntegrationTest]
        public void Data_ObjectProperties_SplitsCommaArray()
        {
            TestSplittableString("first,second", new[] { "first", "second" });
        }

        private void TestSplittableString(string serialized, string[] deserialized)
        {
            var sensor = client.GetSensor(Settings.UpSensor);

            var properties = client.GetSensorProperties(sensor);

            var originalObjectTags = sensor.Tags.Except(properties.ParentTags).ToList();
            var originalSettingsTags = properties.Tags.ToList();
            var originalPropertyTags = client.GetObjectProperty<string[]>(sensor, ObjectProperty.Tags).ToList();

            AssertEx.AreEqualLists(originalObjectTags, originalSettingsTags, "Original object tags and settings tags were not equal");
            AssertEx.AreEqualLists(originalObjectTags, originalPropertyTags, "Original object tags and property tags were not equal");

            try
            {
                var expected = deserialized.ToList();

                AssertEx.AreNotEqualLists(originalSettingsTags, expected.ToList(), "Original and proposed new lists were the same");

                client.SetObjectProperty(Settings.UpSensor, ObjectProperty.Tags, serialized);

                var newProperties = client.GetSensorProperties(sensor);

                var newObjectTags = client.GetSensor(Settings.UpSensor).Tags.Except(newProperties.ParentTags).ToList();
                var newSettingsTags = newProperties.Tags.ToList();
                var newPropertyTags = client.GetObjectProperty<string[]>(sensor, ObjectProperty.Tags).ToList();

                AssertEx.AreEqualLists(expected, newObjectTags, "New object tags did were not correct");
                AssertEx.AreEqualLists(expected, newSettingsTags, "New settings tags were not correct");
                AssertEx.AreEqualLists(expected, newPropertyTags, "New property tags were not correct");
            }
            finally
            {
                client.SetObjectProperty(Settings.UpSensor, ObjectProperty.Tags, originalSettingsTags);
            }
        }

        private List<Tuple<string, SensorOrDeviceOrGroupOrProbe, object>> GetPropertiesForAnalysis(List<ObjectProperty> blacklist)
        {
            var list = GetObjectPropertyMaps();

            var grouped = list.GroupBy(item => item.Item1).ToList();

            var properties = grouped.Select(g => g.FirstOrDefault(p => !string.IsNullOrEmpty(p.Item3?.ToString())))
                .Where(firstWithVal => firstWithVal != null).ToList();

            properties = properties.Where(p =>
            {
                ObjectProperty pr;

                return Enum.TryParse(p.Item1, out pr) && !blacklist.Contains(pr); //Ignore all ObjectPropertyInternal values
            }).ToList();

            return properties;
        }

        private List<Tuple<string, SensorOrDeviceOrGroupOrProbe, object>> GetObjectPropertyMaps()
        {
            var list = new List<Tuple<string, SensorOrDeviceOrGroupOrProbe, object>>();

            list.AddRange(GetSettings(client.GetProbes, client.GetProbeProperties));
            list.AddRange(GetSettings(client.GetGroups, client.GetGroupProperties));
            list.AddRange(GetSettings(client.GetDevices, client.GetDeviceProperties));
            list.AddRange(GetSettings(client.GetSensors, client.GetSensorProperties));

            return list;
        }

        private List<Tuple<string, SensorOrDeviceOrGroupOrProbe, object>> GetSettings<TObject, TSettings>(Func<List<TObject>> getObjects, Func<Either<TObject, int>, TSettings> getSettings) where TObject : SensorOrDeviceOrGroupOrProbe
        {
            var settings = getObjects().Select(o => new ObjectPropertyMap<TObject, TSettings>(o, getSettings(o.Id))).ToList();

            return GetTuple(settings);
        }

        private List<Tuple<string, SensorOrDeviceOrGroupOrProbe, object>> GetTuple<TObject, TProperties>(List<ObjectPropertyMap<TObject, TProperties>> objs) where TObject : SensorOrDeviceOrGroupOrProbe
        {
            return objs.SelectMany(o => o.Properties.GetType().GetProperties().Select(p => Tuple.Create(p.Name, (SensorOrDeviceOrGroupOrProbe)o.Object, p.GetValue(o.Properties)))).ToList();
        }

        private void Analyze(List<Tuple<string, SensorOrDeviceOrGroupOrProbe, object>> typedProperties, List<ObjectProperty> allProperties)
        {
            foreach (var prop in typedProperties)
            {
                var property = prop.Item1.ToEnum<ObjectProperty>();

                allProperties.Remove(property);

                if (property.ToString().StartsWith("Inherit"))
                    continue;

                var direct = client.GetObjectProperty(prop.Item2.Id, property);

                if (prop.Item3 is IEnumerable)
                    AssertEx.AreEqual(string.Join(" ", (IEnumerable)prop.Item3), string.Join(" ", (IEnumerable)direct), $"Values of property '{prop.Item1}' were not equal");
                else
                {
                    var first = prop.Item3;
                    var second = direct;

                    if (first is Schedule)
                        first = ((Schedule) first).Id.ToString();

                    AssertEx.AreEqual(first, second, $"Values of property '{prop.Item1}' were not equal");
                }

                var firstType = prop.Item3.GetType();
                var secondType = direct.GetType();

                if (firstType == typeof(Schedule))
                    firstType = typeof(string);

                AssertEx.AreEqual(firstType, secondType, $"Types of property '{prop.Item1}' were not equal");
            }
        }

        private void SetMissingProperties(List<ObjectProperty> objectProperties)
        {
            foreach (var prop in objectProperties)
            {
                var id = GetMissingPropertyTarget(prop);

                try
                {
                    client.SetObjectProperty(id, prop, "prop_tempValue");
                }
                catch (Exception)
                {
                    client.SetObjectProperty(id, prop, 9999);
                }
            }
        }

        private int GetMissingPropertyTarget(ObjectProperty property)
        {
            var category = (ObjectPropertyCategory)PrtgAPIHelpers.GetPropertyCategory(property);

            switch (category)
            {
                case ObjectPropertyCategory.Devices:
                case ObjectPropertyCategory.CredentialsForLinux:
                case ObjectPropertyCategory.CredentialsForVMware:
                case ObjectPropertyCategory.CredentialsForSNMP:
                case ObjectPropertyCategory.CredentialsForDatabases:
                case ObjectPropertyCategory.CredentialsForAmazon:
                    return Settings.Device;

                case ObjectPropertyCategory.HttpSpecific:
                case ObjectPropertyCategory.ProxySettingsForHttp:
                    return Settings.UpSensor;

                case ObjectPropertyCategory.SensorSettingsExeXml:
                    return Settings.ExeXml;

                case ObjectPropertyCategory.SensorFactorySpecific:
                    return Settings.SensorFactory;

                case ObjectPropertyCategory.DatabaseSpecific:
                case ObjectPropertyCategory.Data:
                    return Settings.SqlServerDB;

                default:
                    throw new NotImplementedException($"Handler for object category '{category}' is not implemented");
            }
        }

        class ObjectPropertyMap<TObject, TProperties> where TObject : SensorOrDeviceOrGroupOrProbe
        {
            public TObject Object { get; set; }

            public TProperties Properties { get; set; }

            public ObjectPropertyMap(TObject obj, TProperties props)
            {
                Object = obj;
                Properties = props;
            }
        }
    }
}
