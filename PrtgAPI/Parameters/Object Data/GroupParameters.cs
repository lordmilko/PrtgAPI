namespace Prtg.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="T:Prtg.PrtgUrl"/> for retrieving <see cref="T:Prtg.Group"/> objects.
    /// </summary>
    public class GroupParameters : TableParameters<Group>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Prtg.Parameters.GroupParameters"/> class.
        /// </summary>
        public GroupParameters() : base(Content.Groups)
        {
        }
    }
}
