using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Utilities;

namespace PrtgAPI
{
#pragma warning disable CS0649

    [DataContract]
    class NotificationTriggerData
    {
        [DataMember(Name = "supported")]
        private string[] supportedTypes;

        public TriggerType[] SupportedTypes => supportedTypes.Select(t => t.ToEnum<TriggerType>()).ToArray();

        [ExcludeFromCodeCoverage]
        [DataMember(Name = "data")]
        public NotificationTriggerDataTrigger[] Triggers { get; set; }

        [ExcludeFromCodeCoverage]
        [DataMember(Name = "readonly")]
        public bool ReadOnly { get; set; }
    }

    [DataContract]
    [DebuggerDisplay("Type = {Type}, SubId = {SubId}, OnNotificationAction = {OnNotificationAction}")]
    class NotificationTriggerDataTrigger
    {
        [DataMember(Name = "type")]
        string Type { get; set; }

        [DataMember(Name = "typename")]
        string TypeName { get; set; }

        [DataMember(Name = "subid")]
        internal int SubId { get; set; }

        [DataMember(Name = "nodest")]
        [PropertyParameter(TriggerProperty.State)]
        string StateTrigger { get; set; }

        [DataMember(Name = "nodest_input")]
        string StateTriggerInput { get; set; }

        [DataMember(Name = "latency")]
        [PropertyParameter(TriggerProperty.Latency)]
        int? Latency { get; set; }

        [DataMember(Name = "latency_input")]
        string LatencyInput { get; set; }

        [DataMember(Name = "onnotificationid")]
        [PropertyParameter(TriggerProperty.OnNotificationAction)]
        string OnNotificationAction { get; set; }

        [DataMember(Name = "onnotificationid_input")]
        string OnNotificationActionInput { get; set; }

        [DataMember(Name = "offnotificationid")]
        [PropertyParameter(TriggerProperty.OffNotificationAction)]
        string OffNotificationAction { get; set; }

        [DataMember(Name = "offnotificationid_input")]
        string OffNotificationActionInput { get; set; }

        [DataMember(Name = "esclatency")]
        [PropertyParameter(TriggerProperty.EscalationLatency)]
        int? EscalationLatency { get; set; }

        [DataMember(Name = "esclatency_input")]
        string EscalationLatencyInput { get; set; }

        [DataMember(Name = "escnotificationid")]
        [PropertyParameter(TriggerProperty.EscalationNotificationAction)]
        string EscalationNotificationAction { get; set; }

        [DataMember(Name = "escnotificationid_input")]
        string EscalationNotificationActionInput { get; set; }

        [DataMember(Name = "repeatival")]
        [PropertyParameter(TriggerProperty.RepeatInterval)]
        int? RepeatInterval { get; set; }

        [DataMember(Name = "repeatival_input")]
        string RepeatIntervalInput { get; set; }

        [DataMember(Name = "channel")]
        [PropertyParameter(TriggerProperty.Channel)]
        string Channel { get; set; }

        [DataMember(Name = "channel_input")]
        internal string ChannelInput { get; set; }

        [DataMember(Name = "condition")]
        [PropertyParameter(TriggerProperty.Condition)]
        string Condition { get; set; }

        [DataMember(Name = "condition_input")]
        string ConditionInput { get; set; }

        [DataMember(Name = "threshold")]
        [PropertyParameter(TriggerProperty.Threshold)]
        double? Threshold { get; set; }

        [DataMember(Name = "threshold_input")]
        string ThresholdInput { get; set; }

        [DataMember(Name = "unitsize")]
        [PropertyParameter(TriggerProperty.UnitSize)]
        string UnitSize { get; set; }

        [DataMember(Name = "unitsize_input")]
        string UnitSizeInput { get; set; }

        [DataMember(Name = "unittime")]
        [PropertyParameter(TriggerProperty.UnitTime)]
        string UnitTime { get; set; }

        [DataMember(Name = "unittime_input")]
        string UnitTimeInput { get; set; }

        [DataMember(Name = "period")]
        [PropertyParameter(TriggerProperty.Period)]
        string Period { get; set; }

        [DataMember(Name = "period_input")]
        string PeriodInput { get; set; }
    }
#pragma warning restore CS0649
}
