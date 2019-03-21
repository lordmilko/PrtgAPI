using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeUnfilteredSensorsScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1:
                    Assert.AreEqual(UnitRequest.Sensors("count=2", UrlFlag.Columns), address);
                    return new SensorResponse(new SensorItem(), new SensorItem(), new SensorItem());

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}