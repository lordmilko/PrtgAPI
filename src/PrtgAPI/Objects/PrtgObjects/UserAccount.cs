namespace PrtgAPI
{
    /// <summary>
    /// Represents a user account capable of logging into PRTG.
    /// </summary>
    public class UserAccount : PrtgObject
    {
        internal UserAccount(string raw) : base(raw)
        {
        }
    }
}
