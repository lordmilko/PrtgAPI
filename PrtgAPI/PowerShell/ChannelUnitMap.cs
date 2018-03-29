using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.PowerShell
{
    class ChannelUnitMap : Dictionary<string, string>
    {
        public int Impurity => Values.Count(v => v == null);
    }
}
