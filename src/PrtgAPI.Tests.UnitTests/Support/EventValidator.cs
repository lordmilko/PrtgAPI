using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.Support
{
    class EventValidator
    {
        private bool ready { get; set; }
        private int i { get; set; } = -1;

        private int count;

        private string[] list;

        private object lockObj = new object();

        public EventValidator(PrtgClient client, string[] list)
        {
            this.list = list;

            client.LogVerbose += (s, e) =>
            {
                var message = e.Message;

                if (message.StartsWith("Synchronously") || message.StartsWith("Asynchronously"))
                {
                    message = Regex.Replace(e.Message, "(.+ request )(.+)", "$2");
                }

                Assert.AreEqual(Get(message), message);
            };
        }

        public bool Finished => i == list.Length - 1;

        public void MoveNext(int count = 1)
        {
            i++;
            this.count = count;
            ready = true;
        }

        public string Get(string next)
        {
            lock (lockObj)
            {
                count--;

                if (ready)
                {
                    Assert.IsTrue(i <= list.Length - 1, $"More requests were received than stored in list. Next record is: {next}");

                    var val = list[i];

                    if (count == 0)
                        ready = false;
                    else
                        i++;

                    return val;
                }

                throw new InvalidOperationException($"Was not ready for request {i + 1} '{next}'");
            }
        }
    }
}
