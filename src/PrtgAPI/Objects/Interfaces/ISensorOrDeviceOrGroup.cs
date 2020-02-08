namespace PrtgAPI
{
    /// <summary>
    /// Specifies properties that apply to Sensors, Devices and Groups.
    /// </summary>
    public interface ISensorOrDeviceOrGroup : ISensorOrDeviceOrGroupOrProbe
    {
        /// <summary>
        /// Probe that manages the execution of this object.
        /// </summary>
        string Probe { get; }
    }
}
