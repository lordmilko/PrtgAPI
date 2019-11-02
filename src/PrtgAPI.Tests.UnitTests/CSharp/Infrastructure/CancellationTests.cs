using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.Infrastructure
{
    [TestClass]
    public class CancellationTests
    {
        [UnitTest]
        [TestMethod]
        public void Cancellation_CancelsSynchronous()
        {
            var client = BaseTest.Initialize_Client(new MultiTypeResponse());

            var cts = new CancellationTokenSource();
            cts.Cancel();

            AssertEx.Throws<TaskCanceledException>(
                () => client.GetSensors(new SensorParameters(), cts.Token),
                "A task was canceled."
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Cancellation_CancelsAsynchronous()
        {
            var client = BaseTest.Initialize_Client(new MultiTypeResponse());

            var cts = new CancellationTokenSource();
            cts.Cancel();

            await AssertEx.ThrowsAsync<TaskCanceledException>(
                async () => await client.GetSensorsAsync(new SensorParameters(), cts.Token),
                "A task was canceled."
            );
        }

        [UnitTest]
        [TestMethod]
        public async Task Cancellation_CancelsWaitOneAsync()
        {
            var cts = new CancellationTokenSource();

            var token = cts.Token;

            var task = token.WaitHandle.WaitOneAsync(10000, token);

            cts.Cancel();

            await AssertEx.ThrowsAsync<TaskCanceledException>(async () => await task, "A task was canceled.");
        }

        [UnitTest]
        [TestMethod]
        public void Cancellation_CancelsLazy()
        {
            var client = BaseTest.Initialize_Client(new NotificationActionResponse(new NotificationActionItem())
            {
                HasSchedule = new[] { 300 }
            });

            var cts = new CancellationTokenSource();

            var action = client.GetNotificationActionsInternal(new NotificationActionParameters(new SearchFilter(Property.Id, 300)), cts.Token).Single();

            cts.Cancel();

            AssertEx.Throws<TaskCanceledException>(() =>
            {
                var schedule = action.Schedule;
            }, "A task was canceled.");
        }

        [UnitTest]
        [TestMethod]
        public async Task Cancellation_CancelsLazyAsync()
        {
            var client = BaseTest.Initialize_Client(new NotificationActionResponse(new NotificationActionItem())
            {
                HasSchedule = new[] { 300 }
            });

            var cts = new CancellationTokenSource();

            var action = await client.GetNotificationActionAsync(300, cts.Token);

            cts.Cancel();

            //No exception because we weren't actually lazy
            var schedule = action.Schedule;
        }
    }
}
