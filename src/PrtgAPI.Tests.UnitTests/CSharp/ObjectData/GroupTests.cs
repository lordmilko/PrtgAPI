using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    [TestClass]
    public class GroupTests : QueryableObjectTests<Group, GroupItem, GroupResponse>
    {
        [UnitTest]
        [TestMethod]
        public void Group_CanDeserialize() => Object_CanDeserialize();

        [UnitTest]
        [TestMethod]
        public async Task Group_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [TestMethod]
        [UnitTest(TestCategory.SkipCoverage)]
#if MSTEST2
        [DoNotParallelize]
#endif
        public void Group_CanStream_Ordered_FastestToSlowest() => Object_CanStream_Ordered_FastestToSlowest();

        [UnitTest]
        [TestMethod]
        public void Group_GetObjectsOverloads_CanExecute() => Object_GetObjectsOverloads_CanExecute(
            (c1, c2) => new List<Func<int, object>>                              { c1.GetGroup, c2.GetGroupAsync },
            (c1, c2) => new List<Func<Property, object, object>>                 { c1.GetGroups, c2.GetGroupsAsync },
            (c1, c2) => new List<Func<Property, FilterOperator, string, object>> { c1.GetGroups, c2.GetGroupsAsync },
            (c1, c2) => new List<Func<SearchFilter[], object>>                   { c1.GetGroups, c2.GetGroupsAsync }
        );

        [UnitTest]
        [TestMethod]
        public void Group_GetObjectsOverloads_Stream_CanExecute() => Object_GetObjectsOverloads_Stream_CanExecute(
            client => client.StreamGroups,
            client => client.StreamGroups,
            client => client.StreamGroups,
            client => client.StreamGroups
        );

        [UnitTest]
        [TestMethod]
        public void Group_StreamSerially() => Object_SerialStreamObjects(
            c => c.StreamGroups,
            c => c.StreamGroups,
            new GroupParameters()
        );

        [UnitTest]
        [TestMethod]
        public void Group_GetObjectsOverloads_Query_CanExecute() => Object_GetObjectsOverloads_Query_CanExecute(
            client => client.QueryGroups,
            client => client.QueryGroups,
            client => client.QueryGroups,
            client => client.QueryGroups
        );

        [UnitTest]
        [TestMethod]
        public void Group_GetGroup_Throws_WhenNoObjectReturned() => Object_GetSingle_Throws_WhenNoObjectReturned(c => c.GetGroup(1001));

        [UnitTest]
        [TestMethod]
        public void Group_GetGroup_Throws_WhenMultipleObjectsReturned() => Object_GetSingle_Throws_WhenMultipleObjectsReturned(c => c.GetGroup(1001));

        [UnitTest]
        [TestMethod]
        public void Group_AllFields_HaveValues() => Object_AllFields_HaveValues();

        [UnitTest]
        [TestMethod]
        public void Group_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var group = client.GetGroup(1001);

            AssertEx.AllPropertiesRetrieveValues(group);
        }

        [UnitTest]
        [TestMethod]
        public async Task Group_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var group = await client.GetGroupAsync(1001);

            AssertEx.AllPropertiesRetrieveValues(group);
        }

        protected override List<Group> GetObjects(PrtgClient client) => client.GetGroups();

        protected override Task<List<Group>> GetObjectsAsync(PrtgClient client) => client.GetGroupsAsync();

        protected override IEnumerable<Group> Stream(PrtgClient client) => client.StreamGroups();

        public override GroupItem GetItem() => new GroupItem();

        protected override GroupResponse GetResponse(GroupItem[] items) => new GroupResponse(items);
    }
}
