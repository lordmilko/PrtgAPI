using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// The exception that is thrown when a trigger of an incompatible type is added to an object.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class InvalidTriggerTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTriggerTypeException"/> class.
        /// </summary>
        public InvalidTriggerTypeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTriggerTypeException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InvalidTriggerTypeException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTriggerTypeException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
        public InvalidTriggerTypeException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTriggerTypeException"/> class with a specified Object ID, the type of trigger that is invalid and a list of valid trigger types.
        /// </summary>
        /// <param name="objectId">The ID of the object the trigger attempted to be applied to.</param>
        /// <param name="type">The type of trigger that is not valid for the object.</param>
        /// <param name="validTypes">The types of triggers that are valid for the object.</param>
        public InvalidTriggerTypeException(int objectId, TriggerType type, List<TriggerType> validTypes) : base(GetMessage(objectId, type, validTypes))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTriggerTypeException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public InvalidTriggerTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private static string GetMessage(int objectId, TriggerType type, List<TriggerType> validTypes)
        {
            return $"Trigger type '{type}' is not a valid trigger type for object ID '{objectId}'. The following types are supported: {string.Join(", ", validTypes)}.";
        }
    }
}
