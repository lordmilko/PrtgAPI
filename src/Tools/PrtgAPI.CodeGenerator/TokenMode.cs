namespace PrtgAPI.CodeGenerator
{
    public enum TokenMode
    {
        /// <summary>
        /// No token mode has been specified. If a method is deemed as requiring a token mode (e.g. the method is asynchronous) <see cref="TokenMode.Automatic"/> will automatically be used.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that an overload containing a token summary, token parameter and token argument should automatically be added. This is the default Token Mode when a token is deemed required.<para/>
        /// Applies only to asynchronous methods, unless specified on a method inside a Token Region.
        /// </summary>
        /// <remarks>
        ///     async Task MAsync();
        ///     async Task MAsync(CancellationToken token);
        /// </remarks>
        Automatic,

        /// <summary>
        /// Indicates that an overload containing a token summary, token parameter and token argument should automatically be added to
        /// both synchronous and asynchronous overloads.
        /// </summary>
        /// <remarks>
        ///     void M();
        ///     void M(CancellationToken token);
        ///     async Task MAsync();
        ///     async Task MAsync(CancellationToken token);
        /// </remarks>
        AutomaticAll,

        /// <summary>
        /// Indicates that an overload containing a token summary, token parameter and named token argument should automatically be added.<para/>
        /// Applies only to asynchronous methods, unless specified on a method inside a Token Region.
        /// </summary>
        /// <remarks>
        ///     async Task MAsync(CancellationToken token)
        ///     {
        ///         await InternalAsync(token: token);
        ///     }
        /// </remarks>
        AutomaticNamed,

        /// <summary>
        /// Indicates that an overload containing a token parameter and token argument should automatically be added with a default value to the regular overloads applicable within this region.
        /// </summary>
        /// <remarks>
        ///    async Task MAsync(CancellationToken token = default(CancellationToken));
        /// </remarks>
        AutomaticDefault,

        /// <summary>
        /// Indicates that an overload containing a token parameter and named token argument should automatically be added with a default value to the regular overloads applicable within this region.
        /// </summary>
        /// <remarks>
        ///     async Task MAsync(CancellationToken token = default(CancellationToken))
        ///     {
        ///         await InternalAsync(token: token);
        ///     }
        /// </remarks>
        AutomaticNamedDefault,

        /// <summary>
        /// Indicates that a token parameter and token argument should automatically be added with a default value to the synchronous and asynchronous overloads.
        /// </summary>
        /// <remarks>
        ///     void M(CancellationToken token = default(CancellationToken));
        ///     async Task MAsync(CancellationToken token = default(CancellationToken));
        /// </remarks>
        MandatoryDefault,

        /// <summary>
        /// Indicates that a token parameter and token argument should automatically be added with a default value to the synchronous and asynchronous overloads.
        /// </summary>
        /// <remarks>
        ///     void M(CancellationToken token = default(CancellationToken))
        ///     {
        ///         Internal(token: token);
        ///     }
        ///
        ///     async Task MAsync(CancellationToken token = default(CancellationToken))
        ///     {
        ///         await InternalAsync(token: token);
        ///     }
        /// </remarks>
        MandatoryNamedDefault,

        /// <summary>
        /// Indicates that a default token parameter should be supplied to internal calls when not provided by this method's parameters.
        /// </summary>
        /// <remarks>
        ///     void M()
        ///     {
        ///         Internal(CancellationToken.None);
        ///     }
        ///
        ///     async Task MAsync(CancellationToken token)
        ///     {
        ///         await InternalAsync(token);
        ///     }
        /// </remarks>
        MandatoryCall,

        /// <summary>
        /// Indicates that a named default token parameter should be supplied to internal calls when not provided by this method's parameters.
        /// </summary>
        /// <remarks>
        ///     void M()
        ///     {
        ///         Internal(token: CancellationToken.None);
        ///     }
        ///
        ///     async Task MAsync(CancellationToken token)
        ///     {
        ///         await InternalAsync(token: token);
        ///     }
        /// </remarks>
        MandatoryNamedCall,

        /// <summary>
        /// Indicates that a cancellation token has been manually specified. Used for example when a custom XmlDoc description is required.
        /// </summary>
        /// <remarks>
        ///     void M(Cancellation Token);
        ///     async Task MAsync(CancellationToken);
        /// </remarks>
        Manual
    }
}
