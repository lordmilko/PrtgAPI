using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace PrtgAPI.Exceptions.Internal
{
    /// <summary>
    /// The exception that is thrown when an <see cref="Expression"/> is encountered that is not valid for the specified context.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    class InvalidExpressionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidExpressionException"/> class.
        /// </summary>
        public InvalidExpressionException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidExpressionException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InvalidExpressionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidExpressionException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
        public InvalidExpressionException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidExpressionException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected InvalidExpressionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
