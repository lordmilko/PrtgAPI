using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for retrieving <see cref="Sensor"/> objects.
    /// </summary>
    public class SensorParameters : TableParameters<Sensor>, IShallowCloneable<SensorParameters>
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
        public Status[] Status
        {
            get { return GetMultiParameterFilterValue<Status>(Property.Status); }
            set { SetMultiParameterFilterValue(Property.Status, value); }
        }

        SensorParameters IShallowCloneable<SensorParameters>.ShallowClone()
        {
            var newParameters = new SensorParameters();

            ShallowClone(newParameters);

            return newParameters;
        }

        object IShallowCloneable.ShallowClone() => ((IShallowCloneable<SensorParameters>) this).ShallowClone();
    }
}
