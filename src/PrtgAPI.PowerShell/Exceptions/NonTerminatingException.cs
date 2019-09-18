using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell
{
    /// <summary>
    /// The exception that is thrown when a non-terminating error occurs in a <see cref="PrtgCmdlet"/> that requires the stack be partially unwound.
    /// </summary>
    [ExcludeFromCodeCoverage]
    class NonTerminatingException : Exception
    {
        internal ErrorCategory ErrorCategory { get; }

        internal object TargetObject { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NonTerminatingException"/> for an <see cref="InvalidOperationException"/> with a specified message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        internal NonTerminatingException(string message) : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NonTerminatingException"/> class for an <see cref="InvalidOperationException"/> with a specified
        /// message and the object that was the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="targetObject">The external object that was responsible for this exception.</param>
        internal NonTerminatingException(string message, object targetObject) : this(new InvalidOperationException(message), ErrorCategory.InvalidOperation, targetObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NonTerminatingException"/> class with a custom exception/error category type and the object
        /// that was the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception this exception should encapsulate.</param>
        /// <param name="errorCategory">The error category of the <paramref name="innerException"/>.</param>
        /// <param name="targetObject">The external object that was responsible for this exception.</param>
        internal NonTerminatingException(Exception innerException, ErrorCategory errorCategory, object targetObject) : base(null, innerException)
        {
            ErrorCategory = errorCategory;
            TargetObject = targetObject;
        }
    }
}
