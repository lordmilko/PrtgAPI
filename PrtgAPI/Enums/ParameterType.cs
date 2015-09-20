namespace Prtg
{
    /// <summary>
    /// Specifies how <see cref="T:Prtg.Parameter"/> values can be formatted when inserted in a <see cref="T:Prtg.PrtgUrl"/>.
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// <see cref="T:Prtg.Parameter"/> is used with a single value under a single parameter instance, i.e. param=value.
        /// </summary>
        SingleValue,

        /// <summary>
        /// <see cref="T:Prtg.Parameter"/> is used with multiple values under a single parameter instance, i.e. param=value1,value2.
        /// </summary>
        MultiValue,

        /// <summary>
        /// <see cref="T:Prtg.Parameter"/> is used with multiple values under multiple instances of a parameter (one value per instance), i.e. param=value1&amp;param=value2.
        /// </summary>
        MultiParameter
    }
}
