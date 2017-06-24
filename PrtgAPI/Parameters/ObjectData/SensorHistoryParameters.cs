using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI.Parameters
{
    class SensorHistoryParameters : Parameters
    {
        public SensorHistoryParameters(int sensorId)
        {
            SensorId = sensorId;
        }

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

        private void SetDate(Parameter parameter, DateTime value)
        {
            this[parameter] = value.ToString("yyyy-MM-dd-HH-mm-ss");
        }

        private DateTime GetDate(Parameter parameter)
        {
            return DateTime.ParseExact(this[parameter].ToString(), "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
        }
    }
}
