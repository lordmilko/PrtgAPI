using System;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.Parameters
{
    internal class SensorHistoryParametersBase : PageableParameters
    {
        public SensorHistoryParametersBase(Either<Sensor, int> sensorOrId, int average, DateTime? startDate, DateTime? endDate, Func<DateTime, DateTime> makeEndDate)
        {
            if (average < 0)
                throw new ArgumentException("Average must be greater than or equal to 0.", nameof(average));

            SensorId = sensorOrId.GetId();

            StartDate = startDate ?? DateTime.Now;
            EndDate = endDate ?? makeEndDate(StartDate);
            Average = average;
        }

        [ExcludeFromCodeCoverage]
        public int SensorId
        {
            get { return (int) this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public DateTime StartDate
        {
            get { return GetDate(Parameter.EndDate); }
            set { SetDate(Parameter.EndDate, value); }
        }

        public DateTime EndDate
        {
            get { return GetDate(Parameter.StartDate); }
            set { SetDate(Parameter.StartDate, value); }
        }

        public int Average
        {
            get { return (int) this[Parameter.Average]; }
            set { this[Parameter.Average] = value; }
        }

        private void SetDate(Parameter parameter, DateTime value)
        {
            this[parameter] = TypeHelpers.DateToString(value);
        }

        private DateTime GetDate(Parameter parameter)
        {
            return TypeHelpers.StringToDate(this[parameter].ToString());
        }
    }
}