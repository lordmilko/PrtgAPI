namespace PrtgAPI
{
    /// <summary>
    /// Specifies properties that apply to Sensors and Devices.
    /// </summary>
    public interface ISensorOrDevice : ISensorOrDeviceOrGroup
    {
        /// <summary>
        /// Group this object is contained in.
        /// </summary>
        string Group { get; }

        /// <summary>
        /// Whether this object has been marked as a favorite.
        /// </summary>
        bool Favorite { get; }
    }
}
