using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.CSharp
{
    [TestClass]
    public class StateTests : BaseTest
    {
        private PrtgClient client;

        public StateTests()
        {
            client = Initialize_Client(new BasicResponse(string.Empty));
        }

        [TestMethod]
        public void AcknowledgeSensor_CanExecute() => client.AcknowledgeSensor(1001, 30, "Acknowledging sensor");

        [TestMethod]
        public async Task AcknowledgeSensor_CanExecuteAsync() => await client.AcknowledgeSensorAsync(1001, 30, "Acknowledging sensor");

        [TestMethod]
        public void PauseObject_CanExecute() => client.PauseObject(1001, 30, "Acknowledging sensor");

        [TestMethod]
        public async Task PauseObject_CanExecute_CanExecuteAsync() => await client.PauseObjectAsync(1001, 30, "Acknowledging sensor");

        [TestMethod]
        public void SimulateError_CanExecute() => client.SimulateError(1001);

        [TestMethod]
        public async Task SimulateError_CanExecuteAsync() => await client.SimulateErrorAsync(1001);
    }
}
