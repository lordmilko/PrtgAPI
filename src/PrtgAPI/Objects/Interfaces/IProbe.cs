namespace PrtgAPI
{
    /// <summary>
    /// Specifies properties that apply to Probes.
    /// </summary>
    public interface IProbe : IGroupOrProbe
    {
        /// <summary>
        /// Connected status of the probe.
        /// </summary>
        ProbeStatus ProbeStatus { get; }
    }
}
