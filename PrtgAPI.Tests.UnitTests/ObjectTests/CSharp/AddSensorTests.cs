using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.CSharp
{
    [TestClass]
    public class AddSensorTests : BaseTest
    {
        [TestMethod]
        public void AddSensor_CanExecute()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            var parameters = new ExeXmlSensorParameters("test.ps1");

            client.AddSensor(1001, parameters);
        }

        [TestMethod]
        public async Task AddSensor_CanExecuteAsync()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            var parameters = new ExeXmlSensorParameters("test.ps1");

            await client.AddSensorAsync(1001, parameters);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddSensor_Throws_WhenMissingRequiredValue()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            var parameters = new ExeXmlSensorParameters("test.ps1")
            {
                ExeName = null
            };

            client.AddSensor(1001, parameters);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddSensor_Throws_MissingSensorName()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            client.AddSensor(1001, new ExeXmlSensorParameters("test.ps1", null));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddSensor_Throws_MissingSensorType()
        {
            var client = Initialize_Client(new BasicResponse(string.Empty));

            client.AddSensor(1001, new RawSensorParameters("sensorName", null));
        }
    }
}
