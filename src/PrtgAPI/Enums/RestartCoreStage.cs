namespace PrtgAPI
{
    /// <summary>
    /// Specifies stages of restarting the PRTG Core Service.
    /// </summary>
    public enum RestartCoreStage
    {
        /// <summary>
        /// PRTG is still in the process of shutting down.
        /// </summary>
        Shutdown = 1,

        /// <summary>
        /// PRTG is currently in the middle of restarting.
        /// </summary>
        Restart = 2,

        /// <summary>
        /// PRTG has restarted and is in the process of initializing.
        /// </summary>
        Startup = 3,

        /// <summary>
        /// PRTG has successfully restarted.
        /// </summary>
        Completed
    }
}
