using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class SensorQueryTargetValidatorResponse : AddressValidatorResponse
    {
#pragma warning disable 618
        public SensorQueryTargetValidatorResponse(object str, bool exactMatch) : base(str, exactMatch)
        {
        }

        public SensorQueryTargetValidatorResponse(object[] str) : base(str)
        {
        }
#pragma warning restore 618

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            ValidateAddress(address);

            switch (function)
            {
                case nameof(JsonFunction.GetStatus):
                    return new ServerStatusResponse(new ServerStatusItem());
                case nameof(JsonFunction.SensorTypes):
                    return new SensorTypeResponse();
                case nameof(CommandFunction.AddSensor2):
                    address = "http://prtg.example.com/controls/addsensor3.htm?id=9999&tmpid=2";
                    return new BasicResponse(string.Empty);
                case nameof(JsonFunction.GetAddSensorProgress):
                    return new BasicResponse($"{{\"progress\":\"100\",\"targeturl\":\" /addsensor4.htm?id=4251&tmpid=119\"}}");
                case nameof(HtmlFunction.AddSensor4):
                    return new ExeFileTargetResponse(); //For the purposes of this test any response will do
                case nameof(CommandFunction.AddSensor5):
                    return new BasicResponse(string.Empty);
                default:
                    throw GetUnknownFunctionException(function);
            }
        }
    }
}
