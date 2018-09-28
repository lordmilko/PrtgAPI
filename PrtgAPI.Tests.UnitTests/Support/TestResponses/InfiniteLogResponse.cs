using System;
using System.Linq;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class InfiniteLogResponse : MultiTypeResponse
    {
        Random random = new Random();

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            return new MessageResponse(Enumerable.Range(0, 50).Select(i =>
                new MessageItem($"Event_{random.NextDouble()}")
            ).ToArray());
        }
    }
}
