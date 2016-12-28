using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PrtgAPI.Tests.UnitTests.InfrastructureTests.Support;

namespace PrtgAPI.Tests.UnitTests.ObjectTests.Support
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
            var xml = new XElement(rootName,
                new XAttribute("listend", 1),
                new XAttribute("totalcount", 21),
                new XElement("prtg-version", "1.2.3.4"),
                items.Select(GetItem)
            );

            return xml.ToString();
        }

        public abstract XElement GetItem(T item);
    }
}
