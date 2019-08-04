using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Utilities;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    public class InfiniteLogValidatorResponse : MultiTypeResponse
    {
        protected DateTime startDate;
        protected string filters;

        private int requestNum;

        public InfiniteLogValidatorResponse(DateTime startDate, string filters = "start=1")
        {
            this.startDate = startDate;
            this.filters = filters;
        }

        protected override IWebResponse GetResponse(ref string address, string function)
        {
            requestNum++;

            switch(requestNum)
            {
                case 1: //Initial request for items using the current date and time. Return none.
                    AssertFirst(address);
                    return new MessageResponse();

                case 2: //Return three items, one hour ahead of the start time, spaced one minute apart
                    AssertSecond(address);

                    //PRTG returns objects newest to oldest. InfiniteLogIterator reverses these, so its oldest to newest
                    return new MessageResponse(
                        new MessageItem("Item 3", datetimeRaw: OADate(startDate.AddHours(1).AddMinutes(2))),
                        new MessageItem("Item 2", datetimeRaw: OADate(startDate.AddHours(1).AddMinutes(1)), statusRaw: "612"),
                        new MessageItem("Item 1", datetimeRaw: OADate(startDate.AddHours(1)))                        
                    );

                case 3:
                    AssertThird(address);
                    return new MessageResponse();

                case 4:
                    AssertFourth(address);
                    return new MessageResponse(
                        new MessageItem("Item 5", datetimeRaw: OADate(startDate.AddHours(1).AddMinutes(6))),
                        new MessageItem("Item 4", datetimeRaw: OADate(startDate.AddHours(1).AddMinutes(5)))
                    );

                case 5:
                    AssertFifth(address);
                    return new MessageResponse(
                        new MessageItem("Item 8", datetimeRaw: OADate(startDate.AddHours(1).AddMinutes(11))),
                        new MessageItem("Item 7", datetimeRaw: OADate(startDate.AddHours(1).AddMinutes(10)), statusRaw: "613"),
                        new MessageItem("Item 6", datetimeRaw: OADate(startDate.AddHours(1).AddMinutes(9)))
                    );

                default:
                    throw new NotImplementedException($"Don't know how to handle request #{requestNum}: {address}");
            }
        }

        protected virtual void AssertFirst(string address)
        {
            Assert.AreEqual(UnitRequest.Logs($"{filters}&filter_dstart={LogDate(startDate)}"), address);
        }

        protected virtual void AssertSecond(string address)
        {
            AssertFirst(address);
        }

        protected virtual void AssertThird(string address)
        {
            Assert.AreEqual(UnitRequest.Logs($"{filters}&filter_dstart={LogDate(startDate.AddHours(1).AddMinutes(2))}"), address);
        }

        protected virtual void AssertFourth(string address)
        {
            AssertThird(address);
        }

        protected virtual void AssertFifth(string address)
        {
            Assert.AreEqual(UnitRequest.Logs($"{filters}&filter_dstart={LogDate(startDate.AddHours(1).AddMinutes(6))}"), address);
        }

        protected string LogDate(DateTime date)
        {
            return TypeHelpers.DateToString(date);
        }

        private string OADate(DateTime date)
        {
            return TypeHelpers.ConvertToPrtgDateTime(date).ToString();
        }
    }
}
