using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    public enum ChannelProperty
    {
        //theres also now an autodiscover api call - /api/discovernow.htm?id=objectid 

        [Description("limitmaxerror")]
        UpperErrorLimit,

        [Description("limitmaxwarning")]
        UpperWarningLimit,

        [Description("limitminerror")]
        LowerErrorLimit,
            
        [Description("limitminwarning")]
        LowerWarningLimit,
        
        [Description("limiterrormsg")]
        ErrorLimitMessage,

        [Description("limitwarningmsg")]
        WarningLimitMessage
    }
}
