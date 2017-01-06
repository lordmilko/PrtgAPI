using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.Items;
using PrtgAPI.Tests.UnitTests.ObjectTests.Responses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class SensorTests : ObjectTests<Sensor, SensorItem, SensorResponse>
    {
        [TestMethod]
        public void Sensor_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        public void Sensor_AllFields_HaveValues()
        {
            Object_AllFields_HaveValues(prop =>
            {
                if (prop.Name == nameof(Sensor.BaseType)) //As Sensor is the default value of BaseType, we cannot verify whether the value was actually set
                    return true;

                return false;
            });
        }

        [TestMethod]
        public void Sensor_Async_Ordered_FastestToSlowest()
        {
            var count = 2000;
            var perPage = 500;
            var pages = count/perPage;

            var client = Initialize_Client_WithItems(Enumerable.Range(0, count).Select(i => GetItem()).ToArray());
            var results = client.GetSensorsAsync().Select(i => i.Id).ToList();
            Assert.IsTrue(results.Count == count);

            for (int pageNum = pages; pageNum > 0; pageNum--)
            {
                var r = results.Skip((pages - pageNum)*perPage).Take(perPage).ToList();
                Assert.IsTrue(r.TrueForAll(item => item == pageNum));
            }
        }

        protected override List<Sensor> GetObjects(PrtgClient client) => client.GetSensors();

        protected override SensorItem GetItem() => new SensorItem();

        protected override SensorResponse GetResponse(SensorItem[] items) => new SensorResponse(items);
    }
}
