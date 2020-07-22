using System;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
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
                case RecursiveRequestScenario.SensorNoRecurseUniqueGroup:
                    this.scenario = new SensorNoRecurseUniqueGroupScenario();
                    break;
                case RecursiveRequestScenario.SensorNoRecurseDuplicateGroup:
                    this.scenario = new SensorNoRecurseDuplicateGroupScenario();
                    break;
                case RecursiveRequestScenario.SensorDeepNesting:
                    this.scenario = new SensorDeepNestingScenario();
                    break;
                case RecursiveRequestScenario.SensorDeepNestingChild:
                    this.scenario = new SensorDeepNestingChildScenario();
                    break;
                case RecursiveRequestScenario.SensorDeepNestingGrandChild:
                    this.scenario = new SensorDeepNestingGrandChildScenario();
                    break;
                case RecursiveRequestScenario.SensorDeepNestingGreatGrandChild:
                    this.scenario = new SensorDeepNestingGreatGrandChildScenario();
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
                case RecursiveRequestScenario.DeviceDeepNesting:
                    this.scenario = new DeviceDeepNestingScenario();
                    break;
                case RecursiveRequestScenario.DeviceDeepNestingChild:
                    this.scenario = new DeviceDeepNestingChildScenario();
                    break;
                case RecursiveRequestScenario.DeviceDeepNestingGrandChild:
                    this.scenario = new DeviceDeepNestingGrandChildScenario();
                    break;
                case RecursiveRequestScenario.DeviceDeepNestingGreatGrandChild:
                    this.scenario = new DeviceDeepNestingGreatGrandChildScenario();
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
                case RecursiveRequestScenario.GroupDeepNestingChild:
                    this.scenario = new GroupDeepNestingChildScenario();
                    break;
                case RecursiveRequestScenario.GroupDeepNestingGrandChild:
                    this.scenario = new GroupDeepNestingGrandChildScenario();
                    break;
                case RecursiveRequestScenario.GroupDeepNestingGreatGrandChild:
                    this.scenario = new GroupDeepNestingGreatGrandChildScenario();
                    break;
                #endregion
                #region Count

                case RecursiveRequestScenario.GroupRecurseAvailableCount:
                    this.scenario = new GroupRecurseAvailableCount();
                    break;
                case RecursiveRequestScenario.GroupRecurseUnavailableCount:
                    this.scenario = new GroupRecurseUnavailableCount();
                    break;
                case RecursiveRequestScenario.GroupRecurseAvailableSingleCount:
                    this.scenario = new GroupRecurseAvailableSingleCount();
                    break;
                case RecursiveRequestScenario.GroupNoRecurseAvailableCount:
                    this.scenario = new GroupNoRecurseAvailableCount();
                    break;
                case RecursiveRequestScenario.GroupNoRecurseUnavailableCount:
                    this.scenario = new GroupNoRecurseUnavailableCount();
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
