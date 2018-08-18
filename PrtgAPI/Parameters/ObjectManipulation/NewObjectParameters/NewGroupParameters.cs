using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// <para type="description">Represents parameters used to construct a <see cref="PrtgUrl"/> for adding new <see cref="Group"/> objects.</para>
    /// </summary>
    public class NewGroupParameters : NewObjectParameters
    {
        internal override CommandFunction Function => CommandFunction.AddGroup2;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewGroupParameters"/> class.
        /// </summary>
        /// <param name="groupName">The name to use for this group.</param>
        public NewGroupParameters(string groupName) : base(groupName)
        {
        }
    }
}
