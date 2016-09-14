namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="T:PrtgAPI.PrtgUrl"/> for retrieving <see cref="T:PrtgAPI.Group"/> objects.
    /// </summary>
    public class GroupParameters : TableParameters<Group>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:PrtgAPI.Parameters.GroupParameters"/> class.
        /// </summary>
        public GroupParameters() : base(Content.Groups)
        {
        }
    }
}
