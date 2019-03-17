using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

namespace PrtgAPI.Utilities
{
    [ExcludeFromCodeCoverage]
    static class UrlUtilities
    {
        public static NameValueCollection CrackUrl(string url)
        {
            var start = url.IndexOf('?');

            if (start < 0)
                start = 0;

            var addr = url.Substring(start).Split('#')[0];

            var queries = ParseQueryString(addr);

            return queries;
        }

        //HttpClient.ParseQueryString
        public static NameValueCollection ParseQueryString(string query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (query.Length > 0 && query[0] == '?')
                query = query.Substring(1);

            var collection = new NameValueCollection();

            int l = query.Length;
            int i = 0;

            while (i < l)
            {
                // find next & while noting first = on the way (and if there are more)

                if (collection.Count > 1000)
                    throw new InvalidOperationException("Cannot have more than 1000 entries in query string.");

                int si = i;
                int ti = -1;

                while (i < l)
                {
                    char ch = query[i];

                    if (ch == '=')
                    {
                        if (ti < 0)
                            ti = i;
                    }
                    else if (ch == '&')
                    {
                        break;
                    }

                    i++;
                }

                // extract the name / value pair

                string name = null;
                string value = null;

                if (ti >= 0)
                {
                    name = query.Substring(si, ti - si);
                    value = query.Substring(ti + 1, i - ti - 1);
                }
                else
                {
                    value = query.Substring(si, i - si);
                }

                // add name / value pair to the collection
                collection.Add(
                    WebUtility.UrlDecode(name),
                    WebUtility.UrlDecode(value)
                );

                // trailing '&'
                if (i == l - 1 && query[i] == '&')
                    collection.Add(null, string.Empty);

                i++;
            }

            return collection;
        }

        //HttpValueCollection.ToString()
        public static string QueryCollectionToString(NameValueCollection collection)
        {
            int n = collection.Count;
            if (n == 0)
                return string.Empty;

            StringBuilder s = new StringBuilder();
            string key, keyPrefix, item;

            for (int i = 0; i < n; i++)
            {
                key = WebUtility.UrlEncode(collection.GetKey(i));
                keyPrefix = (key != null) ? (key + "=") : string.Empty;

                string[] values = collection.GetValues(i);

                if (s.Length > 0)
                    s.Append('&');

                if (values == null || values.Length == 0)
                {
                    s.Append(keyPrefix);
                }
                else if (values.Length == 1)
                {
                    s.Append(keyPrefix);
                    item = WebUtility.UrlEncode(values[0]);
                    s.Append(item);
                }
                else
                {
                    for (int j = 0; j < values.Length; j++)
                    {
                        if (j > 0)
                            s.Append('&');
                        s.Append(keyPrefix);
                        item = WebUtility.UrlEncode(values[j]);
                        s.Append(item);
                    }
                }
            }

            return s.ToString();
        }
    }
}
