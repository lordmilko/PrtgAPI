﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class SlowPathProbesOnlyScenario : TreeScenario
    {
        protected override IWebResponse GetResponse(string address)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Objects("filter_objid=1001"), address);
                    return new ProbeResponse(new ProbeItem(objid: "1001", groupnum: "0", groupnumRaw: "0", devicenum: "0", devicenumRaw: "0"));

                case 2:
                    Assert.AreEqual(UnitRequest.Probes("filter_objid=1001&filter_type=probenode"), address);
                    return new ProbeResponse(new ProbeItem(objid: "1001", groupnum: "0", groupnumRaw: "0", devicenum: "0", devicenumRaw: "0"));

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
