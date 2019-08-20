using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using PrtgAPI.Parameters;
using PrtgAPI.Utilities;

namespace PrtgAPI.Request
{
    /// <summary>
    /// Handles constructing requests from parameter objects and executing them against a HTTP Client.
    /// </summary>
    class RequestEngine
    {
        private IWebClient webClient;
        private PrtgClient prtgClient;

        private const int BatchLimit = 1500;

        internal CancellationToken DefaultCancellationToken { get; set; }

        /// <summary>
        /// Indicates whether a response has been received containing an invalid character (e.g. \0 in XML)
        /// and that future requests should use safe parsing heuristics.
        /// </summary>
        internal bool IsDirty { get; set; }

        internal RequestEngine(PrtgClient prtgClient, IWebClient webClient)
        {
            this.prtgClient = prtgClient;
            this.webClient = webClient;
        }

        #region CSV

        [ExcludeFromCodeCoverage]
        internal PrtgResponse ExecuteRequest(ICsvParameters parameters, Func<HttpResponseMessage, PrtgResponse> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

            var url = GetRequestMessage(parameters);

            var response = ExecuteRequest(url, token, responseParser);

            return response;
        }

        #endregion
        #region JSON + Response Parser

        internal PrtgResponse ExecuteRequest(IJsonParameters parameters, Func<HttpResponseMessage, PrtgResponse> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

            var request = GetRequestMessage(parameters);

            var response = ExecuteRequest(request, token, responseParser);

            return response;
        }

        internal async Task<PrtgResponse> ExecuteRequestAsync(IJsonParameters parameters, Func<HttpResponseMessage, Task<PrtgResponse>> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

            var request = GetRequestMessage(parameters);

            var response = await ExecuteRequestAsync(request, token, responseParser).ConfigureAwait(false);

            return response;
        }

        #endregion
        #region XML + Response Validator / Response Parser

        [Obsolete("XmlReader ExecuteRequest() should not be used directly. Use ObjectEngine.GetObjectsXml() instead")]
        internal XmlReader ExecuteRequest(IXmlParameters parameters, Action<PrtgResponse> responseValidator = null, Func<HttpResponseMessage, PrtgResponse> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

            var request = GetRequestMessage(parameters);

            var response = ExecuteRequest(request, token, responseParser);

            responseValidator?.Invoke(response);

            return response.ToXmlReader();
        }

        [Obsolete("Task<XmlReader> ExecuteRequest() should not be used directly. Use ObjectEngine.GetObjectsXml() instead")]
        internal async Task<XmlReader> ExecuteRequestAsync(IXmlParameters parameters, Action<PrtgResponse> responseValidator = null, Func<HttpResponseMessage, Task<PrtgResponse>> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

            var request = GetRequestMessage(parameters);

            var response = await ExecuteRequestAsync(request, token, responseParser).ConfigureAwait(false);

            responseValidator?.Invoke(response);

            return response.ToXmlReader();
        }

        #endregion
        #region Command + Response Parser

        internal PrtgResponse ExecuteRequest(ICommandParameters parameters, Func<HttpResponseMessage, PrtgResponse> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

            if (parameters is IMultiTargetParameters)
                return ExecuteMultiRequest(p => GetRequestMessage((ICommandParameters)p), (IMultiTargetParameters)parameters, token, responseParser);

            var url = GetRequestMessage(parameters);

            var response = ExecuteRequest(url, token, responseParser);

            return response;
        }

        internal async Task<PrtgResponse> ExecuteRequestAsync(ICommandParameters parameters, Func<HttpResponseMessage, Task<PrtgResponse>> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

            if (parameters is IMultiTargetParameters)
                return await ExecuteMultiRequestAsync(p => GetRequestMessage((ICommandParameters)p), (IMultiTargetParameters)parameters, token, responseParser).ConfigureAwait(false);

            var url = GetRequestMessage(parameters);

            var response = await ExecuteRequestAsync(url, token, responseParser).ConfigureAwait(false);

            return response;
        }

        #endregion
        #region HTML

        internal PrtgResponse ExecuteRequest(IHtmlParameters parameters, Func<HttpResponseMessage, PrtgResponse> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

            var request = GetRequestMessage(parameters);

            var response = ExecuteRequest(request, token, responseParser);

            return response;
        }

        internal async Task<PrtgResponse> ExecuteRequestAsync(IHtmlParameters parameters, Func<HttpResponseMessage, Task<PrtgResponse>> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

            var request = GetRequestMessage(parameters);

            var response = await ExecuteRequestAsync(request, token, responseParser).ConfigureAwait(false);

            return response;
        }

        internal XElement ExecuteRequest(IHtmlParameters parameters, Func<PrtgResponse, XElement> xmlParser, CancellationToken token = default(CancellationToken))
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

            var response = ExecuteRequest(parameters, token: token);

            return xmlParser(response);
        }

        internal async Task<XElement> ExecuteRequestAsync(IHtmlParameters parameters, Func<PrtgResponse, XElement> xmlParser, CancellationToken token = default(CancellationToken))
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters), "Parameters cannot be null.");

            var response = await ExecuteRequestAsync(parameters, token: token).ConfigureAwait(false);

            return xmlParser(response);
        }

        #endregion

        private PrtgResponse ExecuteMultiRequest(Func<IParameters, PrtgRequestMessage> getRequestMessage, IMultiTargetParameters parameters, CancellationToken token, Func<HttpResponseMessage, PrtgResponse> responseParser = null)
        {
            var allIds = parameters.ObjectIds;

            try
            {
                for (int i = 0; i < allIds.Length; i += BatchLimit)
                {
                    parameters.ObjectIds = allIds.Skip(i).Take(BatchLimit).ToArray();

                    ExecuteRequest(getRequestMessage(parameters), token, responseParser);
                }
            }
            finally
            {
                parameters.ObjectIds = allIds;
            }

            return string.Empty;
        }

        internal async Task<PrtgResponse> ExecuteMultiRequestAsync(Func<IParameters, PrtgRequestMessage> getRequestMessage, IMultiTargetParameters parameters, CancellationToken token, Func<HttpResponseMessage, Task<PrtgResponse>> responseParser = null)
        {
            var allIds = parameters.ObjectIds;

            try
            {
                for (int i = 0; i < allIds.Length; i += BatchLimit)
                {
                    parameters.ObjectIds = allIds.Skip(i).Take(BatchLimit).ToArray();

                    await ExecuteRequestAsync(getRequestMessage(parameters), token, responseParser).ConfigureAwait(false);
                }
            }
            finally
            {
                parameters.ObjectIds = allIds;
            }

            return string.Empty;
        }

        private PrtgResponse ExecuteRequest(PrtgRequestMessage request, CancellationToken token, Func<HttpResponseMessage, PrtgResponse> responseParser = null)
        {
            prtgClient.Log($"Synchronously executing request {request}", LogLevel.Request);

            int retriesRemaining = prtgClient.RetryCount;

            do
            {
                try
                {
                    if (token == CancellationToken.None && DefaultCancellationToken != CancellationToken.None)
                        token = DefaultCancellationToken;

                    var responseMessage = webClient.SendSync(request, token).Result;

                    PrtgResponse responseContent = null;

                    if (responseParser != null)
                        responseContent = responseParser(responseMessage);

                    if (responseContent == null)
                        responseContent = GetAppropriateResponse(responseMessage, prtgClient.LogLevel);

                    //If we needed to log response, GetAppropriateResponse will have handled this
                    if (responseContent.Type == PrtgResponseType.String)
                        prtgClient.Log(responseContent.StringValue, LogLevel.Response);

                    ValidateHttpResponse(responseMessage, responseContent);

                    return responseContent;
                }
                catch (Exception ex) when (ex is AggregateException || ex is TaskCanceledException || ex is HttpRequestException)
                {
                    var innerException = ex.InnerException;

                    if (innerException is TaskCanceledException)
                    {
                        //If the token we specified was cancelled, throw the TaskCanceledException
                        if (token.IsCancellationRequested)
                            throw innerException;

                        //Otherwise, a token that was created internally for use with timing out was cancelled, so throw a timeout exception
                        innerException = new TimeoutException($"The server timed out while executing request.", ex.InnerException);
                    }

                    var result = HandleRequestException(ex.InnerException?.InnerException, innerException, request, ref retriesRemaining, () =>
                    {
                        if (innerException != null)
                            throw innerException;
                    }, token);

                    if (!result)
                        throw;
                }
            } while (true); //Keep retrying as long as we have retries remaining
        }

        private async Task<PrtgResponse> ExecuteRequestAsync(PrtgRequestMessage request, CancellationToken token, Func<HttpResponseMessage, Task<PrtgResponse>> responseParser = null)
        {
            prtgClient.Log($"Asynchronously executing request {request}", LogLevel.Request);

            int retriesRemaining = prtgClient.RetryCount;

            do
            {
                try
                {
                    if (token == CancellationToken.None && DefaultCancellationToken != CancellationToken.None)
                        token = DefaultCancellationToken;

                    var responseMessage = await webClient.SendAsync(request, token).ConfigureAwait(false);

                    PrtgResponse responseContent = null;

                    if (responseParser != null)
                        responseContent = await responseParser(responseMessage).ConfigureAwait(false);

                    if (responseContent == null)
                        responseContent = await GetAppropriateResponseAsync(responseMessage, prtgClient.LogLevel).ConfigureAwait(false);

                    //If we needed to log response, GetAppropriateResponseAsync will have handled this
                    if (responseContent.Type == PrtgResponseType.String)
                        prtgClient.Log(responseContent.StringValue, LogLevel.Response);

                    ValidateHttpResponse(responseMessage, responseContent);

                    return responseContent;
                }
                catch (HttpRequestException ex)
                {
                    var inner = ex.InnerException as WebException;

                    var result = HandleRequestException(inner, ex, request, ref retriesRemaining, null, token);

                    if (!result)
                        throw;
                }
                catch (TaskCanceledException ex)
                {
                    //If the token we specified was cancelled, throw the TaskCanceledException
                    if (token.IsCancellationRequested)
                        throw;

                    //Otherwise, a token that was created internally for use with timing out was cancelled, so throw a timeout exception
                    throw new TimeoutException($"The server timed out while executing request.", ex);
                }
            } while (true); //Keep retrying as long as we have retries remaining
        }

        private PrtgResponse GetAppropriateResponse(HttpResponseMessage message, LogLevel logLevel)
        {
            if (NeedsStringResponse(message, logLevel, IsDirty))
                return new PrtgResponse(message.Content.ReadAsStringAsync().Result, IsDirty);

            return new PrtgResponse(message.Content.ReadAsStreamAsync().Result);
        }

        private async Task<PrtgResponse> GetAppropriateResponseAsync(HttpResponseMessage message, LogLevel logLevel)
        {
            if (NeedsStringResponse(message, logLevel, IsDirty))
                return new PrtgResponse(await message.Content.ReadAsStringAsync().ConfigureAwait(false), IsDirty);

            return new PrtgResponse(await message.Content.ReadAsStreamAsync().ConfigureAwait(false));
        }

        internal static bool NeedsStringResponse(HttpResponseMessage message, LogLevel logLevel, bool isDirty)
        {
            return (logLevel & LogLevel.Response) == LogLevel.Response || isDirty || !PrtgResponse.IsSafeDataFormat(message);
        }

        private void HandleSocketException(Exception ex, PrtgRequestMessage request)
        {
            var socketException = ex?.InnerException as SocketException;

            if (socketException != null)
            {
                var port = socketException.Message.Substring(socketException.Message.LastIndexOf(':') + 1);

                var protocol = request.Url.Substring(0, request.Url.IndexOf(':')).ToUpper();

                if (socketException.SocketErrorCode == SocketError.TimedOut)
                {
                    throw new TimeoutException($"Connection timed out while communicating with remote server via {protocol} on port {port}. Confirm server address and port are valid and PRTG Service is running.", ex);
                }
                else if (socketException.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    throw new WebException($"Server rejected {protocol} connection on port {port}. Please confirm expected server protocol and port, PRTG Core Service is running and that any SSL certificate is trusted.", ex);
                }
            }
        }

        private bool HandleRequestException(Exception innerMostEx, Exception fallbackHandlerEx, PrtgRequestMessage request, ref int retriesRemaining, Action thrower, CancellationToken token)
        {
            if (innerMostEx != null) //Synchronous + Asynchronous
            {
                if (retriesRemaining > 0)
                    prtgClient.HandleEvent(prtgClient.retryRequest, new RetryRequestEventArgs(innerMostEx, request.Url, retriesRemaining));
                else
                {
                    HandleSocketException(innerMostEx, request);

                    throw innerMostEx;
                }
            }
            else
            {
                if (fallbackHandlerEx != null && retriesRemaining > 0)
                    prtgClient.HandleEvent(prtgClient.retryRequest, new RetryRequestEventArgs(fallbackHandlerEx, request.Url, retriesRemaining));
                else
                {
                    thrower?.Invoke();

                    return false;
                }
            }

            if (retriesRemaining > 0)
            {
                var attemptsMade = prtgClient.RetryCount - retriesRemaining + 1;
                var delay = prtgClient.RetryDelay * attemptsMade;

                //Note that we don't have an asynchronous wait for the asynchronous version,
                //as this causes weird timing issues in tests like PrtgClient_RetriesWhileStreaming,
                //where the final retry occurs after StreamSensors has supposedly completed
                token.WaitHandle.WaitOne(delay * 1000);
            }

            retriesRemaining--;

            return true;
        }

        internal static void ValidateHttpResponse(HttpResponseMessage responseMessage, PrtgResponse response)
        {
            if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                var xDoc = XDocument.Load(response.ToXmlReader());
                var errorMessage = xDoc.Descendants("error").First().Value;

                throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {errorMessage.EnsurePeriod()}");
            }
            else if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new HttpRequestException("Could not authenticate to PRTG; the specified username and password were invalid.");
            }

            responseMessage.EnsureSuccessStatusCode();

            if (responseMessage.RequestMessage?.RequestUri?.AbsolutePath == "/error.htm")
            {
                var errorUrl = responseMessage.RequestMessage.RequestUri.ToString()
                    .Replace("&%2339;", "\""); //Strange text found when setting ChannelProperty.PercentMode in PRTG 18.4.47.1962

                var queries = UrlUtilities.CrackUrl(errorUrl);
                var errorMsg = queries["errormsg"];

                errorMsg = errorMsg.Replace("<br/><ul><li>", " ").Replace("</li></ul><br/>", " ");

                throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {errorMsg.EnsurePeriod()}");
            }

            if (response.Type == PrtgResponseType.String)
            {
                if (response.StringValue.StartsWith("<div class=\"errormsg\">")) //Example: GetProbeProperties specifying a content type of Probe instead of ProbeNode
                {
                    var msgEnd = response.StringValue.IndexOf("</h3>");

                    var substr = response.StringValue.Substring(0, msgEnd);

                    var substr1 = Regex.Replace(substr, "\\.</.+?><.+?>", ". ");

                    var regex = new Regex("<.+?>");
                    var newStr = regex.Replace(substr1, "");

                    throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {newStr.EnsurePeriod()}");
                }
            }
        }

        private PrtgRequestMessage GetRequestMessage(ICommandParameters parameters) =>
            new PrtgRequestMessage(prtgClient.ConnectionDetails, parameters.Function, parameters);

        private PrtgRequestMessage GetRequestMessage(ICsvParameters parameters) =>
            new PrtgRequestMessage(prtgClient.ConnectionDetails, parameters.Function, parameters);

        private PrtgRequestMessage GetRequestMessage(IHtmlParameters parameters) =>
            new PrtgRequestMessage(prtgClient.ConnectionDetails, parameters.Function, parameters);

        private PrtgRequestMessage GetRequestMessage(IJsonParameters parameters) =>
            new PrtgRequestMessage(prtgClient.ConnectionDetails, parameters.Function, parameters);

        private PrtgRequestMessage GetRequestMessage(IXmlParameters parameters) =>
            new PrtgRequestMessage(prtgClient.ConnectionDetails, parameters.Function, parameters);

        internal static void SetErrorUrlAsRequestUri(HttpResponseMessage response)
        {
            var url = response.RequestMessage.RequestUri.ToString();

            var searchText = "errorurl=";
            var expectedUrl = url.Substring(url.IndexOf(searchText, StringComparison.Ordinal) + searchText.Length);

            if (expectedUrl.EndsWith("%26"))
                expectedUrl = expectedUrl.Substring(0, expectedUrl.LastIndexOf("%26"));
            else if (expectedUrl.EndsWith("&"))
                expectedUrl = expectedUrl.Substring(0, expectedUrl.LastIndexOf("&"));

            searchText = "error.htm";

            var server = url.Substring(0, url.IndexOf(searchText));

            url = $"{server}public/login.htm?loginurl={expectedUrl}&errormsg=";

            response.RequestMessage.RequestUri = new Uri(url);
        }
    }
}
