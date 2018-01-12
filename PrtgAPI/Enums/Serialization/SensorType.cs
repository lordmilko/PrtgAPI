namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Specifies types of sensors that can be created in PRTG.</para>
    /// </summary>
    public enum SensorType
    {
        /// <summary>
        /// EXE/Script Advanced sensor, returning XML or JSON
        /// </summary>
        ExeXml,

        /// <summary>
        /// WMI Service sensor, for monitoring a Microsoft Windows system service.
        /// </summary>
        WmiService
    }
}
