using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Helpers;
using PrtgAPI.Parameters;

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

        internal RequestEngine(PrtgClient prtgClient, IWebClient webClient)
        {
            this.prtgClient = prtgClient;
            this.webClient = webClient;
        }

        #region JSON + Response Parser

        internal string ExecuteRequest(IJsonParameters parameters, Func<HttpResponseMessage, string> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            var url = GetPrtgUrl(parameters);

            var response = ExecuteRequest(url, token, responseParser);

            return response;
        }

        internal async Task<string> ExecuteRequestAsync(IJsonParameters parameters, Func<HttpResponseMessage, Task<string>> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            var url = GetPrtgUrl(parameters);

            var response = await ExecuteRequestAsync(url, token, responseParser).ConfigureAwait(false);

            return response;
        }

        #endregion
        #region XML + Response Validator / Response Parser

        internal XDocument ExecuteRequest(IXmlParameters parameters, Action<string> responseValidator = null, Func<HttpResponseMessage, string> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            var url = GetPrtgUrl(parameters);

            var response = ExecuteRequest(url, token, responseParser);

            responseValidator?.Invoke(response);

            return XDocumentHelpers.SanitizeXml(response);
        }

        internal async Task<XDocument> ExecuteRequestAsync(IXmlParameters parameters, Action<string> responseValidator = null, Func<HttpResponseMessage, Task<string>> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            var url = GetPrtgUrl(parameters);

            var response = await ExecuteRequestAsync(url, token, responseParser).ConfigureAwait(false);

            responseValidator?.Invoke(response);

            return XDocumentHelpers.SanitizeXml(response);
        }

        #endregion
        #region Command + Response Parser

        internal string ExecuteRequest(ICommandParameters parameters, Func<HttpResponseMessage, string> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            if (parameters is IMultiTargetParameters)
                return ExecuteMultiRequest(p => GetPrtgUrl((ICommandParameters)p), (IMultiTargetParameters)parameters, token, responseParser);

            var url = GetPrtgUrl(parameters);

            var response = ExecuteRequest(url, token, responseParser);

            return response;
        }

        internal async Task<string> ExecuteRequestAsync(ICommandParameters parameters, Func<HttpResponseMessage, Task<string>> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            if (parameters is IMultiTargetParameters)
                return await ExecuteMultiRequestAsync(p => GetPrtgUrl((ICommandParameters)p), (IMultiTargetParameters)parameters, token, responseParser).ConfigureAwait(false);

            var url = GetPrtgUrl(parameters);

            var response = await ExecuteRequestAsync(url, token, responseParser).ConfigureAwait(false);

            return response;
        }

        #endregion
        #region HTML

        internal string ExecuteRequest(IHtmlParameters parameters, Func<HttpResponseMessage, string> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            var url = GetPrtgUrl(parameters);

            var response = ExecuteRequest(url, token, responseParser);

            return response;
        }

        internal async Task<string> ExecuteRequestAsync(IHtmlParameters parameters, Func<HttpResponseMessage, Task<string>> responseParser = null, CancellationToken token = default(CancellationToken))
        {
            var url = GetPrtgUrl(parameters);

            var response = await ExecuteRequestAsync(url, token, responseParser).ConfigureAwait(false);

            return response;
        }

        internal XElement ExecuteRequest(IHtmlParameters parameters, Func<string, XElement> xmlParser, CancellationToken token = default(CancellationToken))
        {
            var response = ExecuteRequest(parameters, token: token);

            return xmlParser(response);
        }

        internal async Task<XElement> ExecuteRequestAsync(IHtmlParameters parameters, Func<string, XElement> xmlParser, CancellationToken token = default(CancellationToken))
        {
            var response = await ExecuteRequestAsync(parameters, token: token).ConfigureAwait(false);

            return xmlParser(response);
        }

        #endregion

        private string ExecuteMultiRequest(Func<IParameters, PrtgUrl> getUrl, IMultiTargetParameters parameters, CancellationToken token, Func<HttpResponseMessage, string> responseParser = null)
        {
            var allIds = parameters.ObjectIds;

            try
            {
                for (int i = 0; i < allIds.Length; i += BatchLimit)
                {
                    parameters.ObjectIds = allIds.Skip(i).Take(BatchLimit).ToArray();

                    ExecuteRequest(getUrl(parameters), token, responseParser);
                }
            }
            finally
            {
                parameters.ObjectIds = allIds;
            }

            return string.Empty;
        }

        internal async Task<string> ExecuteMultiRequestAsync(Func<IParameters, PrtgUrl> getUrl, IMultiTargetParameters parameters, CancellationToken token, Func<HttpResponseMessage, Task<string>> responseParser = null)
        {
            var allIds = parameters.ObjectIds;

            try
            {
                for (int i = 0; i < allIds.Length; i += BatchLimit)
                {
                    parameters.ObjectIds = allIds.Skip(i).Take(BatchLimit).ToArray();

                    await ExecuteRequestAsync(getUrl(parameters), token, responseParser).ConfigureAwait(false);
                }
            }
            finally
            {
                parameters.ObjectIds = allIds;
            }

            return string.Empty;
        }

        private string ExecuteRequest(PrtgUrl url, CancellationToken token, Func<HttpResponseMessage, string> responseParser = null)
        {
            prtgClient.Log($"Synchronously executing request {url.Url}", LogLevel.Request);

            int retriesRemaining = prtgClient.RetryCount;

            do
            {
                try
                {
                    if (token == CancellationToken.None && DefaultCancellationToken != CancellationToken.None)
                        token = DefaultCancellationToken;

                    var response = webClient.GetSync(url.Url, token).Result;

                    string responseText = null;

                    if (responseParser != null)
                        responseText = responseParser(response);

                    if (responseText == null)
                        responseText = response.Content.ReadAsStringAsync().Result;

                    prtgClient.Log(responseText, LogLevel.Response);

                    ValidateHttpResponse(response, responseText);

                    return responseText;
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

                    var result = HandleRequestException(ex.InnerException?.InnerException, innerException, url, ref retriesRemaining, () =>
                    {
                        if (innerException != null)
                            throw innerException;
                    });

                    if (!result)
                        throw;
                }
            } while (true);
        }

        private async Task<string> ExecuteRequestAsync(PrtgUrl url, CancellationToken token, Func<HttpResponseMessage, Task<string>> responseParser = null)
        {
            prtgClient.Log($"Asynchronously executing request {url.Url}", LogLevel.Request);

            int retriesRemaining = prtgClient.RetryCount;

            do
            {
                try
                {
                    if (token == CancellationToken.None && DefaultCancellationToken != CancellationToken.None)
                        token = DefaultCancellationToken;

                    var response = await webClient.GetAsync(url.Url, token).ConfigureAwait(false);

                    string responseText = null;

                    if (responseParser != null)
                        responseText = await responseParser(response).ConfigureAwait(false);

                    if (responseText == null)
                        responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    prtgClient.Log(responseText, LogLevel.Response);

                    ValidateHttpResponse(response, responseText);

                    return responseText;
                }
                catch (HttpRequestException ex)
                {
                    var inner = ex.InnerException as WebException;

                    var result = HandleRequestException(inner, ex, url, ref retriesRemaining, null);

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
            } while (true);
        }

        private void HandleSocketException(Exception ex, string url)
        {
            var socketException = ex?.InnerException as SocketException;

            if (socketException != null)
            {
                var port = socketException.Message.Substring(socketException.Message.LastIndexOf(':') + 1);

                var protocol = url.Substring(0, url.IndexOf(':')).ToUpper();

                if (socketException.SocketErrorCode == SocketError.TimedOut)
                {
                    throw new TimeoutException($"Connection timed out while communicating with remote server via {protocol} on port {port}. Confirm server address and port are valid and PRTG Service is running", ex);
                }
                else if (socketException.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    throw new WebException($"Server rejected {protocol} connection on port {port}. Please confirm expected server protocol and port, PRTG Core Service is running and that any SSL certificate is trusted", ex);
                }
            }
        }

        private bool HandleRequestException(Exception innerMostEx, Exception fallbackHandlerEx, PrtgUrl url, ref int retriesRemaining, Action thrower)
        {
            if (innerMostEx != null) //Synchronous + Asynchronous
            {
                if (retriesRemaining > 0)
                    prtgClient.HandleEvent(prtgClient.retryRequest, new RetryRequestEventArgs(innerMostEx, url.Url, retriesRemaining));
                else
                {
                    HandleSocketException(innerMostEx, url.Url);

                    throw innerMostEx;
                }
            }
            else
            {
                if (fallbackHandlerEx != null && retriesRemaining > 0)
                    prtgClient.HandleEvent(prtgClient.retryRequest, new RetryRequestEventArgs(fallbackHandlerEx, url.Url, retriesRemaining));
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

                Thread.Sleep(delay * 1000);
            }

            retriesRemaining--;

            return true;
        }

        private void ValidateHttpResponse(HttpResponseMessage response, string responseText)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var xDoc = XDocument.Parse(responseText);
                var errorMessage = xDoc.Descendants("error").First().Value;

                throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {errorMessage}");
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new HttpRequestException("Could not authenticate to PRTG; the specified username and password were invalid.");
            }

            response.EnsureSuccessStatusCode();

            if (response.RequestMessage?.RequestUri?.AbsolutePath == "/error.htm")
            {
                var errorUrl = response.RequestMessage.RequestUri.ToString();

                var queries = UrlHelpers.CrackUrl(errorUrl);
                var errorMsg = queries["errormsg"];

                errorMsg = errorMsg.Replace("<br/><ul><li>", " ").Replace("</li></ul><br/>", " ");

                throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {errorMsg}");
            }

            if (responseText.StartsWith("<div class=\"errormsg\">")) //Example: GetProbeProperties specifying a content type of Probe instead of ProbeNode
            {
                var msgEnd = responseText.IndexOf("</h3>");

                var substr = responseText.Substring(0, msgEnd);

                var substr1 = Regex.Replace(substr, "\\.</.+?><.+?>", ". ");

                var regex = new Regex("<.+?>");
                var newStr = regex.Replace(substr1, "");

                throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {newStr}");
            }
        }

        private PrtgUrl GetPrtgUrl(ICommandParameters parameters) =>
            new PrtgUrl(prtgClient.ConnectionDetails, parameters.Function, parameters);

        private PrtgUrl GetPrtgUrl(IHtmlParameters parameters) =>
            new PrtgUrl(prtgClient.ConnectionDetails, parameters.Function, parameters);

        private PrtgUrl GetPrtgUrl(IJsonParameters parameters) =>
            new PrtgUrl(prtgClient.ConnectionDetails, parameters.Function, parameters);

        private PrtgUrl GetPrtgUrl(IXmlParameters parameters) =>
            new PrtgUrl(prtgClient.ConnectionDetails, parameters.Function, parameters);

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
