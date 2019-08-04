using System.ComponentModel;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies approval actions for controlling whether new probes may be utilized with PRTG.
    /// </summary>
    public enum ProbeApproval
    {
        /// <summary>
        /// Approve a new probe for use within PRTG.
        /// </summary>
        [Description("allow")]
        Allow,

        /// <summary>
        /// Approve a new probe for use within PRTG and immediately perform an auto-discovery.
        /// </summary>
        [Description("allowanddiscover")]
        AllowAndDiscover,

        /// <summary>
        /// Deny a probe from communicating with PRTG and blacklist its GID.
        /// </summary>
        [Description("deny")]
        Deny,
    }
}
