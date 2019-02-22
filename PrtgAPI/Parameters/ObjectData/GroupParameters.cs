using System.Linq;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for retrieving <see cref="Group"/> objects.
    /// </summary>
    public class GroupParameters : TableParameters<Group>, IShallowCloneable<GroupParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupParameters"/> class.
        /// </summary>
        public GroupParameters() : base(Content.Groups)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupParameters"/> class with one or more conditions to filter results by.
        /// </summary>
        /// <param name="filters">A list of conditions to filter results by.</param>
        public GroupParameters(params SearchFilter[] filters) : this()
        {
            SearchFilters = filters.ToList();
        }

        GroupParameters IShallowCloneable<GroupParameters>.ShallowClone()
        {
            var newParameters = new GroupParameters();

            ShallowClone(newParameters);

            return newParameters;
        }

        object IShallowCloneable.ShallowClone() => ((IShallowCloneable<GroupParameters>)this).ShallowClone();
    }
}
