using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.CSharp
{
    [TestClass]
    public class LogTests : StreamableObjectTests<Log, MessageItem, MessageResponse>
    {
        [TestMethod]
        public void Log_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        public async Task Log_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [TestMethod]
        public void Log_AllFields_HaveValues() => Object_AllFields_HaveValues();

        protected override List<Log> GetObjects(PrtgClient client) => client.GetLogs();

        protected override Task<List<Log>> GetObjectsAsync(PrtgClient client) => client.GetLogsAsync();

        protected override IEnumerable<Log> Stream(PrtgClient client) => client.StreamLogs();

        public override MessageItem GetItem() => new MessageItem();

        protected override MessageResponse GetResponse(MessageItem[] items) => new MessageResponse(items);
    }
}
