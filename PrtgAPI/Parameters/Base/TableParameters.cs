using PrtgAPI.Objects.Shared;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for retrieving data stored in tables.
    /// </summary>
    /// <typeparam name="T">The type of PRTG Object to retrieve.</typeparam>
    public class TableParameters<T> : ContentParameters<T> where T : SensorOrDeviceOrGroupOrProbe
    {
        /// <summary>
        /// Filter objects to those with a <see cref="Property"/> of a certain value. Specify multiple filters to limit results further.
        /// </summary>
        public virtual SearchFilter[] SearchFilter
        {
            get { return (SearchFilter[])this[Parameter.FilterXyz]; }
            set { this[Parameter.FilterXyz] = value; }
        }

        /// <summary>
        /// <see cref="Property"/> to sort response by.
        /// </summary>
        public Property SortBy
        {
            get { return (Property)this[Parameter.SortBy]; }
            set { this[Parameter.SortBy] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableParameters{T}"/> class.
        /// </summary>
        protected TableParameters(Content content) : base(content)
        {
        }
    }
}
