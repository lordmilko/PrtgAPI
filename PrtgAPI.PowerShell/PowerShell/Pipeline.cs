using System.Collections.Generic;

namespace PrtgAPI.PowerShell
{
    class Pipeline
    {
        public object Current { get; }

        public List<object> List { get; }

        public int CurrentIndex => List.IndexOf(Current);

        public Pipeline(object current, List<object> list)
        {
            Current = current;
            List = list;
        }
    }
}
