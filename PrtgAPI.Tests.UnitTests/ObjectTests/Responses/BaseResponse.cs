using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;
using PrtgAPI.Tests.UnitTests.ObjectTests.Items;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Responses
{
    public abstract class BaseResponse<T> : IWebResponse
    {
        protected List<T> items;
        private string rootName;

        protected BaseResponse(string rootName, T[] items)
        {
            this.rootName = rootName;
            this.items = items.ToList();
        }

        public virtual string GetResponseText(string address)
        {
            return GetResponseText(address, items);
        }

        private string GetResponseText(string address, List<T> list)
        {
            var xmlList = list.Select(GetItem).ToList();

            var xml = new XElement(rootName,
                new XAttribute("listend", 1),
                new XAttribute("totalcount", list.Count),
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

            await Task.Delay(delay * 1000);
            var response = GetResponseText(address, list);

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
