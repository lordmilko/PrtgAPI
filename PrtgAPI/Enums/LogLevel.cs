using System;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the types of events that should be logged by <see cref="PrtgClient.LogVerbose"/>.
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        /// <summary>
        /// Disable all logging.
        /// </summary>
        None = 0,

        /// <summary>
        /// Trace messages describing decisions that will be made and the results of those actions.
        /// </summary>
        Trace = 1,

        /// <summary>
        /// HTTP requests that will be executed and whether those requests will be executed synchronously/asynchronously.
        /// </summary>
        Request = 2,

        /// <summary>
        /// Responses that are returned from PRTG in response to HTTP API requests.
        /// </summary>
        Response = 4,

        /// <summary>
        /// All log types.
        /// </summary>
        All = Trace | Request | Response
    }
}
