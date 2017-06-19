using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
