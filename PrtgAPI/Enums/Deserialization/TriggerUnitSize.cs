using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    enum TriggerUnitSize
    {
        [Description("bit")]
        Bit,

        [Description("kbit")]
        Kbit,
        Mbit,
        Gbit,
        Tbit,
        Byte,
        KByte,
        MByte,
        GByte,
        TByte
    }
}
