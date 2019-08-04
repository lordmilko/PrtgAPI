using PrtgAPI.PowerShell.Cmdlets;

namespace PrtgAPI.PowerShell
{
    /// <summary>
    /// Specifies sensor factory summary modes known to <see cref="NewSensorFactoryDefinition"/>.
    /// </summary>
    public enum FactorySummaryMode
    {
        /// <summary>
        /// Calculate the minimum of all channel values.
        /// </summary>
        Min,

        /// <summary>
        /// Calculate the maximum of all channel values.
        /// </summary>
        Max,

        /// <summary>
        /// Calculate the sum of all channel values.
        /// </summary>
        Sum,

        /// <summary>
        /// Calculate the average of all channel values.
        /// </summary>
        Average
    }
}