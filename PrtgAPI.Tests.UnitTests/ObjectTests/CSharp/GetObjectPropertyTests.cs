using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class GetObjectPropertyTests : BaseTest
    {
        [TestMethod]
        public void GetObjectProperty_AsObject() => GetObjectProperty(c => c.GetObjectProperty(1001, ObjectProperty.Name));

        [TestMethod]
        public async Task GetObjectPropertyAsync_AsObject() => await GetObjectPropertyAsync(async c => await c.GetObjectPropertyAsync(1001, ObjectProperty.Name));


        [TestMethod]
        public void GetObjectProperty_AsType() => GetObjectProperty(c => c.GetObjectProperty<string>(1001, ObjectProperty.Name));

        [TestMethod]
        public void GetObjectProperty_Nullable_AsUnderlying() => GetObjectProperty(c => c.GetObjectProperty<bool>(1001, ObjectProperty.InheritAccess), "True");

        [TestMethod]
        public void GetObjectProperty_Nullable_AsUnderlying_WithNull_Throws()
        {
            AssertEx.Throws<InvalidCastException>(
                () => GetObjectProperty(c => c.GetObjectProperty<bool>(1001, ObjectProperty.InheritInterval), "True"),
                "Cannot convert a value of type 'null' to type 'System.Boolean'"
            );
        }

        [TestMethod]
        public void GetObjectProperty_Nullable_AsNullable() => GetObjectProperty(c => c.GetObjectProperty<bool?>(1001, ObjectProperty.InheritAccess), "True");

        [TestMethod]
        public void GetObjectProperty_Nullable_AsNullable_WithNull() => GetObjectProperty(c => c.GetObjectProperty<bool?>(1001, ObjectProperty.InheritInterval), null);

        [TestMethod]
        public async Task GetObjectPropertyAsync_AsType() => await GetObjectPropertyAsync(async c => await c.GetObjectPropertyAsync<string>(1001, ObjectProperty.Name));

        [TestMethod]
        public void GetObjectPropertiesRaw() => GetObjectProperty(c => c.GetObjectPropertiesRaw(1001, ObjectType.Sensor)["name"], "Server CPU Usage");

        [TestMethod]
        public async Task GetObjectPropertiesRawAsync() => await GetObjectPropertyAsync(async c => (await c.GetObjectPropertiesRawAsync(1001, ObjectType.Sensor))["name"], "Server CPU Usage");

        private void GetObjectProperty<T>(Func<PrtgClient, T> getProperty, string expected = "testName")
        {
            var client = Initialize_Client(new MultiTypeResponse());

            T val = getProperty(client);

            Assert.AreEqual(expected, val?.ToString());
        }

        private async Task GetObjectPropertyAsync<T>(Func<PrtgClient, Task<T>> getProperty, string expected = "testName")
        {
            var client = Initialize_Client(new MultiTypeResponse());

            T val = await getProperty(client);

            Assert.AreEqual(expected, val);
        }
    }
}
