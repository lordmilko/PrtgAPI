using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    public enum TriggerProperty
    {
        [Description("nodest")]
        StateTrigger,

        [Description("latency")]
        Latency,

        [Description("onnotificationid")]
        OnNotificationAction,

        [Description("offnotificationid")]
        OffNotificationAction,

        [Description("escnotificationid")]
        EscalationNotificationAction,

        [Description("esclatency")]
        EscalationLatency,

        [Description("repeatival")]
        RepeatInterval,

        [Description("channel")]
        Channel,

        [Description("period")]
        Period,

        [Description("unitsize")]
        UnitSize,

        [Description("condition")]
        Condition,

        [Description("threshold")]
        Threshold,

        [Description("unittime")]
        UnitTime
    }
}
