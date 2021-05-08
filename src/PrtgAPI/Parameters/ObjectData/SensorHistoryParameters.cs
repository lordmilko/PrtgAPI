using System;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    internal class SensorHistoryParameters : SensorHistoryParametersBase, IShallowCloneable<SensorHistoryParameters>, IXmlParameters
    {
        XmlFunction IXmlParameters.Function => XmlFunction.HistoricData;

        public SensorHistoryParameters(Either<Sensor, int> sensorOrId, int average, DateTime? startDate, DateTime? endDate, int? count)
        : base(sensorOrId, average, startDate, endDate, s => s.AddDays(-1))
        {
            if (count != null)
                Count = count;

            SortBy = Property.DateTime;
            SortDirection = SortDirection.Descending;
        }

        SensorHistoryParameters IShallowCloneable<SensorHistoryParameters>.ShallowClone()
        {
            var newParameters = new SensorHistoryParameters(SensorId, Average, StartDate, EndDate, Count);

            ShallowClone(newParameters);

            return newParameters;
        }

        [ExcludeFromCodeCoverage]
        object IShallowCloneable.ShallowClone() => ((IShallowCloneable<SensorHistoryParameters>) this).ShallowClone();
    }
}
