using System;

namespace PrtgAPI.Parameters
{
    internal class SensorHistoryReportParameters : SensorHistoryParametersBase, IHtmlParameters
    {
        public HtmlFunction Function => HtmlFunction.HistoricDataReport;

        public SensorHistoryReportParameters(Either<Sensor, int> sensorOrId, DateTime? startDate, DateTime? endDate) :
            base(sensorOrId, 3600, startDate, endDate, s => s.AddDays(-1))
        {
        }
    }
}