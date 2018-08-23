namespace PrtgAPI.PowerShell.Progress
{
    /// <summary>
    /// Specifies the progress processing mode of a cmdlet.
    /// </summary>
    enum ProcessingOperation
    {
        /// <summary>
        /// Retrieving items from the server.
        /// </summary>
        Retrieving,

        /// <summary>
        /// Processing previously retrieved items from the server.
        /// </summary>
        Processing
    };
}
