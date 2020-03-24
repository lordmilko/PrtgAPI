using System;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class FaultySensorTargetResponse : MultiTypeResponse
    {
        private Scenario scenario;

        public enum Scenario
        {
            Credentials,
            Other,
            EnhancedError
        }

        public FaultySensorTargetResponse(Scenario scenario)
        {
            this.scenario = scenario;
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(CommandFunction.AddSensor2):
                    address = "http://prtg.example.com/controls/addsensor3.htm?id=9999&tmpid=2";
                    return new BasicResponse(string.Empty);

                case nameof(JsonFunction.GetAddSensorProgress):
                    var faultyUrl = GetFaultyUrl();
                    return new BasicResponse($"{{\"progress\":\"100\",\"targeturl\":\"{faultyUrl}\"");

                case nameof(JsonFunction.SensorTypes):
                    return new SensorTypeResponse();

                case nameof(HtmlFunction.AddSensorFailed):
                    if (scenario == Scenario.EnhancedError)
                        return new BasicResponse("<span><div class=\"errormessage\">Enhanced error</div></span>");
                    else
                        return new BasicResponse(string.Empty);

                default:
                    throw GetUnknownFunctionException(function);
            }
        }

        private string GetFaultyUrl()
        {
            var str = string.Empty;

            switch (scenario)
            {
                case Scenario.Credentials:
                    str = "Incomplete%20connection%20settings%20or%20credentials.";
                    break;
                case Scenario.Other:
                    str = "Other%20error";
                    break;
                case Scenario.EnhancedError:
                    return $"addsensorfailed.htm?id=2808&sensorkind=WMI%20Service&manuallink=%2Fhelp%2Fwmi_service_sensor.htm";
                default:
                    throw new NotImplementedException($"Unknown scenario '{scenario}' passed to {typeof(FaultySensorTargetResponse).Name}");
            }

            return $"addsensorfailed.htm?errormsg={str}&id=2808&sensorkind=WMI%20Service&manuallink=%2Fhelp%2Fwmi_service_sensor.htm";
        }
    }
}
