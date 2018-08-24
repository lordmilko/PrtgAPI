using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrtgAPI.PowerShell;

namespace PrtgAPI.Tests.UnitTests.ObjectData
{
    public abstract class BaseObjectTests<TObject, TItem, TResponse> : BaseTest where TResponse : IWebResponse
    {
        protected PrtgClient Initialize_Client_WithItems(params TItem[] items)
        {
            if (items.Length == 0)
                throw new ArgumentException("Length of items cannot be 0", nameof(items));

            var response = GetResponse(items);

            var client = Initialize_Client(response);

            return client;
        }

        protected abstract List<TObject> GetObjects(PrtgClient client);

        protected abstract Task<List<TObject>> GetObjectsAsync(PrtgClient client);

        protected TObject GetSingleItem()
        {
            var item = GetItem();
            var obj = GetItemsInternal(new[] {item}).FirstOrDefault();

            return obj;
        }

        protected async Task<TObject> GetSingleItemAsync()
        {
            var item = GetItem();
            var obj = (await GetItemsInternalAsync(new[] {item})).FirstOrDefault();

            return obj;
        }

        protected List<TObject> GetMultipleItems()
        {
            var items = GetItems();

            return GetItemsInternal(items);
        }

        protected async Task<List<TObject>> GetMultipleItemsAsync()
        {
            var items = GetItems();

            return await GetItemsInternalAsync(items);
        }

        private List<TObject> GetItemsInternal(TItem[] items)
        {
            var client = Initialize_Client_WithItems(items);

            var obj = GetObjects(client);

            return obj;
        }

        private async Task<List<TObject>> GetItemsInternalAsync(TItem[] items)
        {
            var client = Initialize_Client_WithItems(items);

            var obj = await GetObjectsAsync(client);

            return obj;
        }

        public void SetPrtgSessionState(params TItem[] items)
        {
            if (!items.Any())
            {
                PrtgSessionState.Client = Initialize_Client_WithItems(GetItems());
            }
            else
                PrtgSessionState.Client = Initialize_Client_WithItems(items);
        }

        public void SetPrtgSessionState(PrtgClient client)
        {
            PrtgSessionState.Client = client;
        }

        public abstract TItem GetItem();

        protected virtual TItem[] GetItems()
        {
            return new[] {GetItem()};
        }

        protected abstract TResponse GetResponse(TItem[] items);
    }
}
