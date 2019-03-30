using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// The exception that is thrown when an XML document cannot be deserialized.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class XmlDeserializationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDeserializationException"/> class.
        /// </summary>
        public XmlDeserializationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDeserializationException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public XmlDeserializationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDeserializationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
        public XmlDeserializationException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDeserializationException"/> class with the outer type that was being deserialized and the XML that is believed to have caused the exception.
        /// </summary>
        /// <param name="type">The outer type that was being deserialized.</param>
        /// <param name="xml">The XML believed to have caused the exception.</param>
        public XmlDeserializationException(Type type, string xml) : base(GetMessage(type, xml, null))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDeserializationException"/> class with the outer type that was being deserialized, the XML that is believed to have caused the exception and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="type">The outer type that was being deserialized.</param>
        /// <param name="xml">The XML believed to have caused the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> parameter is not null, the current exception is raised in a catch block that handles the inner exception.</param>
        public XmlDeserializationException(Type type, string xml, Exception inner) : base(GetMessage(type, xml, inner), inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDeserializationException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected XmlDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private static string GetMessage(Type type, string xml, Exception ex)
        {
            var str = $"An error occurred while attempting to deserialize an object of type '{type.Name}', possibly caused by the following XML: '{xml}'.";

            if (ex?.InnerException != null)
            {
                str += " " + ex.InnerException.Message;
            }
            else
            {
                if (ex != null)
                    str += " " + ex.Message;
            }

            return str;
        }
    }
}
