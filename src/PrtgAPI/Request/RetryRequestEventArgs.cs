using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI
{
    /// <summary>
    /// The arguments passed to an event handler when a web request is retried.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RetryRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the exception that caused the request to fail.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the URL of the request that failed.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the number of retries remaining for the current request.
        /// </summary>
        public int RetriesRemaining { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryRequestEventArgs"/> class.
        /// </summary>
        /// <param name="ex">The exception that caused the request to fail.</param>
        /// <param name="url">The URL of the request that failed.</param>
        /// <param name="retriesRemaining">The number of retries remaining.</param>
        public RetryRequestEventArgs(Exception ex, string url, int retriesRemaining)
        {
            Exception = ex;
            Url = url;
            RetriesRemaining = retriesRemaining;
        }
    }
}
