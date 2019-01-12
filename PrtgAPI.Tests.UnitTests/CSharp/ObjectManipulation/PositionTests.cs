using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectManipulation
{
    [TestClass]
    public class PositionTests : BaseTest
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Position_Absolute_CanExecute() =>
            Execute(c => c.SetPosition(GetSensor(), 1), "api/setposition.htm?id=2203&newpos=9");

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Position_Absolute_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.SetPositionAsync(GetSensor(), 1), "api/setposition.htm?id=2203&newpos=9");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Position_Relative_CanExecute() =>
            Execute(c => c.SetPosition(1001, Position.Down), "api/setposition.htm?id=1001&newpos=down");

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Position_Relative_CanExecuteAsync() =>
            await ExecuteAsync(async c => await c.SetPositionAsync(1001, Position.Down), "api/setposition.htm?id=1001&newpos=down");

        private Sensor GetSensor()
        {
            return Initialize_Client(new SensorResponse(new SensorItem())).GetSensors().First();
        }
    }
}
