using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// The exception that is thown when a HTTP document cannot be deserialized.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class HttpDeserializationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDeserializationException"/> class.
        /// </summary>
        public HttpDeserializationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDeserializationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public HttpDeserializationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDeserializationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
        public HttpDeserializationException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDeserializationException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected HttpDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
