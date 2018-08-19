using System;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI
{
    /// <summary>
    /// The arguments passed to an event handler when a informational debug data is logged.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LogVerboseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the description of the event.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogVerboseEventArgs"/> class.
        /// </summary>
        /// <param name="message">The description of the event.</param>
        public LogVerboseEventArgs(string message)
        {
            Message = message;
        }
    }
}
