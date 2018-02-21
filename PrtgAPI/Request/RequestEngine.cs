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
    class RequestEngine
    {
        private IWebClient webClient;
        private PrtgClient prtgClient;

        private const int BatchLimit = 1500;

        internal RequestEngine(PrtgClient prtgClient, IWebClient webClient)
        {
            this.prtgClient = prtgClient;
            this.webClient = webClient;
        }

        #region JSON + Response Parser

        internal string ExecuteRequest(JsonFunction function, IParameters parameters, Func<HttpResponseMessage, string> responseParser = null)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = ExecuteRequest(url, responseParser);

            return response;
        }

        internal async Task<string> ExecuteRequestAsync(JsonFunction function, IParameters parameters, Func<HttpResponseMessage, Task<string>> responseParser = null)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = await ExecuteRequestAsync(url, responseParser).ConfigureAwait(false);

            return response;
        }

        #endregion
        #region XML + Response Validator

        internal XDocument ExecuteRequest(XmlFunction function, Parameters.Parameters parameters, Action<string> responseValidator = null)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = ExecuteRequest(url);

            responseValidator?.Invoke(response);

            return XDocumentHelpers.SanitizeXml(response);
        }

        internal async Task<XDocument> ExecuteRequestAsync(XmlFunction function, Parameters.Parameters parameters, Action<string> responseValidator = null)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = await ExecuteRequestAsync(url).ConfigureAwait(false);

            responseValidator?.Invoke(response);

            return XDocumentHelpers.SanitizeXml(response);
        }

        #endregion
        #region Command + Response Parser

        internal string ExecuteRequest(CommandFunction function, IParameters parameters, Func<HttpResponseMessage, string> responseParser = null)
        {
            if (parameters is IMultiTargetParameters)
                return ExecuteMultiRequest(p => GetPrtgUrl(function, p), (IMultiTargetParameters)parameters, responseParser);

            var url = GetPrtgUrl(function, parameters);

            var response = ExecuteRequest(url, responseParser);

            return response;
        }

        internal async Task<string> ExecuteRequestAsync(CommandFunction function, IParameters parameters, Func<HttpResponseMessage, Task<string>> responseParser = null)
        {
            if (parameters is IMultiTargetParameters)
                return await ExecuteMultiRequestAsync(p => GetPrtgUrl(function, p), (IMultiTargetParameters)parameters, responseParser).ConfigureAwait(false);

            var url = GetPrtgUrl(function, parameters);

            var response = await ExecuteRequestAsync(url, responseParser).ConfigureAwait(false);

            return response;
        }

        #endregion
        #region HTML

        internal string ExecuteRequest(HtmlFunction function, Parameters.Parameters parameters, Func<HttpResponseMessage, string> responseParser = null)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = ExecuteRequest(url, responseParser);

            return response;
        }

        internal async Task<string> ExecuteRequestAsync(HtmlFunction function, Parameters.Parameters parameters, Func<HttpResponseMessage, Task<string>> responseParser = null)
        {
            var url = GetPrtgUrl(function, parameters);

            var response = await ExecuteRequestAsync(url, responseParser);

            return response;
        }

        #endregion


        private string ExecuteMultiRequest(Func<IParameters, PrtgUrl> getUrl, IMultiTargetParameters parameters, Func<HttpResponseMessage, string> responseParser = null)
        {
            var allIds = parameters.ObjectIds;

            try
            {
                for (int i = 0; i < allIds.Length; i += BatchLimit)
                {
                    parameters.ObjectIds = allIds.Skip(i).Take(BatchLimit).ToArray();

                    ExecuteRequest(getUrl(parameters), responseParser);
                }
            }
            finally
            {
                parameters.ObjectIds = allIds;
            }

            return string.Empty;
        }

        internal async Task<string> ExecuteMultiRequestAsync(Func<IParameters, PrtgUrl> getUrl, IMultiTargetParameters parameters, Func<HttpResponseMessage, Task<string>> responseParser = null)
        {
            var allIds = parameters.ObjectIds;

            try
            {
                for (int i = 0; i < allIds.Length; i += BatchLimit)
                {
                    parameters.ObjectIds = allIds.Skip(i).Take(BatchLimit).ToArray();

                    await ExecuteRequestAsync(getUrl(parameters), responseParser).ConfigureAwait(false);
                }
            }
            finally
            {
                parameters.ObjectIds = allIds;
            }

            return string.Empty;
        }


        private string ExecuteRequest(PrtgUrl url, Func<HttpResponseMessage, string> responseParser = null)
        {
            prtgClient.Log($"Synchronously executing request {url.Url}");

            int retriesRemaining = prtgClient.RetryCount;

            do
            {
                try
                {
                    var response = webClient.GetSync(url.Url).Result;

                    string responseText = null;

                    if (responseParser != null)
                        responseText = responseParser(response);

                    if (responseText == null)
                        responseText = response.Content.ReadAsStringAsync().Result;

                    ValidateHttpResponse(response, responseText);

                    return responseText;
                }
                catch (Exception ex) when (ex is AggregateException || ex is TaskCanceledException || ex is HttpRequestException)
                {
                    var innerException = ex.InnerException;

                    if (ex.InnerException is TaskCanceledException)
                        innerException = new TimeoutException($"The server timed out while executing request.", ex.InnerException);

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

        private async Task<string> ExecuteRequestAsync(PrtgUrl url, Func<HttpResponseMessage, Task<string>> responseParser = null)
        {
            prtgClient.Log($"Asynchronously executing request {url.Url}");

            int retriesRemaining = prtgClient.RetryCount;

            do
            {
                try
                {
                    var response = await webClient.GetAsync(url.Url).ConfigureAwait(false);

                    string responseText = null;

                    if (responseParser != null)
                        responseText = await responseParser(response).ConfigureAwait(false);

                    if (responseText == null)
                        responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

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

        private PrtgUrl GetPrtgUrl(CommandFunction function, IParameters parameters) =>
            new PrtgUrl(prtgClient.connectionDetails, function, parameters);

        private PrtgUrl GetPrtgUrl(HtmlFunction function, IParameters parameters) =>
            new PrtgUrl(prtgClient.connectionDetails, function, parameters);

        private PrtgUrl GetPrtgUrl(JsonFunction function, IParameters parameters) =>
            new PrtgUrl(prtgClient.connectionDetails, function, parameters);

        private PrtgUrl GetPrtgUrl(XmlFunction function, IParameters parameters) =>
            new PrtgUrl(prtgClient.connectionDetails, function, parameters);

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
