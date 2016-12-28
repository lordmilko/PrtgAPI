using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.Support;

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

        protected override List<Sensor> GetObjects(PrtgClient client) => client.GetSensors();

        protected override SensorItem GetItem() => new SensorItem();

        protected override SensorResponse GetResponse(SensorItem[] items) => new SensorResponse(items);
    }
}
