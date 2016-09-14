namespace PrtgAPI
{
    /// <summary>
    /// Specifies the Authentication Mode used to make requests against PRTG.
    /// </summary>
    public enum AuthMode
    {
        /// <summary>
        /// PRTG PassHash corresponding to a PRTG or Active Directory user account.
        /// </summary>
        PassHash,

        /// <summary>
        /// Password corresponding to a PRTG or Active Directory user account.
        /// </summary>
        Password
    }
}
