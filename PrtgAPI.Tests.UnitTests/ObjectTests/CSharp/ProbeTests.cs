using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestItems;
using PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses;

namespace PrtgAPI.Tests.UnitTests.ObjectTests
{
    [TestClass]
    public class ProbeTests : StreamableObjectTests<Probe, ProbeItem, ProbeResponse>
    {
        [TestMethod]
        public void Probe_CanDeserialize() => Object_CanDeserialize();

        [TestMethod]
        public async Task Probe_CanDeserializeAsync() => await Object_CanDeserializeAsync();

        [TestMethod]
        [TestCategory("SlowCoverage")]
        public void Probe_CanStream_Ordered_FastestToSlowest() => Object_CanStream_Ordered_FastestToSlowest();

        [TestMethod]
        public void Probe_GetObjectsOverloads_CanExecute() => Object_GetObjectsOverloads_CanExecute(
            (c1, c2) => new List<Func<Property, object, object>> { c1.GetProbes, c2.GetProbesAsync },
            (c1, c2) => new List<Func<Property, FilterOperator, string, object>> { c1.GetProbes, c2.GetProbesAsync },
            (c1, c2) => new List<Func<SearchFilter[], object>> { c1.GetProbes, c2.GetProbesAsync }
        );

        [TestMethod]
        [TestCategory("SlowCoverage")]
        public void Probe_GetObjectsOverloads_Stream_CanExecute() => Object_GetObjectsOverloads_Stream_CanExecute(
            client => client.StreamProbes,
            client => client.StreamProbes,
            client => client.StreamProbes
        );

        [TestMethod]
        public void Probe_AllFields_HaveValues()
        {
            Object_AllFields_HaveValues(prop =>
            {
                if (prop.Name == nameof(SensorOrDeviceOrGroupOrProbe.ParentId)) //As all probes are under the group with ID 0, we cannot verify whether the value was actually set
                    return true;

                return false;
            });
        }

        protected override List<Probe> GetObjects(PrtgClient client) => client.GetProbes();

        protected override Task<List<Probe>> GetObjectsAsync(PrtgClient client) => client.GetProbesAsync();

        protected override IEnumerable<Probe> Stream(PrtgClient client) => client.StreamProbes();

        public override ProbeItem GetItem() => new ProbeItem();

        protected override ProbeResponse GetResponse(ProbeItem[] items) => new ProbeResponse(items);
    }
}
