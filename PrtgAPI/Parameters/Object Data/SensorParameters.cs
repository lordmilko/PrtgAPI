using System.Linq;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="T:PrtgAPI.PrtgUrl"/> for retrieving <see cref="T:PrtgAPI.Sensor"/> objects.
    /// </summary>
    public class SensorParameters : TableParameters<Sensor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:PrtgAPI.Parameters.SensorParameters"/> class.
        /// </summary>
        public SensorParameters() : base(Content.Sensors)
        {
        }

        /// <summary>
        /// Filter PRTG Results according to one or more sensor statuses.
        /// </summary>
        public SensorStatus[] StatusFilter
        {
            get { return (SensorStatus[]) this[Parameter.FilterStatus]; }
            set
            {
                this[Parameter.FilterStatus] = value.Select(x => (int)x).ToArray();
            }
        }

        //todo: implement filter_tags
    }
}
