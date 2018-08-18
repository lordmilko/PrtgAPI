namespace PrtgAPI
{
    /// <summary>
    /// Represents a user group used for organizing one or more <see cref="UserAccount"/> objects within PRTG.
    /// </summary>
    public class UserGroup : PrtgObject
    {
        internal UserGroup(string raw) : base(raw)
        {
        }
    }
}
