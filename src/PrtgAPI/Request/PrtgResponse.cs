using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Xml;
using PrtgAPI.Utilities;

namespace PrtgAPI.Request
{
    enum PrtgResponseType
    {
        String,
        Stream
    }

    class PrtgResponse : IDisposable
    {
        bool isDirty;

        private string stringValue;

        public string StringValue
        {
            get
            {
                if (Type != PrtgResponseType.String)
                    throw new InvalidOperationException($"Attempted to read {nameof(StringValue)} from a response of type '{Type}'.");

                return stringValue;
            }
        }

        private Stream streamValue;

        [ExcludeFromCodeCoverage]
        internal Stream GetStreamUnsafe()
        {
            if (Type != PrtgResponseType.Stream)
                throw new InvalidOperationException($"Attempted to read {nameof(streamValue)} from a response of type '{Type}'.");

            return streamValue;
        }

        public PrtgResponseType Type { get; }

        public PrtgResponse(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            streamValue = stream;
            Type = PrtgResponseType.Stream;
        }

        public PrtgResponse(string str, bool isDirty)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            stringValue = str;
            Type = PrtgResponseType.String;
            this.isDirty = isDirty;
        }

        public XmlReader ToXmlReader()
        {
            switch(Type)
            {
                case PrtgResponseType.Stream:
                    return XmlReader.Create(streamValue);

                case PrtgResponseType.String:

                    var str = stringValue;

                    if (isDirty)
                        str = XDocumentUtilities.SanitizeStr(stringValue);

                    return XmlReader.Create(new StringReader(str));

                default:
                    throw new NotImplementedException($"Don't know how to create {nameof(XmlReader)} for response of type '{Type}'.");
            }
        }

        public static bool IsSafeDataFormat(HttpResponseMessage message)
        {
            var function = message.RequestMessage.RequestUri.AbsolutePath;

            return function.EndsWith(".xml") || function.EndsWith(".json") || function.EndsWith(".csv");
        }

        public static implicit operator PrtgResponse(string str)
        {
            if (str == null)
                return null;

            //Implicit conversions to string occur when returning string literals; as such, no need to parse dirty
            return new PrtgResponse(str, false);
        }

        public void Dispose()
        {
            if (Type == PrtgResponseType.Stream)
                streamValue.Dispose();
        }
    }
}
