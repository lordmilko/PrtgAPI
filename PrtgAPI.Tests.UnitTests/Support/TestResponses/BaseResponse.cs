using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public abstract class BaseResponse<T> : IWebStreamResponse
    {
        protected internal List<T> Items;
        private string rootName;

        protected BaseResponse(string rootName, T[] items)
        {
            this.rootName = rootName;
            Items = items.ToList();
        }

        public virtual string GetResponseText(ref string address)
        {
            return GetResponseText(ref address, Items);
        }

        private string GetResponseText(ref string address, List<T> list)
        {
            var queries = UrlUtilities.CrackUrl(address);

            List<XElement> xmlList;
            var count = list.Count;

            if (queries["count"] != null && queries["count"] != "*")
            {
                var c = Convert.ToInt32(queries["count"]);

                var streaming = queries["start"] != null && Convert.ToInt32(queries["start"]) % 500 == 0 || IsStreaming();
                var streamingLogs = queries["start"] != null && queries["content"] == "messages" && Convert.ToInt32(queries["start"]) % 500 == 1 || IsStreaming();

                if ((c < list.Count && c > 0 || c == 0 || !streaming && !streamingLogs) && !(queries["content"] == "messages" && c == 1) || IsStreaming())
                {
                    count = c;

                    var skip = 0;

                    if (queries["start"] != null)
                    {
                        skip = Convert.ToInt32(queries["start"]);

                        if (queries["content"] == "messages")
                            skip--;
                    }

                    xmlList = list.Skip(skip).Take(count).Select(GetItem).ToList();
                }
                else
                {
                    xmlList = list.Select(GetItem).ToList();
                }
            }
            else
                xmlList = list.Select(GetItem).ToList();

            var xml = new XElement(rootName,
                new XAttribute("listend", 1),
                new XAttribute("totalcount", list.Count),
                new XElement("prtg-version", "1.2.3.4"),
                xmlList
            );

            return xml.ToString();
        }

        public virtual async Task<string> GetResponseTextStream(string address)
        {
            var page = GetPageNumber(address);
            var delay = (Items.Count / 500) + 1 - page;

            var list = Items.Skip((page - 1)*500).Take(500).ToList();
            list.ForEach(i => ((BaseItem)(object)i).ObjId = page.ToString());

            await Task.Delay(delay * 600);
            var response = GetResponseText(ref address, list);

            return response;
        }

        private int GetPageNumber(string address)
        {
            var addr = address.Substring(address.IndexOf('?')).Split('#')[0];

            var queries = UrlUtilities.ParseQueryString(addr);

            if (queries["start"] == "0" || queries["start"] == null)
                return 1;
            else
                return Convert.ToInt32(queries["start"])/500 + 1;
        }

        protected virtual bool IsStreaming()
        {
            return false;
        }

        public abstract XElement GetItem(T item);
    }
}
