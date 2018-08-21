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
            var start = url.IndexOf('?');

            if (start < 0)
                start = 0;

            var addr = url.Substring(start).Split('#')[0];

            var queries = HttpUtility.ParseQueryString(addr);

            return queries;
        }
    }
}
