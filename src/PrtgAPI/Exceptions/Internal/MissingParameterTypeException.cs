using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Exceptions.Internal
{
    /// <summary>
    /// The exception that is thrown when a <see cref="Parameter"/> is missing a <see cref="ParameterType"/> .
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class MissingParameterTypeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingParameterTypeException"/> class with a specified <see cref="Parameter"/>.
        /// </summary>
        /// <param name="parameter">The parameter missing a <see cref="ParameterTypeAttribute"/> property that caused this exception.</param>
        public MissingParameterTypeException(Parameter parameter) : base(GetMessage(parameter))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingParameterTypeException"/> class with a specified <see cref="Parameter"/> and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="parameter">The parameter missing a <see cref="ParameterTypeAttribute"/> property that caused this exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
        public MissingParameterTypeException(Parameter parameter, Exception inner) : base(GetMessage(parameter), inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingParameterTypeException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public MissingParameterTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private static string GetMessage(Parameter parameter)
        {
            return $"Parameter '{parameter.ToString()}' is missing a '{nameof(Attributes.ParameterTypeAttribute)}' attribute. This error message indicates an internal bug that must be corrected.";
        }
    }
}
