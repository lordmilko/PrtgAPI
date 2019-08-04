namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Specifies types of config files that can be loaded by PRTG.</para>
    /// </summary>
    public enum ConfigFileType
    {
        /// <summary>
        /// Device icons, report templates and language files.
        /// </summary>
        General,

        /// <summary>
        /// Custom lookups used for customizing the display of sensor channel values.
        /// </summary>
        Lookups
    }
}
