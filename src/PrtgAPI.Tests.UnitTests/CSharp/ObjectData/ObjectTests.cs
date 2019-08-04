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
    public class ObjectTests : StreamableObjectTests<PrtgObject, BaseItem, ObjectResponse>
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void PrtgObject_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PrtgObject_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [TestMethod]
        [TestCategory("SlowCoverage")]
        [TestCategory("UnitTest")]
#if MSTEST2
        [DoNotParallelize]
#endif
        public void PrtgObject_CanStream_Ordered_FastestToSlowest() => Object_CanStream_Ordered_FastestToSlowest();

        [TestMethod]
        [TestCategory("UnitTest")]
        public void PrtgObject_GetObjectsOverloads_CanExecute() => Object_GetObjectsOverloads_CanExecute(
            (c1, c2) => new List<Func<int, object>> { i => c1.GetObject(i), i => c2.GetObjectAsync(i) },
            (c1, c2) => new List<Func<Property, object, object>> { c1.GetObjects, c2.GetObjectsAsync },
            (c1, c2) => new List<Func<Property, FilterOperator, string, object>> { c1.GetObjects, c2.GetObjectsAsync },
            (c1, c2) => new List<Func<SearchFilter[], object>> { c1.GetObjects, c2.GetObjectsAsync }
        );

        [TestMethod]
        [TestCategory("UnitTest")]
        public void PrtgObject_GetObjectsOverloads_Stream_CanExecute() => Object_GetObjectsOverloads_Stream_CanExecute(
            client => client.StreamObjects,
            client => client.StreamObjects,
            client => client.StreamObjects,
            client => client.StreamObjects
        );

        [TestMethod]
        [TestCategory("UnitTest")]
        public void PrtgObject_StreamSerially() => Object_SerialStreamObjects(
            c => c.StreamObjects,
            c => c.StreamObjects,
            new PrtgObjectParameters()
        );

        [TestMethod]
        [TestCategory("UnitTest")]
        public void PrtgObject_ResolvesAllTypes()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            Assert.IsTrue(client.GetObject(4000, true) is Sensor);
            Assert.IsTrue(client.GetObject(3000, true) is Device);
            Assert.IsTrue(client.GetObject(2000, true) is Group);
            Assert.IsTrue(client.GetObject(1000, true) is Probe);
            Assert.IsTrue(client.GetObject(600, true) is Schedule);
            Assert.IsTrue(client.GetObject(300, true) is NotificationAction);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PrtgObject_ResolvesAllTypesAsync()
        {
            var client = Initialize_Client(new MultiTypeResponse());

            Assert.IsTrue(await client.GetObjectAsync(4000, true) is Sensor);
            Assert.IsTrue(await client.GetObjectAsync(3000, true) is Device);
            Assert.IsTrue(await client.GetObjectAsync(2000, true) is Group);
            Assert.IsTrue(await client.GetObjectAsync(1000, true) is Probe);
            Assert.IsTrue(await client.GetObjectAsync(600, true) is Schedule);
            Assert.IsTrue(await client.GetObjectAsync(300, true) is NotificationAction);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void PrtgObject_GetPrtgObject_Throws_WhenNoObjectReturned() => Object_GetSingle_Throws_WhenNoObjectReturned(c => c.GetObject(1001));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void PrtgObject_GetPrtgObject_Throws_WhenMultipleObjectsReturned() => Object_GetSingle_Throws_WhenMultipleObjectsReturned(c => c.GetObject(1001));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void PrtgObject_AllFields_HaveValues() => Object_AllFields_HaveValues();

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Object_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var @object = client.GetObject(1001);

            AssertEx.AllPropertiesRetrieveValues(@object);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Object_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var @object = await client.GetObjectAsync(1001);

            AssertEx.AllPropertiesRetrieveValues(@object);
        }

        protected override List<PrtgObject> GetObjects(PrtgClient client) => client.GetObjects();

        protected override async Task<List<PrtgObject>> GetObjectsAsync(PrtgClient client) => await client.GetObjectsAsync();

        protected override IEnumerable<PrtgObject> Stream(PrtgClient client) => client.StreamObjects();

        public override BaseItem GetItem() => new SensorItem();

        protected override ObjectResponse GetResponse(BaseItem[] items) => new ObjectResponse(items);
    }
}
