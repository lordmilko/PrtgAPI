using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.CSharp
{
    [TestClass]
    public class ScheduleTests : ObjectTests<Schedule, ScheduleItem, ScheduleResponse>
    {
        [TestMethod]
        public void Schedule_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        public async Task Schedule_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [TestMethod]
        public void Schedule_AllFields_HaveValues() => Object_AllFields_HaveValues(p =>
        {
            if (p.Name == "Tags")
                return true;

            return false;
        });

        protected override List<Schedule> GetObjects(PrtgClient client) => client.GetSchedules();

        protected override async Task<List<Schedule>> GetObjectsAsync(PrtgClient client) => await client.GetSchedulesAsync();

        public override ScheduleItem GetItem() => new ScheduleItem();

        protected override ScheduleResponse GetResponse(ScheduleItem[] items) => new ScheduleResponse(items);
    }
}
