using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PrtgAPI.Tests.UnitTests.Support
{
    class EventValidator<T>
    {
        private bool ready { get; set; }
        private int i { get; set; } = -1;

        private int count;

        private T[] list;

        private object lockObj = new object();

        public EventValidator(T[] list)
        {
            this.list = list;
        }

        public bool Finished => i == list.Length - 1;

        public void MoveNext(int count = 1)
        {
            i++;
            this.count = count;
            ready = true;
        }

        public T Get(string next)
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

                throw new InvalidOperationException($"Was not ready for request '{next}'");
            }
        }
    }
}
