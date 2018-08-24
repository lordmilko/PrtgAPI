using System;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class TakeIteratorResponse : MultiTypeResponse
    {
        private TakeScenario scenario;

        public TakeIteratorResponse(TakeIteratorScenario scenario)
        {
            switch (scenario)
            {
                #region Sensors

                case TakeIteratorScenario.Sensors:
                    this.scenario = new TakeUnfilteredSensorsScenario();
                    break;
                case TakeIteratorScenario.SensorsInsufficient:
                    this.scenario = new TakeUnfilteredSensorsInsufficientScenario();
                    break;
                case TakeIteratorScenario.SensorsFromGroup:
                    this.scenario = new TakeUnfilteredSensorsFromGroupScenario();
                    break;
                case TakeIteratorScenario.SensorsWithFilter:
                    this.scenario = new TakeFilteredSensorsScenario();
                    break;
                case TakeIteratorScenario.SensorsWithFilterFromGroup:
                    this.scenario = new TakeFilteredSensorsFromGroupScenario();
                    break;

                case TakeIteratorScenario.SensorsWithFilterFromGroupInsufficient:
                    this.scenario = new TakeFilteredSensorsFromGroupInsufficientScenario();
                    break;
                case TakeIteratorScenario.SensorsWithFilterInsufficient:
                    this.scenario = new TakeFilteredSensorsInsufficientScenario();
                    break;
                case TakeIteratorScenario.SensorsWithFilterInsufficientOneLeft:
                    this.scenario = new TakeFilteredSensorsInsufficientOneLeftScenario();
                    break;
                case TakeIteratorScenario.SensorsWithFilterInsufficientNoneLeft:
                    this.scenario = new TakeFilteredSensorsInsufficientNoneLeftScenario();
                    break;
                case TakeIteratorScenario.SensorsWithFilterInsufficientNegativeLeft:
                    this.scenario = new TakeFilteredSensorsInsufficientNegativeLeftScenario();
                    break;

                case TakeIteratorScenario.SensorsWithFilterFromDuplicateGroup:
                    this.scenario = new TakeFilteredSensorsFromDuplicateGroupScenario();
                    break;
                case TakeIteratorScenario.SensorsWithFilterFromDuplicateGroupInsufficient:
                    this.scenario = new TakeFilteredSensorsFromDuplicateGroupInsufficientScenario();
                    break;

                case TakeIteratorScenario.SensorsFromGroupNoRecurse:
                    this.scenario = new TakeUnfilteredSensorsFromGroupNoRecurseScenario();
                    break;
                case TakeIteratorScenario.SensorsWithFilterFromGroupNoRecurse:
                    this.scenario = new TakeFilteredSensorsFromGroupNoRecurseScenario();
                    break;

                #endregion
                #region Logs

                case TakeIteratorScenario.Logs:
                    this.scenario = new TakeUnfilteredLogsScenario();
                    break;
                case TakeIteratorScenario.LogsWithFilter:
                    this.scenario = new TakeFilteredLogsScenario();
                    break;

                case TakeIteratorScenario.LogsInsufficient:
                    this.scenario = new TakeUnfilteredLogsInsufficientScenario();
                    break;
                case TakeIteratorScenario.LogsWithFilterInsufficient:
                    this.scenario = new TakeFilteredLogsInsufficientScenario();
                    break;
                case TakeIteratorScenario.LogsWithFilterInsufficientOneLeft:
                    this.scenario = new TakeFilteredLogsInsufficientOneLeftScenario();
                    break;
                case TakeIteratorScenario.LogsWithFilterInsufficientNoneLeft:
                    this.scenario = new TakeFilteredLogsInsufficientNoneLeftScenario();
                    break;
                case TakeIteratorScenario.LogsWithFilterInsufficientNegativeLeft:
                    this.scenario = new TakeFilteredLogsInsufficientNegativeLeftScenario();
                    break;

                case TakeIteratorScenario.LogsForceStream:
                    this.scenario = new TakeUnfilteredLogsForceStreamScenario();
                    break;
                case TakeIteratorScenario.LogsWithFilterForceStream:
                    this.scenario = new TakeFilteredLogsForceStreamScenario();
                    break;

                case TakeIteratorScenario.LogsWithFilterInsufficientForceStream:
                    this.scenario = new TakeFilteredLogsInsufficientForceStreamScenario();
                    break;
                case TakeIteratorScenario.LogsWithFilterInsufficientOneLeftForceStream:
                    this.scenario = new TakeFilteredLogsInsufficientOneLeftForceStreamScenario();
                    break;
                case TakeIteratorScenario.LogsWithFilterInsufficientNoneLeftForceStream:
                    this.scenario = new TakeFilteredLogsInsufficientNoneLeftForceStreamScenario();
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
