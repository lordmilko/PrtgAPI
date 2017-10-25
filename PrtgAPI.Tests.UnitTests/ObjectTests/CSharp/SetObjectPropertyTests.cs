using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.CSharp
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
            SetObjectProperty(ObjectProperty.DBPort, "8080.0", "8080");
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
        public async Task SetObjectProperty_CanExecuteAsync()
        {
            var client = Initialize_Client(new SetObjectPropertyResponse<ObjectProperty>(ObjectProperty.DBPort, "8080"));

            await client.SetObjectPropertyAsync(1001, ObjectProperty.DBPort, 8080);
        }

        [TestMethod]
        public void SetObjectProperty_CanSetLocation()
        {
            SetObjectProperty(ObjectProperty.Location, "23 Fleet Street", "23 Fleet St, Boston, MA 02113, USA");
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
