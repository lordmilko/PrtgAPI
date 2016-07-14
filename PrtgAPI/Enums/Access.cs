namespace Prtg
{
    /// <summary>
    /// Specifies the Access Rights applied to a PRTG Object.
    /// </summary>
    public enum Access
    {
        Inherited,
        
        /// <summary>
        /// The object is not displayed. Logs, tickets and alarms pertaining to the object are not visible.
        /// </summary>
        None,

        /// <summary>
        /// The object can be viewed but not edited.
        /// </summary>
        Read,

        /// <summary>
        /// The object can be viewed, edited and deleted.
        /// </summary>
        Write,

        /// <summary>
        /// The object can be viewed, edited and deleted. In addition, Access Rights can be modified.
        /// </summary>
        Full,

        /// <summary>
        /// All options are available.
        /// </summary>
        Admin
    }
}
