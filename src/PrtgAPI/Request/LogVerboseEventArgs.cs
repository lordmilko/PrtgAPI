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
        /// Gets or sets the type of event that was logged.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogVerboseEventArgs"/> class.
        /// </summary>
        /// <param name="message">The description of the event.</param>
        /// <param name="logLevel">The type of event that was logged.</param>
        public LogVerboseEventArgs(string message, LogLevel logLevel)
        {
            Message = message;
            LogLevel = logLevel;
        }
    }
}
