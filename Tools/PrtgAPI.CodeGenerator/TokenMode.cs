namespace PrtgAPI.CodeGenerator
{
    public enum TokenMode
    {
        None,

        /// <summary>
        /// Indicates that a token summary, token parameter and token argument should automatically be added.
        /// </summary>
        Automatic,

        /// <summary>
        /// Indicates that a token summary, token parameter and named token argument should automatically be added.
        /// </summary>
        AutomaticNamed,

        /// <summary>
        /// Indicates that a token parameter and token argument should automatically be added with a default value to the regular overloads applicable within this region.
        /// </summary>
        AutomaticDefault,

        /// <summary>
        /// Indicates that a token parameter and named token argument should automatically be added with a default value to the regular overloads applicable within this region.
        /// </summary>
        AutomaticNamedDefault,

        /// <summary>
        /// Indicates that a token parameter and token argument should automatically be added with a default value to the synchronous and asynchronous overloads.
        /// </summary>
        MandatoryDefault,

        /// <summary>
        /// Indicates that a token parameter and token argument should automatically be added with a default value to the synchronous and asynchronous overloads.
        /// </summary>
        MandatoryNamedDefault,

        /// <summary>
        /// Indicates that a default token parameter should be supplied to internal calls when not provided by this method's parameters.
        /// </summary>
        MandatoryCall,

        /// <summary>
        /// Indicates that a named default token parameter should be supplied to internal calls when not provided by this method's parameters.
        /// </summary>
        MandatoryNamedCall,

        /// <summary>
        /// Indicates that a cancellation token has been manually specified.
        /// </summary>
        Manual
    }
}
