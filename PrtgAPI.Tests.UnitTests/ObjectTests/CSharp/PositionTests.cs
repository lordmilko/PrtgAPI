using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.CSharp
{
    [TestClass]
    public class PositionTests : BaseTest
    {
        [TestMethod]
        public void Position_Absolute_CanExecute()
        {
            var client = GetClient();

            client.SetPosition(GetSensor(), 1);
        }

        [TestMethod]
        public async Task Position_Absolute_CanExecuteAsync()
        {
            var client = GetClient();

            await client.SetPositionAsync(GetSensor(), 1);
        }

        [TestMethod]
        public void Position_Relative_CanExecute()
        {
            var client = GetClient();

            client.SetPosition(1001, Position.Down);
        }

        [TestMethod]
        public async Task Position_Relative_CanExecuteAsync()
        {
            var client = GetClient();

            await client.SetPositionAsync(1001, Position.Down);
        }

        private PrtgClient GetClient()
        {
            return Initialize_Client(new BasicResponse(string.Empty));
        }

        private Sensor GetSensor()
        {
            return Initialize_Client(new SensorResponse(new SensorItem())).GetSensors().First();
        }
    }
}
