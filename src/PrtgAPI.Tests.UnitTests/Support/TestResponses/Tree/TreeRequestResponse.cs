using System;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class TreeRequestResponse : MultiTypeResponse
    {
        private TreeScenario scenario;

        public TreeRequestResponse(TreeRequestScenario scenario, bool async = false)
        {
            switch (scenario)
            {
                case TreeRequestScenario.StandaloneContainer:
                    this.scenario = new StandaloneContainerScenario();
                    break;

                case TreeRequestScenario.ContainerWithChild:
                    this.scenario = new ContainerWithChildScenario();
                    break;

                case TreeRequestScenario.ContainerWithGrandChild:
                    this.scenario = new ContainerWithGrandChildScenario();
                    break;

                case TreeRequestScenario.MultiLevelContainer:
                    this.scenario = new MultiLevelContainerScenario(async);
                    break;

                case TreeRequestScenario.ObjectPipeToContainerWithChild:
                    this.scenario = new ObjectPipeToContainerWithChildScenario();
                    break;

                case TreeRequestScenario.ActionPipeToContainerWithChild:
                    this.scenario = new ActionPipeToContainerWithChildScenario();
                    break;

                case TreeRequestScenario.FastPath:
                    this.scenario = new FastPathScenario(async);
                    break;

                case TreeRequestScenario.ObjectToFastPath:
                    this.scenario = new ObjectToFastPathScenario();
                    break;

                //Sensor Paths

                case TreeRequestScenario.FastPathSensorsOnly:
                    this.scenario = new FastPathSensorsOnlyScenario();
                    break;

                case TreeRequestScenario.SlowPathSensorsOnly:
                    this.scenario = new SlowPathSensorsOnlyScenario();
                    break;

                //Device Paths

                case TreeRequestScenario.FastPathDevicesOnly:
                    this.scenario = new FastPathDevicesOnlyScenario();
                    break;

                case TreeRequestScenario.SlowPathDevicesOnly:
                    this.scenario = new SlowPathDevicesOnlyScenario();
                    break;

                //Group Paths

                case TreeRequestScenario.FastPathGroupsOnly:
                    this.scenario = new FastPathGroupsOnlyScenario();
                    break;

                case TreeRequestScenario.SlowPathGroupsOnly:
                    this.scenario = new SlowPathGroupsOnlyScenario();
                    break;

                //Probe Paths

                case TreeRequestScenario.FastPathProbesOnly:
                    this.scenario = new FastPathProbesOnlyScenario();
                    break;

                case TreeRequestScenario.SlowPathProbesOnly:
                    this.scenario = new SlowPathProbesOnlyScenario();
                    break;

                //Trigger Paths

                case TreeRequestScenario.FastPathTriggersOnly:
                    this.scenario = new FastPathTriggersOnlyScenario(async);
                    break;

                case TreeRequestScenario.SlowPathTriggersOnly:
                    this.scenario = new SlowPathTriggersOnlyScenario(async);
                    break;

                //Property Paths

                case TreeRequestScenario.FastPathPropertiesOnly:
                    this.scenario = new FastPathPropertiesOnlyScenario();
                    break;

                case TreeRequestScenario.SlowPathPropertiesOnly:
                    this.scenario = new SlowPathPropertiesOnlyScenario();
                    break;

                //All

                case TreeRequestScenario.AllObjectTypes:
                    this.scenario = new AllObjectTypesScenario();
                    break;

                case TreeRequestScenario.LazyReorderChildren:
                    this.scenario = new LazyReorderChildrenScenario();
                    break;

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
                case nameof(HtmlFunction.ObjectData):
                    if (address.Contains("id=810"))
                        return base.GetResponse(ref address, function);
                    return scenario.GetResponse(address, function);
            }

            return base.GetResponse(ref address, function);
        }
    }
}
