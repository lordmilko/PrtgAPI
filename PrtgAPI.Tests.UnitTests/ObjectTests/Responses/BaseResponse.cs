using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using PrtgAPI.Helpers;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.Items;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Responses
{
    public abstract class BaseResponse<T> : IWebResponse
    {
        protected List<T> items;
        private string rootName;

        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        protected BaseResponse(string rootName, T[] items)
        {
            this.rootName = rootName;
            this.items = items.ToList();
        }

        public virtual string GetResponseText(ref string address)
        {
            return GetResponseText(ref address, items);
        }

        private string GetResponseText(ref string address, List<T> list)
        {
            var queries = UrlHelpers.CrackUrl(address);

            List<XElement> xmlList = null;
            var count = list.Count;

            if (queries["count"] != null)
            {
                var c = Convert.ToInt32(queries["count"]);

                if (c < list.Count && c > 0)
                {
                    count = c;

                    xmlList = list.Take(count).Select(GetItem).ToList();
                }
                else
                    xmlList = list.Select(GetItem).ToList();
            }
            else
                xmlList = list.Select(GetItem).ToList();

            var xml = new XElement(rootName,
                new XAttribute("listend", 1),
                new XAttribute("totalcount", count),
                new XElement("prtg-version", "1.2.3.4"),
                xmlList
            );

            return xml.ToString();
        }

        //public abstract Task<string> GetResponseTextAsync(string address);

        //public async Task<string> GetResponseTextAsync(string address, Action<T, string> action)
        public virtual async Task<string> GetResponseTextStream(string address)
        {
            var page = GetPageNumber(address);
            var delay = (items.Count / 500) + 1 - page;

            var list = items.Skip((page - 1)*500).Take(500).ToList();
            list.ForEach(i => ((BaseItem)(object)i).ObjId = page.ToString());

            await Task.Delay(delay * 600);
            var response = GetResponseText(ref address, list);

            return response;
        }

        private int GetPageNumber(string address)
        {
            var addr = address.Substring(address.IndexOf('?')).Split('#')[0];

            var queries = HttpUtility.ParseQueryString(addr);

            if (queries["start"] == "0" || queries["start"] == null)
                return 1;
            else
                return Convert.ToInt32(queries["start"])/Convert.ToInt32(queries["count"]) + 1;
        }

        public abstract XElement GetItem(T item);
    }
}
