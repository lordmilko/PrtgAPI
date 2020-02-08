namespace PrtgAPI
{
    /// <summary>
    /// <para>Specifies properties that apply to Devices and Groups.</para>
    /// </summary>
    public interface IDeviceOrGroup : ISensorOrDeviceOrGroup, IDeviceOrGroupOrProbe
    {
        /// <summary>
        /// Auto-discovery progress (if one is in progress). Otherwise, null.
        /// </summary>
        string Condition { get; }
    }
}
