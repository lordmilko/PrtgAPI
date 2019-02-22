using System.Linq;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for retrieving generic <see cref="PrtgObject"/> objects.
    /// </summary>
    public class PrtgObjectParameters : TableParameters<PrtgObject>, IShallowCloneable<PrtgObjectParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgObjectParameters"/> class.
        /// </summary>
        public PrtgObjectParameters() : base(Content.Objects)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgObjectParameters"/> class with one or more conditions to filter results by.
        /// </summary>
        /// <param name="filters">A list of conditions to filter results by.</param>
        public PrtgObjectParameters(params SearchFilter[] filters) : this()
        {
            SearchFilters = filters.ToList();
        }

        PrtgObjectParameters IShallowCloneable<PrtgObjectParameters>.ShallowClone()
        {
            var newParameters = new PrtgObjectParameters();

            ShallowClone(newParameters);

            return newParameters;
        }

        object IShallowCloneable.ShallowClone() => ((IShallowCloneable<PrtgObjectParameters>)this).ShallowClone();
    }
}