using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.Items;
using PrtgAPI.Tests.UnitTests.ObjectTests.Responses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class SensorTests : StreamableObjectTests<Sensor, SensorItem, SensorResponse>
    {
        [TestMethod]
        public void Sensor_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        public async Task Sensor_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [TestMethod]
        public void Sensor_CanStream_Ordered_FastestToSlowest() => Object_CanStream_Ordered_FastestToSlowest();

        [TestMethod]
        public void Sensor_GetObjectsOverloads_CanExecute()
        {
            Object_GetObjectsOverloads_CanExecute
            (
                (c1, c2, c3) => new List<Func<Property, object, object>> {c1.GetSensors, c2.GetSensorsAsync, c3.StreamSensors},
                (c1, c2, c3) => new List<Func<Property, FilterOperator, string, object>> {c1.GetSensors, c2.GetSensorsAsync, c3.StreamSensors},
                (c1, c2, c3) => new List<Func<SearchFilter[], object>> {c1.GetSensors, c2.GetSensorsAsync, c3.StreamSensors},
                (c1, c2, c3) =>
                {
                    Sensor_GetObjectsOverloads_SensorStatus_CanExecute(new List<Func<SensorStatus[], object>> {c1.GetSensors, c2.GetSensorsAsync, c3.StreamSensors});
                }
            );
        }

        private async void Sensor_GetObjectsOverloads_SensorStatus_CanExecute(List<Func<SensorStatus[], object>> sensorStatus)
        {
            var status = new[] {SensorStatus.Down, SensorStatus.DownAcknowledged};

            CheckResult<List<Sensor>>(sensorStatus[0](status));
            CheckResult<List<Sensor>>(await (Task<List<Sensor>>)sensorStatus[1](status));
            CheckResult<IEnumerable<Sensor>>(sensorStatus[2](status));
        }

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

        protected override Task<List<Sensor>> GetObjectsAsync(PrtgClient client) => client.GetSensorsAsync();

        protected override IEnumerable<Sensor> Stream(PrtgClient client) => client.StreamSensors();

        public override SensorItem GetItem() => new SensorItem();

        protected override SensorResponse GetResponse(SensorItem[] items) => new SensorResponse(items);
    }
}
