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
    public class ProbeTests : QueryableObjectTests<Probe, ProbeItem, ProbeResponse>
    {
        protected override SearchFilter[] TestFilters { get; } = {
            new SearchFilter(Property.ParentId, 0),
            new SearchFilter(Property.Probe, FilterOperator.Contains, "contoso")
        };

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Probe_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Probe_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [TestMethod]
        [TestCategory("SlowCoverage")]
        [TestCategory("UnitTest")]
        public void Probe_CanStream_Ordered_FastestToSlowest() => Object_CanStream_Ordered_FastestToSlowest();

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Probe_GetObjectsOverloads_CanExecute() => Object_GetObjectsOverloads_CanExecute(
            (c1, c2) => new List<Func<int, object>>                              { c1.GetProbe, c2.GetProbeAsync },
            (c1, c2) => new List<Func<Property, object, object>>                 { c1.GetProbes, c2.GetProbesAsync },
            (c1, c2) => new List<Func<Property, FilterOperator, string, object>> { c1.GetProbes, c2.GetProbesAsync },
            (c1, c2) => new List<Func<SearchFilter[], object>>                   { c1.GetProbes, c2.GetProbesAsync }
        );

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Probe_GetObjectsOverloads_Stream_CanExecute() => Object_GetObjectsOverloads_Stream_CanExecute(
            client => client.StreamProbes,
            client => client.StreamProbes,
            client => client.StreamProbes,
            client => client.StreamProbes
        );

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Probe_StreamSerially() => Object_SerialStreamObjects(
            c => c.StreamProbes,
            c => c.StreamProbes,
            new ProbeParameters()
        );

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Probe_GetObjectsOverloads_Query_CanExecute() => Object_GetObjectsOverloads_Query_CanExecute(
            client => client.QueryProbes,
            client => client.QueryProbes,
            client => client.QueryProbes,
            client => client.QueryProbes
        );

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Probe_GetProbe_Throws_WhenNoObjectReturned() => Object_GetSingle_Throws_WhenNoObjectReturned(c => c.GetProbe(1001));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Probe_GetProbe_Throws_WhenMultipleObjectsReturned() => Object_GetSingle_Throws_WhenMultipleObjectsReturned(c => c.GetProbe(1001));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Probe_AllFields_HaveValues()
        {
            Object_AllFields_HaveValues(prop =>
            {
                if (prop.Name == nameof(SensorOrDeviceOrGroupOrProbe.ParentId)) //As all probes are under the group with ID 0, we cannot verify whether the value was actually set
                    return true;

                return false;
            });
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Probe_ReadOnly()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var probe = client.GetProbe(1001);

            AssertEx.AllPropertiesRetrieveValues(probe);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Probe_ReadOnlyAsync()
        {
            var client = Initialize_ReadOnlyClient(GetResponse(new[] { GetItem() }));

            var probe = await client.GetProbeAsync(1001);

            AssertEx.AllPropertiesRetrieveValues(probe);
        }

        protected override List<Probe> GetObjects(PrtgClient client) => client.GetProbes();

        protected override Task<List<Probe>> GetObjectsAsync(PrtgClient client) => client.GetProbesAsync();

        protected override IEnumerable<Probe> Stream(PrtgClient client) => client.StreamProbes();

        public override ProbeItem GetItem() => new ProbeItem();

        protected override ProbeResponse GetResponse(ProbeItem[] items) => new ProbeResponse(items);
    }
}
