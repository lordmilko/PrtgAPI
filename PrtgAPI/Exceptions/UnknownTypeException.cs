using System;
using System.Runtime.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// The exception that is thrown when a method does not know how to convert a generic object to its actual type.
    /// </summary>
    [Serializable]
    public class UnknownTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownTypeException"/> class.
        /// </summary>
        public UnknownTypeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownTypeException"/> class with a specified type.
        /// </summary>
        /// <param name="type">The type whose conversion implementation could not be found.</param>
        public UnknownTypeException(Type type) : base($"Implementation missing for converting to type '{type}'")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownTypeException"/> class with a specified type and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="type">The type whose conversion implementation could not be found.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
        public UnknownTypeException(Type type, Exception inner) : base($"Implementation missing for converting to type '{type}'", inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownTypeException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public UnknownTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}