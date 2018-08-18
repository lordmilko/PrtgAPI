using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class ModificationHistoryTests : ObjectTests<ModificationEvent, ModificationHistoryItem, ModificationHistoryResponse>
    {
        [TestMethod]
        public void ModificationHistory_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        public async Task ModificationHistory_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [TestMethod]
        public void ModificationHistory_AllFields_HaveValues() => Object_AllFields_HaveValues();

        protected override List<ModificationEvent> GetObjects(PrtgClient client) => client.GetModificationHistory(1234);

        protected override async Task<List<ModificationEvent>> GetObjectsAsync(PrtgClient client) => await client.GetModificationHistoryAsync(1234);

        public override ModificationHistoryItem GetItem() => new ModificationHistoryItem();

        protected override ModificationHistoryResponse GetResponse(ModificationHistoryItem[] items) => new ModificationHistoryResponse(items);
    }
}
