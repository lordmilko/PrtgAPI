using System;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Helpers;

namespace PrtgAPI.Parameters
{
    class SensorHistoryParameters : Parameters
    {
        public SensorHistoryParameters(int sensorId, int average, DateTime? startDate, DateTime? endDate)
        {
            if(average < 0)
                throw new ArgumentException("Average must be greater than or equal to 0", nameof(average));

            SensorId = sensorId;

            StartDate = startDate ?? DateTime.Now.AddHours(-1);
            EndDate = endDate ?? DateTime.Now;
            Average = average;
        }

        [ExcludeFromCodeCoverage]
        public int SensorId
        {
            get { return (int)this[Parameter.Id]; }
            set { this[Parameter.Id] = value; }
        }

        public DateTime StartDate
        {
            get { return GetDate(Parameter.StartDate); }
            set { SetDate(Parameter.StartDate, value); }
        }

        public DateTime EndDate
        {
            get { return GetDate(Parameter.EndDate); }
            set { SetDate(Parameter.EndDate, value); }
        }

        public int Average
        {
            get { return (int) this[Parameter.Average]; }
            set { this[Parameter.Average] = value; }
        }

        private void SetDate(Parameter parameter, DateTime value)
        {
            this[parameter] = ParameterHelpers.DateToString(value);
        }

        private DateTime GetDate(Parameter parameter)
        {
            return ParameterHelpers.StringToDate(this[parameter].ToString());
        }
    }
}
