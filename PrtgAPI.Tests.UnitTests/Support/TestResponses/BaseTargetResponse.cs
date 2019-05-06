
namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public abstract class BaseTargetResponse : MultiTypeResponse
    {
        private int progressCount;

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(CommandFunction.AddSensor2):
                    address = "http://prtg.example.com/controls/addsensor3.htm?id=9999&tmpid=2";
                    return new BasicResponse(string.Empty);

                case nameof(JsonFunction.GetAddSensorProgress):

                    progressCount++;

                    var progress = progressCount == 1 ? 50 : 100;

                    return new BasicResponse($"{{\"progress\":\"{progress}\",\"targeturl\":\"/addsensor4.htm?id=4251&tmpid=119\"}}");

                case nameof(HtmlFunction.AddSensor4):
                    return new BasicResponse(GetResponseText());

                case nameof(HtmlFunction.ObjectData):
                    return new BasicResponse(GetResponseText());

                case nameof(JsonFunction.SensorTypes):
                    return new SensorTypeResponse();

                default:
                    throw GetUnknownFunctionException(function);
            }
        }

        protected abstract string GetResponseText();
    }
}