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
            }

            return base.GetResponse(ref address, function);
        }
    }
}
