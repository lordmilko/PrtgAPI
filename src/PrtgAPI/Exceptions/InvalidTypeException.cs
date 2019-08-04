using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// The exception that is thrown when a type is not valid for a specified context.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class InvalidTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTypeException"/> class.
        /// </summary>
        public InvalidTypeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTypeException"/> class with a specified type.
        /// </summary>
        /// <param name="type">The type that was not valid in the given context.</param>
        public InvalidTypeException(Type type) : base($"Type '{type}' was not valid in the given context.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTypeException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InvalidTypeException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTypeException"/> class with a specified type and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="type">The type that was not valid in the given context.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
        public InvalidTypeException(Type type, Exception inner) : base($"Type '{type}' was not valid in the given context.", inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTypeException"/> class with the tye that was expected and the actual type that was received.
        /// </summary>
        /// <param name="expectedType">The type that was expected.</param>
        /// <param name="actualType">The type that was received.</param>
        public InvalidTypeException(Type expectedType, Type actualType) : base($"Expected type '{expectedType}' however received type '{actualType}'.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTypeException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public InvalidTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
