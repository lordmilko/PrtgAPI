using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Xml.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Helpers;

namespace PrtgAPI
{
#pragma warning disable CS0649 //Field 'field' is never assigned to, and will always have its default value 'value'

    /// <summary>
    /// <para type="description">Causes notification actions to occur when a sensor exhibits a specified behaviour.</para>
    /// </summary>
    [DataContract]
    [Description("Notification Trigger")]
    [DebuggerDisplay("Type = {Type}, Inherited = {Inherited}, OnNotificationAction = {OnNotificationAction}")]
    public class NotificationTrigger : ISubObject
    {
        [DataMember(Name = "type")]
        private string type;

        [ExcludeFromCodeCoverage]
        string IObject.Name
        {
            get { return OnNotificationAction.Name; }
            set { OnNotificationAction.Name = value; }
        }

        /// <summary>
        /// The Object ID this notification trigger applies to.
        /// </summary>
        public int ObjectId { get; set; }

        /// <summary>
        /// The Object ID notification trigger is defined on. If this value is not the same as the <see cref="ObjectId"/>, this indicates this notification trigger is inherited.
        /// </summary>
        public int ParentId
        {
            get
            {
                //todo - it seems like they all have a thisid AND an id, so this is unnecessary?
                var thisId = objectLinkXml.Attribute("thisid");

                if (thisId != null)
                    return Convert.ToInt32(thisId.Value);
                else
                    return Convert.ToInt32(objectLinkXml.Attribute("id").Value);
            }
        }

        /// <summary>
        /// Whether the notification trigger is defined on its parent object, or whether it is inherited from another object.
        /// </summary>
        public bool Inherited => ObjectId != ParentId;

        /// <summary>
        /// The type of event that triggers this notification.
        /// Applies to: State, Volume, Threshold, Change, Speed Triggers
        /// </summary>
        public TriggerType Type => type.ToEnum<TriggerType>();

        private string typeName;

        /// <summary>
        /// Full name of the event that triggers this notification.
        /// Applies to: State, Volume, Threshold, Change, Speed Triggers
        /// </summary>
        [DataMember(Name = "typename")]
        public string TypeName
        {
            get { return typeName; }
            set
            {
                if (value == string.Empty)
                    value = null;

                typeName = value;
            }
        }

        /// <summary>
        /// ID of the trigger on the object it is assigned to.
        /// Applies to: State, Volume, Threshold, Change, Speed Triggers
        /// </summary>
        [DataMember(Name = "subid")]
        public int SubId { get; set; }

        /// <summary>
        /// State that will cause this notification to trigger.
        /// Applies to: State Triggers
        /// </summary>
        [DataMember(Name = "nodest")]
        private string stateTrigger;

        internal TriggerSensorState? StateTrigger => stateTrigger == null ? null : (TriggerSensorState?)EnumExtensions.XmlToEnumAnyAttrib(stateTrigger, typeof(TriggerSensorState));

        /// <summary>
        /// Delay (in seconds) before this notification is activated after activation requirements have been met.
        /// Applies to: State, Threshold, Speed Triggers
        /// </summary>
        [DataMember(Name = "latency")]
        public int? Latency { get; set; }

        [DataMember(Name = "channel")]
        internal string channelName;

        //Manually assigned to by GetNotificationTriggers
        internal Channel channelObj;

        private TriggerChannel channel;

        /// <summary>
        /// The channel the trigger should apply to.
        /// Applies to: Speed, Threshold, Volume Triggers
        /// </summary>
        public TriggerChannel Channel => channel ?? (channel = TriggerChannel.ParseFromResponse(channelObj));

        /// <summary>
        /// The formatted units display of this trigger.
        /// </summary>
        public string Unit
        {
            get
            {
                if (UnitSize != null && UnitTime != null)
                    return $"{UnitSize}/{UnitTime}";
                else if (UnitSize != null && Period != null)
                    return $"{UnitSize}/{Period}";
                else if (UnitSize == null && UnitTime == null && Period == null)
                    return null;
                else
                    throw new Exception("not sure how to format units"); //todo: fix this up
            }
        }

        [DataMember(Name = "unitsize")]
        private string unitSizeStr; //volume, speed

        internal DataUnit? UnitSize => unitSizeStr?.DescriptionToEnum<DataUnit>();

        [DataMember(Name = "unittime")]
        private string unitTimeStr; //speed

        internal TimeUnit? UnitTime => unitTimeStr?.DescriptionToEnum<TimeUnit>();

        [DataMember(Name = "period")]
        private string periodStr; //volume

        internal TriggerPeriod? Period => periodStr?.DescriptionToEnum<TriggerPeriod>();

        [DataMember(Name = "onnotificationid")]
        private string onNotificationActionStr;

        private NotificationAction onNotificationAction;

        /// <summary>
        /// Notification action that will occur when trigger is activated.
        /// Applies to: State, Volume, Threshold, Change, Speed Triggers
        /// </summary>
        public NotificationAction OnNotificationAction => onNotificationAction ?? (onNotificationAction = new NotificationAction(onNotificationActionStr));

        [DataMember(Name = "offnotificationid")]
        private string offNotificationActionStr;

        private NotificationAction offNotificationAction;

        /// <summary>
        /// Notification action that will occur when trigger is deactivated.
        /// Applies to: State, Threshold, Speed Triggers
        /// </summary>
        public NotificationAction OffNotificationAction => offNotificationActionStr == null ? null : offNotificationAction ?? (offNotificationAction = new NotificationAction(offNotificationActionStr));

        private int? threshold;

        /// <summary>
        /// Value threshold or object state required before this notification is activated.
        /// Applies to: Threshold, Speed, Volume Triggers
        /// </summary>
        [DataMember(Name = "threshold")]
        public string Threshold
        {
            get
            {
                if (stateTrigger != null)
                    return stateTrigger;

                return threshold?.ToString();
            }
            set { threshold = Convert.ToInt32(value); }
        }

        internal int? ThresholdInternal => threshold;

        [DataMember(Name = "condition")]
        private string conditionStr;

        /// <summary>
        /// Condition that must be true for the trigger to activate.
        /// Applies to: Speed, Threshold Triggers
        /// </summary>
        public TriggerCondition? Condition => conditionStr?.DescriptionToEnum<TriggerCondition>() ?? TriggerCondition.Equals;

        /// <summary>
        /// Delay (in seconds) before the <see cref="EscalationNotificationAction"/> occurs after this trigger has been activated.
        /// Applies to: State Triggers
        /// </summary>
        [DataMember(Name = "esclatency")]
        public int? EscalationLatency { get; set; }

        [DataMember(Name = "escnotificationid")]
        private string escalationNotificationActionStr;

        private NotificationAction escalationNotificationAction;

        /// <summary>
        /// Notification action that will occur when the trigger condition has been remained active for an extended period of time. Repeats every <see cref="EscalationLatency"/> seconds.
        /// Applies to: State Triggers
        /// </summary>
        public NotificationAction EscalationNotificationAction => escalationNotificationActionStr == null ? null : escalationNotificationAction ?? (escalationNotificationAction = new NotificationAction(escalationNotificationActionStr));

        /// <summary>
        /// Interval to repeat the <see cref="EscalationNotificationAction"/>, in minutes.
        /// Applies to: State Triggers
        /// </summary>
        [DataMember(Name = "repeatival")]
        public int? RepeatInterval { get; set; } //what if theres no interval?

        [DataMember(Name = "objectlink")]
        private string objectLink;

        private XElement objectLinkXml => XElement.Parse(objectLink.Replace("&", "&amp;"));

        internal bool HasChannel()
        {
            switch(Type)
            {
                case TriggerType.Speed:
                case TriggerType.Threshold:
                case TriggerType.Volume:
                    return true;
                default:
                    return false;
            }
        }
        
        internal bool SetEnumChannel()
        {
            var @enum = EnumHelpers.XmlToEnum<XmlEnumAlternateName>(channelName, typeof(StandardTriggerChannel), false);

            if(@enum != null)
            {
                channel = (StandardTriggerChannel)@enum;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Type = {Type}, Inherited = {Inherited}, OnNotificationAction = {OnNotificationAction}";
        }

#pragma warning restore CS0649
    }
}
