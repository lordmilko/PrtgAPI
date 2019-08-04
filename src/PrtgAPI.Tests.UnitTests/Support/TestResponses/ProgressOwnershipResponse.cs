namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class ProgressOwnershipResponse : MultiTypeResponse
    {
        int sensorCount;

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch(function)
            {
                case nameof(XmlFunction.TableData):
                    var content = GetContent(address);

                    if (content == Content.Sensors)
                    {
                        sensorCount++;

                        if (sensorCount == 1)
                            return base.GetResponse(ref address, function);
                        else
                            return new SensorResponse();
                    }

                    return base.GetResponse(ref address, function);
                default:
                    return base.GetResponse(ref address, function);
            }
        }
    }
}
