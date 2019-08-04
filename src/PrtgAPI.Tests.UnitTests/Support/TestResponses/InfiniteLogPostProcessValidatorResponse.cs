using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class InfiniteLogPostProcessValidatorResponse : InfiniteLogValidatorResponse
    {
        public InfiniteLogPostProcessValidatorResponse(DateTime startDate, string filters = "start=1") : base(startDate, filters)
        {
        }

        protected override void AssertThird(string address)
        {
            //We did a client side filter for the second item after request 2, so the latest Current is the second item, not the third
            Assert.AreEqual(UnitRequest.Logs($"{filters}&filter_dstart={LogDate(startDate.AddHours(1).AddMinutes(1))}"), address);
        }

        protected override void AssertFifth(string address)
        {
            //No objects in the previous request matched our post process filter, so the log iterator's Current Date hasn't changed
            AssertThird(address);
        }
    }
}
