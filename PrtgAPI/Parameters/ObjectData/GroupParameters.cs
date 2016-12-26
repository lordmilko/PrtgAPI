namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for retrieving <see cref="Group"/> objects.
    /// </summary>
    public class GroupParameters : TableParameters<Group>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupParameters"/> class.
        /// </summary>
        public GroupParameters() : base(Content.Groups)
        {
        }
    }
}
