using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    public enum TriggerCondition
    {
        [Description("above")]
        Above,

        [Description("below")]
        Below,

        [Description("Equal to")]
        Equals,

        [Description("Not Equal to")]
        NotEquals,

        [Description("change")]
        Change //for use in "change" triggers only
    }
}
