using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for retrieving <see cref="Sensor"/> objects.
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
        /// Initializes a new instance of the <see cref="SensorParameters"/> class with one or more conditions to filter results by.
        /// </summary>
        /// <param name="filters">A list of conditions to filter results by.</param>
        public SensorParameters(params SearchFilter[] filters) : this()
        {
            SearchFilters = filters.ToList();
        }

        /// <summary>
        /// Gets or sets a collection of search filters used to limit results according to one or more sensor statuses.
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

        [ExcludeFromCodeCoverage]
        object IShallowCloneable.ShallowClone() => ((IShallowCloneable<SensorParameters>) this).ShallowClone();
    }
}
