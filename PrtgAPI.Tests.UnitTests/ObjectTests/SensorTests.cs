using System;
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
        public void Sensor_Stream_Ordered_FastestToSlowest()
        {
            var count = 2000;
            var perPage = 500;
            var pages = count/perPage;

            //bug: the issue is our sensorresponse has no way of knowing whether to do a normal response or do a streaming response

            var client = Initialize_Client_WithItems(Enumerable.Range(0, count).Select(i => GetItem()).ToArray());
            var results = client.StreamSensors().Select(i => i.Id).ToList();
            Assert.IsTrue(results.Count == count, $"Expected {count} results but got {results.Count} instead.");

            for (int pageNum = pages; pageNum > 0; pageNum--)
            {
                var r = results.Skip((pages - pageNum)*perPage).Take(perPage).ToList();
                Assert.IsTrue(r.TrueForAll(item => item == pageNum));
            }
        }

        /*class Form1 : Form
        {
            public Form1(PrtgClient client)
            {
                var context = SynchronizationContext.Current;
                var response = client.StreamSensors(); //this doesnt deadlock; we're probably not doing it right
                foreach (var sensor in response)
                    Debug.WriteLine(sensor.Name);
            }
        }*/

/*public void FormTest()
{
    throw new NotImplementedException();
    var form = new Form1(Initialize_Client_WithItems(GetItem()));

    form.Show();
}*/

        //move to some objecttests class not sure what to call it yet need to reorganize objecttests, baseobjecttests

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Object_SetProperty_Enum_With_Int()
        {
            Object_SetProperty(ObjectProperty.ErrorIntervalDown, 1);
        }

        [TestMethod]
        public void Object_SetProperty_Enum_With_Enum()
        {
            Object_SetProperty(ObjectProperty.ErrorIntervalDown, ErrorIntervalDown.DownImmediately, ((int)ErrorIntervalDown.DownImmediately).ToString());
        }

        private void Object_SetProperty(ObjectProperty property, object value, string serializedValue = null)
        {
            if (serializedValue == null)
                serializedValue = value.ToString();

            var client = Initialize_Client(new SetObjectPropertyResponse(property, serializedValue));
            client.SetObjectProperty(1, property, value);
        }

        [TestMethod]
        public void Object_SetProperty_Bool_With_Bool()
        {
            Object_SetProperty(ObjectProperty.InheritScanningInterval, false, "0");
        }

        public void Object_SetProperty_Bool_With_Int()
        {
            
        }

        public void Object_SetProperty_Int_With_Enum()
        {
            
        }

        public void Object_SetProperty_Int_With_Int()
        {
            
        }

        public void Object_SetProperty_Int_With_Bool()
        {
            
        }

        protected override List<Sensor> GetObjects(PrtgClient client) => client.GetSensors();

        public override SensorItem GetItem() => new SensorItem();

        protected override SensorResponse GetResponse(SensorItem[] items) => new SensorResponse(items);
    }
}
