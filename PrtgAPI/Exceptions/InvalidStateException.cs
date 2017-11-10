using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// The exception that is thrown when an object on a PRTG Server is not in a valid state.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class InvalidStateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateException"/> class.
        /// </summary>
        public InvalidStateException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InvalidStateException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
        public InvalidStateException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected InvalidStateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
