using System;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses
{
    public class RecursiveRequestResponse : MultiTypeResponse
    {
        private GroupScenario scenario;

        public RecursiveRequestResponse(RecursiveRequestScenario scenario)
        {
            switch (scenario)
            {
            #region Sensor
                case RecursiveRequestScenario.SensorUniqueGroup:
                    this.scenario = new SensorUniqueGroupScenario();
                    break;
                case RecursiveRequestScenario.SensorDuplicateGroup:
                    this.scenario = new SensorDuplicateGroupScenario();
                    break;
                case RecursiveRequestScenario.SensorUniqueChildGroup:
                    this.scenario = new SensorUniqueChildGroupScenario();
                    break;
                case RecursiveRequestScenario.SensorDuplicateChildGroup:
                    this.scenario = new SensorDuplicateChildGroupScenario();
                    break;
                case RecursiveRequestScenario.SensorNoRecurse:
                    this.scenario = new SensorNoRecurseScenario();
                    break;
                #endregion
                #region Device
                case RecursiveRequestScenario.DeviceUniqueGroup:
                    this.scenario = new DeviceUniqueGroupScenario();
                    break;
                case RecursiveRequestScenario.DeviceDuplicateGroup:
                    this.scenario = new DeviceDuplicateGroupScenario();
                    break;
                case RecursiveRequestScenario.DeviceUniqueChildGroup:
                    this.scenario = new DeviceUniqueChildGroupScenario();
                    break;
                case RecursiveRequestScenario.DeviceDuplicateChildGroup:
                    this.scenario = new DeviceDuplicateChildGroupScenario();
                    break;
                case RecursiveRequestScenario.DeviceNoRecurse:
                    this.scenario = new DeviceNoRecurseScenario();
                    break;
                #endregion
                #region Group
                case RecursiveRequestScenario.GroupUniqueGroup:
                    this.scenario = new GroupUniqueGroupScenario();
                    break;
                case RecursiveRequestScenario.GroupDuplicateGroup:
                    this.scenario = new GroupDuplicateGroupScenario();
                    break;
                case RecursiveRequestScenario.GroupUniqueChildGroup:
                    this.scenario = new GroupUniqueChildGroupScenario();
                    break;
                case RecursiveRequestScenario.GroupDuplicateChildGroup:
                    this.scenario = new GroupDuplicateChildGroupScenario();
                    break;
                case RecursiveRequestScenario.GroupNoRecurse:
                    this.scenario = new GroupNoRecurseScenario();
                    break;
                case RecursiveRequestScenario.GroupDeepNesting:
                    this.scenario = new GroupDeepNestingScenario();
                    break;
                #endregion

                default:
                    throw new NotImplementedException($"Unknown scenario '{scenario}' passed to {GetType().Name}");
            }
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            switch (function)
            {
                case nameof(XmlFunction.TableData):
                    return scenario.GetResponse(address, function);

                default:
                    throw GetUnknownFunctionException(function);
            }
        }
    }
}
