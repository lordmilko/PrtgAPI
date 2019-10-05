using System;
using System.Linq;
using System.Xml.Linq;
using PrtgAPI.Parameters;
using PrtgAPI.Tests.UnitTests.Support.TestItems;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;
using PrtgAPI.Utilities;

namespace PrtgAPI.Tests.UnitTests.Support
{
    class AdvancedItemGenerator<TItem, TObject>
        where TItem : BaseItem
        where TObject : IObject
    {
        private Func<int, TItem> createItem;
        private Func<TItem[], IWebStreamResponse> createResponse;
        private Content content;
        private int count;
        private string[] columns;
        private string address;
        private bool async;
        private int offset;

        private MultiTypeResponse multiTypeResponse;

        public AdvancedItemGenerator(Func<int, TItem> createItem,
            Func<TItem[], IWebStreamResponse> createResponse,
            Content content,
            int count,
            string[] columns,
            string address,
            bool async,
            MultiTypeResponse response)
        {
            this.createItem = createItem;
            this.createResponse = createResponse;
            this.content = content;
            this.count = count;
            this.columns = columns;
            this.address = address;
            this.async = async;
            multiTypeResponse = response;
        }

        public IWebStreamResponse GetResponse()
        {
            offset = CalculateOffset();

            var items = GetItems(content, createItem, count);

            var response = createResponse(items);

            var filteredResponse = FilterColumns(response);

            return filteredResponse;
        }

        private int CalculateOffset()
        {
            if (address.Contains("filter_objid") && !address.Contains("filter_objid=@"))
            {
                var number = UrlUtilities.CrackUrl(address)["filter_objid"].Split(',').Select(v => Convert.ToInt32(v)).First();

                if (number >= 1000 && number < 5000)
                {
                    var offset = number % 1000;

                    return offset;
                }
            }

            return 0;
        }

        private TItem[] GetItems(Content content, Func<int, TItem> func, int count)
        {
            BaseItem[] items;
            TItem[] typedItems;

            if (multiTypeResponse.ItemOverride != null && multiTypeResponse.ItemOverride.TryGetValue(content, out items))
                typedItems = items.Cast<TItem>().ToArray();
            else
                typedItems = GetItems(func, count);

            return typedItems;
        }

        protected T[] GetItems<T>(Func<int, T> func, int count)
        {
            return Enumerable.Range(0, count).Select(v => func(v + offset)).ToArray();
        }

        private IWebStreamResponse FilterColumns(IWebStreamResponse response)
        {
            if (columns != null)
            {
                var defaultProperties = ContentParameters<TObject>.GetDefaultProperties();

                var defaultPropertiesStr = defaultProperties.Select(p => p.GetDescription().ToLower()).ToList();

                var missing = defaultPropertiesStr.Where(p => !columns.Contains(p)).ToList();

                if (missing.Count > 0)
                {
                    string responseStr;

                    if (async)
                        responseStr = response.GetResponseTextStream(address).Result;
                    else
                        responseStr = response.GetResponseText(ref address);

                    var xDoc = XDocument.Parse(responseStr);

                    var toRemove = xDoc.Descendants("item").Descendants().Where(
                        e =>
                        {
                            var str = e.Name.ToString();

                            if (str.EndsWith("_raw"))
                                str = str.Substring(0, str.Length - "_raw".Length);

                            return missing.Contains(str);
                        }).ToList();

                    foreach (var elm in toRemove)
                    {
                        elm.Remove();
                    }

                    return new BasicResponse(xDoc.ToString());
                }
            }

            return response;
        }
    }
}
