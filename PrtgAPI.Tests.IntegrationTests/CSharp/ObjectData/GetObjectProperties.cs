using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Tests.IntegrationTests.Support;
using PrtgAPI.Attributes;
using PrtgAPI.Internal;

namespace PrtgAPI.Tests.IntegrationTests.DataTests
{
    [TestClass]
    public class GetObjectProperties : BasePrtgClientTest
    {
        [TestMethod]
        public void Data_GetObjectProperties_RetrievesAllRawProperties()
        {
            var dictionary = client.GetObjectPropertiesRaw(0, ObjectType.Group);

            AssertEx.AreEqual("Root", dictionary["name"], "Failed to retrieve name");
            AssertEx.AreEqual(Settings.Server, dictionary["windowslogindomain"], "Failed to retrieve domain");
            AssertEx.AreEqual(Settings.WindowsUserName.ToLower(), dictionary["windowsloginusername"].ToLower(), "Failed to retrieve username");
        }

        [TestMethod]
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
                ObjectProperty.FileSizeUnit
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
                client.SetObjectProperty(Settings.Device, ObjectProperty.Host, Settings.Server);
            }
        }

        [TestMethod]
        public void Data_GetObjectProperties_Throws_RetrievingAnInvalidProperty()
        {
            AssertEx.Throws<PrtgRequestException>(
                () => client.GetObjectPropertyRaw(Settings.UpSensor, "banana"),
                "A value for property 'banana' could not be found"
            );
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

        private List<Tuple<string, SensorOrDeviceOrGroupOrProbe, object>> GetSettings<TObject, TSettings>(Func<List<TObject>> getObjects, Func<int, TSettings> getSettings) where TObject : SensorOrDeviceOrGroupOrProbe
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
                    AssertEx.AreEqual(prop.Item3, direct, $"Values of property '{prop.Item1}' were not equal");

                AssertEx.AreEqual(prop.Item3.GetType(), direct.GetType(), $"Types of property '{prop.Item1}' were not equal");
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
            var category = property.GetEnumAttribute<CategoryAttribute>(true).Name.ToEnum<ObjectPropertyCategory>();

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
