using System.Collections.Specialized;
using System.Web;

namespace PrtgAPI.Helpers
{
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
