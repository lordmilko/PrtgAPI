using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class SetObjectPropertyTests : BaseTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SetObjectProperty_Enum_With_Int()
        {
            SetObjectProperty(ObjectProperty.IntervalErrorMode, 1);
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
            try
            {
                SetObjectProperty(ObjectProperty.DBPort, Status.Up, "8");
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Expected type: 'System.Int32'. Actual type: 'PrtgAPI.Status'"))
                    throw;
            }
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
            try
            {
                SetObjectProperty(ObjectProperty.DBPort, true, "1");
                Assert.Fail("Expected an exception to be thrown");
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("Expected type: 'System.Int32'. Actual type: 'System.Boolean'"))
                    throw;
            }
        }

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
        [ExpectedException(typeof(PrtgRequestException))]
        public void Location_FailsToResolve()
        {
            var client = Initialize_Client(new LocationUnresolvedResponse());

            Location.Resolve(client, "something");
        }

        [TestMethod]
        [ExpectedException(typeof(PrtgRequestException))]
        public async Task Location_FailsToResolveAsync()
        {
            var client = Initialize_Client(new LocationUnresolvedResponse());

            await Location.ResolveAsync(client, "something");
        }

        [TestMethod]
        public void Location_ProviderUnavailable()
        {
            var client = Initialize_Client(new LocationUnresolvedResponse(true));

            try
            {
                Location.Resolve(client, "something");
                Assert.Fail("Expected an exception to be thrown");
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("the PRTG map provider is not currently available"))
                    throw;
            }
        }

        [TestMethod]
        public async Task SetObjectPropertyRaw_CanExecuteAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            await client.SetObjectPropertyRawAsync(1001, "name_", "testName");
        }

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
    }
}
