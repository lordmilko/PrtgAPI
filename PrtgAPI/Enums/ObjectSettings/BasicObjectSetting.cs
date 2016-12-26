namespace PrtgAPI
{
    /// <summary>
    /// Specifies basic object settings, found under the "Basic Settings" section of object settings pages.
    /// </summary>
    public enum BasicObjectSetting
    {
        //Basic Sensor Settings

        /// <summary>
        /// The name of the object.
        /// </summary>
        Name,

        /// <summary>
        /// Tags stored on the object for filtering purposes.
        /// </summary>
        Tags,

        //apparently you DON'T need to specofy an underscore to set values. need to test with the inheritance property...we could just revert back to using the description attribute if we dont need underscores

        /// <summary>
        /// The order with which the object will be displayed within lists.
        /// </summary>
        Priority,
    }
}
