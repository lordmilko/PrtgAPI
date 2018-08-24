using System;
using System.Net;
using System.Net.Http;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class ExceptionResponse : IWebResponse
    {
        private Exception exception;

        public ExceptionResponse(Exception ex)
        {
            exception = ex;
        }

        public string GetResponseText(ref string address)
        {
            switch (exception.GetType().Name)
            {
                case nameof(SocketException):
                    throw SocketException();

                default:
                    throw new NotSupportedException($"Don't know how to handle exception {typeof(Exception).Name}");
            }
        }

        private Exception SocketException()
        {
            return new AggregateException(new HttpRequestException("There was an issue completing the request", new WebException("There was an issue completing the request", exception)));
        }
    }
}
