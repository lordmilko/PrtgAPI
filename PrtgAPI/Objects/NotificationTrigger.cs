using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PrtgAPI.Helpers;

namespace PrtgAPI
{
#pragma warning disable CS0649

    /// <summary>
    /// Causes notification actions to occur when a sensor exhibits a specified behaviour.
    /// </summary>
    [DataContract]
    public class NotificationTrigger
    {
        [DataMember(Name = "type")]
        private string type;

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

        /// <summary>
        /// Full name of the event that triggers this notification.
        /// Applies to: State, Volume, Threshold, Change, Speed Triggers
        /// </summary>
        [DataMember(Name = "typename")]
        public string TypeName { get; set; }

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

        internal TriggerSensorState? StateTrigger => stateTrigger?.XmlToEnum<TriggerSensorState>();

        /// <summary>
        /// Delay (in seconds) before this notification is activated after activation requirements have been met.
        /// Applies to: State, Threshold, Speed Triggers
        /// </summary>
        [DataMember(Name = "latency")]
        public int? Latency { get; set; }

        [DataMember(Name = "channel")]
        private string channel;

        /// <summary>
        /// The channel the trigger should apply to.
        /// Applies to: Speed, Threshold, Volume Triggers
        /// </summary>
        public TriggerChannel? Channel => channel?.XmlAltToEnum<TriggerChannel>();

        /// <summary>
        /// The formatted units display of this trigger.
        /// </summary>
        public string Units
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

        internal TriggerUnitSize? UnitSize => unitSizeStr?.DescriptionToEnum<TriggerUnitSize>();

        [DataMember(Name = "unittime")]
        private string unitTimeStr; //speed

        internal TriggerUnitTime? UnitTime => unitTimeStr?.DescriptionToEnum<TriggerUnitTime>();

        [DataMember(Name = "period")]
        private string periodStr; //volume

        internal TriggerPeriod? Period => periodStr?.DescriptionToEnum<TriggerPeriod>();

        [DataMember(Name = "onnotificationid")]
        private string onNotificationAction;

        /// <summary>
        /// Notification action that will occur when trigger is activated.
        /// Applies to: State, Volume, Threshold, Change, Speed Triggers
        /// </summary>
        public NotificationAction OnNotificationAction => new NotificationAction(onNotificationAction);

        [DataMember(Name = "offnotificationid")]
        private string offNotificationAction;

        /// <summary>
        /// Notification action that will occur when trigger is deactivated.
        /// Applies to: State, Threshold, Speed Triggers
        /// </summary>
        public NotificationAction OffNotificationAction => offNotificationAction == null ? null : new NotificationAction(offNotificationAction);

        private int? threshold;

        /// <summary>
        /// Value threshold required before this notification is activated.
        /// Applies to: Threshold, Speed Triggers
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
        /// Delay before the <see cref="EscalationNotificationAction"/> occurs after this trigger has been activated.
        /// Applies to: State Triggers
        /// </summary>
        [DataMember(Name = "esclatency")]
        public int? EscalationLatency { get; set; }

        [DataMember(Name = "escnotificationid")]
        private string escalationNotificationAction;

        /// <summary>
        /// Notification action to repeat when the trigger cause does not get cleared.
        /// Applies to: State Triggers
        /// </summary>
        public NotificationAction EscalationNotificationAction => escalationNotificationAction == null ? null : new NotificationAction(escalationNotificationAction);

        /// <summary>
        /// Interval to repeat the <see cref="EscalationNotificationAction"/>, in minutes.
        /// Applies to: State Triggers
        /// </summary>
        [DataMember(Name = "repeatival")]
        public int? RepeatInterval { get; set; } //what if theres no interval?

        [DataMember(Name = "objectlink")]
        private string objectLink;

        private XElement objectLinkXml => XElement.Parse(objectLink);

#pragma warning restore CS0649
    }
}
