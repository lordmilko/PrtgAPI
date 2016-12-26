using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    public enum TriggerUnitTime
    {
        [Description("s")]
        Sec,

        [Description("m")]
        Min,

        [Description("h")]
        Hour,

        [Description("d")]
        Day
    }
}
