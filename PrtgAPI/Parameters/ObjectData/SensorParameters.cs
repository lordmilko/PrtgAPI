using PrtgAPI.Request;

namespace PrtgAPI.Parameters.ObjectData
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for retrieving <see cref="Sensor"/> objects.
    /// </summary>
    public class SensorParameters : TableParameters<Sensor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SensorParameters"/> class.
        /// </summary>
        public SensorParameters() : base(Content.Sensors)
        {
        }

        /// <summary>
        /// Filter PRTG Results according to one or more sensor statuses.
        /// </summary>
        public Status[] StatusFilter
        {
            get { return (Status[]) this[Parameter.FilterStatus]; }
            set { this[Parameter.FilterStatus] = value; }
        }

        //todo: implement filter_tags
    }
}
