using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class TakeUnfilteredSensorsFromGroupNoRecurseScenario : TakeScenario
    {
        protected override IWebResponse GetResponse(string address, Content content)
        {
            switch (requestNum)
            {
                case 1: //Get a group
                    Assert.AreEqual(UnitRequest.Groups("count=1", UrlFlag.Columns), address);
                    return new GroupResponse(new GroupItem());

                case 2: //Get 2 sensors under the group "Windows Infrastructure"
                    Assert.AreEqual(UnitRequest.Sensors("count=2&filter_group=Windows+Infrastructure", UrlFlag.Columns), address);
                    return new SensorResponse(new SensorItem(name: "First"));

                    //todo: why didnt this start streaming?

                default:
                    throw UnknownRequest(address);
            }
        }
    }
}
