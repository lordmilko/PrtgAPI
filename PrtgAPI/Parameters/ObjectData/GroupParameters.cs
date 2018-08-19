using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for retrieving <see cref="Group"/> objects.
    /// </summary>
    public class GroupParameters : TableParameters<Group>, IShallowCloneable<GroupParameters>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupParameters"/> class.
        /// </summary>
        public GroupParameters() : base(Content.Groups)
        {
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
