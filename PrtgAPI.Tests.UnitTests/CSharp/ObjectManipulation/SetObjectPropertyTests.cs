using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Helpers;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class SetObjectPropertyTests : BaseTest
    {
        #region Type Parsing

        [TestMethod]
        public void SetObjectProperty_Enum_With_Int()
        {
            AssertEx.Throws<ArgumentException>(
                () => SetObjectProperty(ObjectProperty.IntervalErrorMode, 1),
                "'1' is not a valid value for enum IntervalErrorMode. Please specify one of 'DownImmediately'"
            );
        }

        [TestMethod]
        public void SetObjectProperty_Enum_With_Enum()
        {
            SetObjectProperty(ObjectProperty.IntervalErrorMode, IntervalErrorMode.DownImmediately, ((int)IntervalErrorMode.DownImmediately).ToString());
        }

        [TestMethod]
        public void SetObjectProperty_Bool_With_Bool()
        {
            SetObjectProperty(ObjectProperty.InheritInterval, false, "0");
        }

        [TestMethod]
        public void SetObjectProperty_Bool_With_Int()
        {
            SetObjectProperty(ObjectProperty.InheritInterval, true, "1");
        }

        [TestMethod]
        public void SetObjectProperty_Int_With_Enum()
        {
            AssertEx.Throws<InvalidTypeException>(
                () => SetObjectProperty(ObjectProperty.DBPort, Status.Up, "8"),
                "Expected type: 'System.Int32'. Actual type: 'PrtgAPI.Status'"
            );
        }

        [TestMethod]
        public void SetObjectProperty_Int_With_Int()
        {
            SetObjectProperty(ObjectProperty.DBPort, 8080);
        }

        [TestMethod]
        public void SetObjectProperty_Double_With_Int()
        {
            SetObjectProperty(ChannelProperty.ScalingDivision, 10);
        }

        [TestMethod]
        public void SetObjectProperty_Int_With_Double()
        {
            double val = 8080.0;
            SetObjectProperty(ObjectProperty.DBPort, val, "8080");
        }

        [TestMethod]
        public void SetObjectProperty_Int_With_Bool()
        {
            AssertEx.Throws<InvalidTypeException>(
                () => SetObjectProperty(ObjectProperty.DBPort, true, "1"),
                "Expected type: 'System.Int32'. Actual type: 'System.Boolean'"
            );
        }

        #endregion
        #region Normal

        [TestMethod]
        public void SetObjectProperty_ReverseDependencyProperty()
        {
            SetObjectProperty(ChannelProperty.ColorMode, AutoMode.Manual, "1");
        }

        [TestMethod]
        public async Task SetObjectProperty_CanExecuteAsync()
        {
            var client = Initialize_Client(new SetObjectPropertyResponse<ObjectProperty>(ObjectProperty.DBPort, "8080"));

            await client.SetObjectPropertyAsync(1001, ObjectProperty.DBPort, 8080);
        }

        [TestMethod]
        public void SetObjectProperty_CanSetLocation()
        {
            SetObjectProperty(ObjectProperty.Location, "23 Fleet Street, Boston", "23 Fleet St, Boston, MA 02113, USA");
        }

        [TestMethod]
        public void Location_ResolvesNothing()
        {
            var client = Initialize_Client(new SetObjectPropertyResponse<ObjectProperty>(ObjectProperty.Location, ""));

            SetObjectProperty(ObjectProperty.Location, null, "");
        }

        [TestMethod]
        public async Task Location_ResolvesNothingAsync()
        {
            var client = Initialize_Client(new SetObjectPropertyResponse<ObjectProperty>(ObjectProperty.Location, ""));

            var location = await Location.ResolveAsync(client, null);

            Assert.AreEqual(null, location.ToString());
        }

        [TestMethod]
        public void Location_FailsToResolve()
        {
            var client = Initialize_Client(new LocationUnresolvedResponse());

            AssertEx.Throws<PrtgRequestException>(() => Location.Resolve(client, "something"), "Could not resolve 'something' to an actual address");
        }

        [TestMethod]
        public async Task Location_FailsToResolveAsync()
        {
            var client = Initialize_Client(new LocationUnresolvedResponse());

            await AssertEx.ThrowsAsync<PrtgRequestException>(async () => await Location.ResolveAsync(client, "something"), "Could not resolve 'something' to an actual address");
        }

        [TestMethod]
        public void Location_ProviderUnavailable()
        {
            var client = Initialize_Client(new LocationUnresolvedResponse(true));

            AssertEx.Throws<PrtgRequestException>(() => Location.Resolve(client, "something"), "the PRTG map provider is not currently available");
        }

        [TestMethod]
        public void SetObjectProperty_CanExecute() =>
            Execute(c => c.SetObjectPropertyRaw(1001, "name_", "testName"), "editsettings?id=1001&name_=testName");

        [TestMethod]
        public async Task SetObjectPropertyRaw_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.SetObjectPropertyRawAsync(1001, "name_", "testName"), "editsettings?id=1001&name_=testName");

        [TestMethod]
        public async Task SetChannelProperty_CanExecuteAsync()
        {
            var client = Initialize_Client(new SetObjectPropertyResponse<ChannelProperty>(ChannelProperty.LimitsEnabled, "1"));

            await client.SetObjectPropertyAsync(1001, 1, ChannelProperty.LimitsEnabled, true);
        }

        [TestMethod]
        public void SetObjectProperty_CanNullifyValue()
        {
            SetObjectProperty(ChannelProperty.UpperErrorLimit, null, string.Empty);
        }

        [TestMethod]
        public async Task SetObjectProperty_CanSetLocationAsync()
        {
            var client = Initialize_Client(new SetObjectPropertyResponse<ObjectProperty>(ObjectProperty.Location, "23 Fleet St, Boston, MA 02113, USA"));

            await client.SetObjectPropertyAsync(1, ObjectProperty.Location, "23 Fleet Street");
        }

        [TestMethod]
        public void SetObjectProperty_Array()
        {
            var channels = new[]
            {
                "#1: First Channel",
                "channel(1001,1)",
                "#2: Second Channel",
                "channel(2001,1)"
            };

            SetObjectProperty(ObjectProperty.ChannelDefinition, channels, string.Join("\n", channels));
        }

        private void SetObjectProperty(ObjectProperty property, object value, string expectedSerializedValue = null)
        {
            if (expectedSerializedValue == null)
                expectedSerializedValue = value.ToString();

            var client = Initialize_Client(new SetObjectPropertyResponse<ObjectProperty>(property, expectedSerializedValue));
            client.SetObjectProperty(1, property, value);
        }

        private void SetObjectProperty(ChannelProperty property, object value, string expectedSerializedValue = null)
        {
            if (expectedSerializedValue == null)
                expectedSerializedValue = value.ToString();

            var client = Initialize_Client(new SetObjectPropertyResponse<ChannelProperty>(property, expectedSerializedValue));
            client.SetObjectProperty(1, 1, property, value);
        }

        [TestMethod]
        public void SetObjectProperty_DecimalPoint_AmericanCulture()
        {
            TestCustomCulture(() => SetObjectProperty(ChannelProperty.ScalingDivision, "1.1", "1.1"), new CultureInfo("en-US"));
        }

        [TestMethod]
        public void SetObjectProperty_DecimalPoint_EuropeanCulture()
        {
            TestCustomCulture(() =>
            {
                SetObjectProperty(ChannelProperty.ScalingDivision, "1,1", "1,1");
                SetObjectProperty(ChannelProperty.ScalingDivision, 1.1, "1,1");
            }, new CultureInfo("de-DE"));
        }

        private void TestCustomCulture(Action action, CultureInfo newCulture)
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = newCulture;

                action();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        #endregion
        #region Multiple

        [TestMethod]
        public void SetObjectProperty_Modifies_MultipleProperties()
        {
            var client = Initialize_Client(new AddressValidatorResponse("id=1001&windowsloginusername_=username&windowsloginpassword_=password&windowsconnection=0"));

            client.SetObjectProperty(
                1001,
                new PropertyParameter(ObjectProperty.WindowsUserName, "username"),
                new PropertyParameter(ObjectProperty.WindowsPassword, "password")
            );
        }

        [TestMethod]
        public async Task SetObjectProperty_Modifies_MultiplePropertiesAsync()
        {
            var client = Initialize_Client(new AddressValidatorResponse("id=1001&windowsloginusername_=username&windowsloginpassword_=password&windowsconnection=0"));

            await client.SetObjectPropertyAsync(
                1001,
                new PropertyParameter(ObjectProperty.WindowsUserName, "username"),
                new PropertyParameter(ObjectProperty.WindowsPassword, "password")
            );
        }

        [TestMethod]
        public void SetChannelProperty_Modifies_MultipleProperties()
        {
            var client = Initialize_Client(new AddressValidatorResponse("id=1001&limitmaxerror_2=100&limitminerror_2=20&limitmode_2=1"));

            client.SetObjectProperty(
                1001,
                2,
                new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                new ChannelParameter(ChannelProperty.LowerErrorLimit, 20)
            );
        }

        [TestMethod]
        public async Task SetChannelProperty_Modifies_MultiplePropertiesAsync()
        {
            var client = Initialize_Client(new AddressValidatorResponse("id=1001&limitmaxerror_2=100&limitminerror_2=20&limitmode_2=1"));

            await client.SetObjectPropertyAsync(
                1001,
                2,
                new ChannelParameter(ChannelProperty.UpperErrorLimit, 100),
                new ChannelParameter(ChannelProperty.LowerErrorLimit, 20)
            );
        }

        [TestMethod]
        public void SetObjectProperty_Modifies_MultipleRawProperties()
        {
            var client = Initialize_Client(new AddressValidatorResponse("id=1001&windowsloginusername_=username&windowsloginpassword_=password&windowsconnection=0"));

            client.SetObjectPropertyRaw(
                1001,
                new CustomParameter("windowsloginusername_", "username"),
                new CustomParameter("windowsloginpassword_", "password"),
                new CustomParameter("windowsconnection", 0)
            );
        }

        [TestMethod]
        public async Task SetObjectProperty_Modifies_MultipleRawPropertiesAsync()
        {
            var client = Initialize_Client(new AddressValidatorResponse("id=1001&windowsloginusername_=username&windowsloginpassword_=password&windowsconnection=0"));

            await client.SetObjectPropertyRawAsync(
                1001,
                new CustomParameter("windowsloginusername_", "username"),
                new CustomParameter("windowsloginpassword_", "password"),
                new CustomParameter("windowsconnection", 0)
            );
        }

        #endregion
        #region Version Specific
            #region Single

        [TestMethod]
        public void SetChannelProperty_SingleValue_OnlyUpperErrorLimit()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        public void SetChannelProperty_SingleValue_OnlyLowerErrorLimit()
        {
            var matrix = new[]
            {
                new int?[] {null, 1, null, null},
                new int?[] {null, 1, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=1" });
        }

        [TestMethod]
        public void SetChannelProperty_SingleValue_OnlyUpperWarningLimit()
        {
            var matrix = new[]
            {
                new int?[] {null, null, 1, null},
                new int?[] {null, null, 1, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxwarning_2=1" });
        }

        [TestMethod]
        public void SetChannelProperty_SingleValue_OnlyLowerWarningLimit()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, 1},
                new int?[] {null, null, null, 1}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitminwarning_2=1" });
        }

            #endregion
            #region Multiple

        [TestMethod]
        public void SetChannelProperty_MultipleValues_OnlyUpperErrorLimit()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {2, null, null, null},
                new int?[] {2, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1"
            });
        }

        [TestMethod]
        public void SetChannelProperty_MultipleValues_OnlyLowerErrorLimit()
        {
            var matrix = new[]
            {
                new int?[] {null, 1, null, null},
                new int?[] {null, 2, null, null},
                new int?[] {null, 2, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            //todo: does this actually group together the sensor IDs to execute against for the request?

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=1"
            });
        }

        [TestMethod]
        public void SetChannelProperty_MultipleValues_OnlyUpperWarningLimit()
        {
            var matrix = new[]
            {
                new int?[] {null, null, 1, null},
                new int?[] {null, null, 2, null},
                new int?[] {null, null, 2, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitmaxwarning_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitmaxwarning_2=1"
            });
        }

        [TestMethod]
        public void SetChannelProperty_MultipleValues_OnlyLowerWarningLimit()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, 1},
                new int?[] {null, null, null, 2},
                new int?[] {null, null, null, 2}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitminwarning_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitminwarning_2=1"
                
            });
        }

            #endregion
            #region Version Properties

        [TestMethod]
        public void SetChannelProperty_VersionProperty_ErrorLimitMessage()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        public void SetChannelProperty_VersionProperty_WarningLimitMessage()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.WarningLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limitwarningmsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limitwarningmsg_2=test&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        public void SetChannelProperty_VersionProperty_LimitsEnabled_True()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.LimitsEnabled,
                (object)true,
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        public void SetChannelProperty_VersionProperty_LimitsEnabled_False()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.LimitsEnabled,
                (object)false,
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&limitmode_2=0&limitmaxerror_2=&limitmaxwarning_2=&limitminerror_2=&limitminwarning_2=&limiterrormsg_2=&limitwarningmsg_2=" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&limitmode_2=0&limitmaxerror_2=&limitmaxwarning_2=&limitminerror_2=&limitminwarning_2=&limiterrormsg_2=&limitwarningmsg_2=" });
        }

        [TestMethod]
        public void SetChannelProperty_VersionProperty_NormalProperty()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.SpikeFilterMax,
                (object)100,
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001&spikemax_2=100&spikemode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[] { "id=1001,2001&spikemax_2=100&spikemode_2=1" });
        }

            #endregion

        [TestMethod]
        public void SetChannelProperty_ThreeValues()
        {
            var matrix = new[]
            {
                new int?[] {1,    null, 2,    1}, //1001 - 2
                new int?[] {1,    null, null, 4}, //2001 - 2
                new int?[] {5,    null, 8,    7}, //3001 - 4
                new int?[] {2,    3,    15,   6}, //4001 - 1
                new int?[] {null, 3,    20,   5}, //5001 - 1
                new int?[] {4,    3,    null, 8}  //6001 - 1
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            SetChannelProperty(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001,4001,5001,6001&limiterrormsg_2=test&limitmode_2=1" });
            SetChannelProperty(config, RequestVersion.v18_1, new[]
            {
                "id=4001,5001,6001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=3",
                "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1",
                "id=3001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=5"
            });
        }

        [TestMethod]
        public void SetChannelProperty_SomeNull()
        {
            var matrix = new[]
            {
                new int?[] {null, 2, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            Action<RequestVersion> action = version => SetChannelProperty(config, version, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });

            action(RequestVersion.v14_4);

            var builder = new StringBuilder();
            builder.Append("Cannot set property 'ErrorLimitMessage' to value 'test' for Channel ID 2: ");
            builder.Append("Sensor ID 2001 does not have a limit value defined on it. ");
            builder.Append("Please set one of 'UpperErrorLimit', 'LowerErrorLimit', 'UpperWarningLimit' or 'LowerWarningLimit' first and then try again");

            AssertEx.Throws<InvalidOperationException>(() => action(RequestVersion.v18_1), builder.ToString());
        }

        [TestMethod]
        public void SetChannelProperty_AllNull()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            Action<RequestVersion> action = version => SetChannelProperty(config, version, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });

            action(RequestVersion.v14_4);

            var builder = new StringBuilder();
            builder.Append("Cannot set property 'ErrorLimitMessage' to value 'test' for Channel ID 2: ");
            builder.Append("Sensor IDs 1001 and 2001 do not have a limit value defined on them. ");
            builder.Append("Please set one of 'UpperErrorLimit', 'LowerErrorLimit', 'UpperWarningLimit' or 'LowerWarningLimit' first and then try again");

            AssertEx.Throws<InvalidOperationException>(() => action(RequestVersion.v18_1), builder.ToString());
        }

        [TestMethod]
        public void SetChannelProperty_VersionSpecific_ResolvesChannels()
        {
            var addresses = new[]
            {
                "api/table.xml?content=channels&columns=objid,name,lastvalue&count=*&id=1001",
                "controls/channeledit.htm?id=1001&channel=1",
                "editsettings?id=1001&limiterrormsg_1=hello&limitmode_1=1&limitmaxerror_1=100"
            };

            var property = ChannelProperty.ErrorLimitMessage;
            var val = "hello";
            
            var client = Initialize_Client(new AddressValidatorResponse(addresses.Select(a => $"https://prtg.example.com/{a}&username=username&passhash=12345678").ToArray(), true));
            SetVersion(client, RequestVersion.v18_1);

            client.GetVersionClient(new object[] {property}).SetChannelProperty(new[] { 1001 }, 1, null, new[] { new ChannelParameter(property, val) });
        }

        private void SetChannelProperty(Tuple<ChannelProperty, object, int?[][]> config, RequestVersion version, string[] addresses)
        {
            var property = config.Item1;
            var val = config.Item2;
            var matrix = config.Item3;

            var channels = matrix.Select(CreateChannel).ToList();

            var client = Initialize_Client(new AddressValidatorResponse(addresses.Select(a => $"https://prtg.example.com/editsettings?{a}&username=username&passhash=12345678").ToArray(), true));
            SetVersion(client, version);

            client.GetVersionClient(new object[] { property }).SetChannelProperty(channels.Select(c => c.SensorId).ToArray(), 2, channels, new [] {new ChannelParameter(property, val)});
        }

        private void SetVersion(PrtgClient client, RequestVersion version)
        {
            var f = client.GetInternalFieldInfo("version");
            f.SetValue(client, new Version(version.ToString().TrimStart('v').Replace('_', '.')));
        }

        private Channel CreateChannel(int?[] limits, int i)
        {
            return new Channel
            {
                Id = 1,
                SensorId = 1001 + i*1000,
                UpperErrorLimit = limits[0],
                LowerErrorLimit = limits[1],
                UpperWarningLimit = limits[2],
                LowerWarningLimit = limits[3]
            };
        }

        #endregion
        #region Version Specific Async
            #region Single

        [TestMethod]
        public async Task SetChannelProperty_SingleValue_OnlyUpperErrorLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        public async Task SetChannelProperty_SingleValue_OnlyLowerErrorLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, 1, null, null},
                new int?[] {null, 1, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=1" });
        }

        [TestMethod]
        public async Task SetChannelProperty_SingleValue_OnlyUpperWarningLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, 1, null},
                new int?[] {null, null, 1, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxwarning_2=1" });
        }

        [TestMethod]
        public async Task SetChannelProperty_SingleValue_OnlyLowerWarningLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, 1},
                new int?[] {null, null, null, 1}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitminwarning_2=1" });
        }

            #endregion
            #region Multiple

        [TestMethod]
        public async Task SetChannelProperty_MultipleValues_OnlyUpperErrorLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {2, null, null, null},
                new int?[] {2, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1"
            });
        }

        [TestMethod]
        public async Task SetChannelProperty_MultipleValues_OnlyLowerErrorLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, 1, null, null},
                new int?[] {null, 2, null, null},
                new int?[] {null, 2, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            //todo: does this actually group together the sensor IDs to execute against for the request?

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=1"
            });
        }

        [TestMethod]
        public async Task SetChannelProperty_MultipleValues_OnlyUpperWarningLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, 1, null},
                new int?[] {null, null, 2, null},
                new int?[] {null, null, 2, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitmaxwarning_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitmaxwarning_2=1"
            });
        }

        [TestMethod]
        public async Task SetChannelProperty_MultipleValues_OnlyLowerWarningLimitAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, 1},
                new int?[] {null, null, null, 2},
                new int?[] {null, null, null, 2}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                "id=2001,3001&limiterrormsg_2=test&limitmode_2=1&limitminwarning_2=2",
                "id=1001&limiterrormsg_2=test&limitmode_2=1&limitminwarning_2=1"

            });
        }

            #endregion
            #region Version Properties

        [TestMethod]
        public async Task SetChannelProperty_VersionProperty_ErrorLimitMessageAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        public async Task SetChannelProperty_VersionProperty_WarningLimitMessageAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.WarningLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limitwarningmsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limitwarningmsg_2=test&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        public async Task SetChannelProperty_VersionProperty_LimitsEnabled_TrueAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.LimitsEnabled,
                (object)true,
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limitmode_2=1&limitmaxerror_2=1" });
        }

        [TestMethod]
        public async Task SetChannelProperty_VersionProperty_LimitsEnabled_FalseAsync()
        {
            var matrix = new[]
            {
                new int?[] {1, null, null, null},
                new int?[] {1, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.LimitsEnabled,
                (object)false,
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&limitmode_2=0&limitmaxerror_2=&limitmaxwarning_2=&limitminerror_2=&limitminwarning_2=&limiterrormsg_2=&limitwarningmsg_2=" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&limitmode_2=0&limitmaxerror_2=&limitmaxwarning_2=&limitminerror_2=&limitminwarning_2=&limiterrormsg_2=&limitwarningmsg_2=" });
        }

        [TestMethod]
        public async Task SetChannelProperty_VersionProperty_NormalPropertyAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.SpikeFilterMax,
                (object)100,
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001&spikemax_2=100&spikemode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[] { "id=1001,2001&spikemax_2=100&spikemode_2=1" });
        }

            #endregion

        [TestMethod]
        public async Task SetChannelProperty_ThreeValuesAsync()
        {
            var matrix = new[]
            {
                new int?[] {1,    null, 2,    1}, //1001 - 2
                new int?[] {1,    null, null, 4}, //2001 - 2
                new int?[] {5,    null, 8,    7}, //3001 - 4
                new int?[] {2,    3,    15,   6}, //4001 - 1
                new int?[] {null, 3,    20,   5}, //5001 - 1
                new int?[] {4,    3,    null, 8}  //6001 - 1
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            await SetChannelPropertyAsync(config, RequestVersion.v14_4, new[] { "id=1001,2001,3001,4001,5001,6001&limiterrormsg_2=test&limitmode_2=1" });
            await SetChannelPropertyAsync(config, RequestVersion.v18_1, new[]
            {
                "id=4001,5001,6001&limiterrormsg_2=test&limitmode_2=1&limitminerror_2=3",
                "id=1001,2001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=1",
                "id=3001&limiterrormsg_2=test&limitmode_2=1&limitmaxerror_2=5"
            });
        }

        [TestMethod]
        public async Task SetChannelProperty_SomeNullAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, 2, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            Func<RequestVersion, Task> action = async version => await SetChannelPropertyAsync(config, version, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });

            await action(RequestVersion.v14_4);

            var builder = new StringBuilder();
            builder.Append("Cannot set property 'ErrorLimitMessage' to value 'test' for Channel ID 2: ");
            builder.Append("Sensor ID 2001 does not have a limit value defined on it. ");
            builder.Append("Please set one of 'UpperErrorLimit', 'LowerErrorLimit', 'UpperWarningLimit' or 'LowerWarningLimit' first and then try again");

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await action(RequestVersion.v18_1), builder.ToString());
        }

        [TestMethod]
        public async Task SetChannelProperty_AllNullAsync()
        {
            var matrix = new[]
            {
                new int?[] {null, null, null, null},
                new int?[] {null, null, null, null}
            };

            var config = Tuple.Create(
                ChannelProperty.ErrorLimitMessage,
                (object)"test",
                matrix
            );

            Func<RequestVersion, Task> action = async version => await SetChannelPropertyAsync(config, version, new[] { "id=1001,2001&limiterrormsg_2=test&limitmode_2=1" });

            await action(RequestVersion.v14_4);

            var builder = new StringBuilder();
            builder.Append("Cannot set property 'ErrorLimitMessage' to value 'test' for Channel ID 2: ");
            builder.Append("Sensor IDs 1001 and 2001 do not have a limit value defined on them. ");
            builder.Append("Please set one of 'UpperErrorLimit', 'LowerErrorLimit', 'UpperWarningLimit' or 'LowerWarningLimit' first and then try again");

            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await action(RequestVersion.v18_1), builder.ToString());
        }

        [TestMethod]
        public async Task SetChannelProperty_VersionSpecific_ResolvesChannelsAsync()
        {
            var addresses = new[]
            {
                "api/table.xml?content=channels&columns=objid,name,lastvalue&count=*&id=1001",
                "controls/channeledit.htm?id=1001&channel=1",
                "editsettings?id=1001&limiterrormsg_1=hello&limitmode_1=1&limitmaxerror_1=100"
            };

            var property = ChannelProperty.ErrorLimitMessage;
            var val = "hello";

            var client = Initialize_Client(new AddressValidatorResponse(addresses.Select(a => $"https://prtg.example.com/{a}&username=username&passhash=12345678").ToArray(), true));
            SetVersion(client, RequestVersion.v18_1);

            await client.GetVersionClient(new object[] { property }).SetChannelPropertyAsync(new[] { 1001 }, 1, null, new[] { new ChannelParameter(property, val) });
        }

        private async Task SetChannelPropertyAsync(Tuple<ChannelProperty, object, int?[][]> config, RequestVersion version, string[] addresses)
        {
            var property = config.Item1;
            var val = config.Item2;
            var matrix = config.Item3;

            var channels = matrix.Select(CreateChannel).ToList();

            var client = Initialize_Client(new AddressValidatorResponse(addresses.Select(a => $"https://prtg.example.com/editsettings?{a}&username=username&passhash=12345678").ToArray(), true));
            SetVersion(client, version);

            await client.GetVersionClient(new object[] { property }).SetChannelPropertyAsync(channels.Select(c => c.SensorId).ToArray(), 2, channels, new[] { new ChannelParameter(property, val) });
        }

        #endregion
    }
}
