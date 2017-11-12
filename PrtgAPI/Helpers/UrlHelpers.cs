using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace PrtgAPI.Helpers
{
    [ExcludeFromCodeCoverage]
    static class UrlHelpers
    {
        public static NameValueCollection CrackUrl(string url)
        {
            var addr = url.Substring(url.IndexOf('?')).Split('#')[0];

            var queries = HttpUtility.ParseQueryString(addr);

            return queries;
        }
    }
}
