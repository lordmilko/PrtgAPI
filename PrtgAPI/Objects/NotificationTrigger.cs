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

        /// <summary>
        /// Delay before this notification is activated after activation requirements have been met.
        /// Applies to: State, Threshold, Speed Triggers
        /// </summary>
        [DataMember(Name = "latency")]
        public int? Latency { get; set; }

        [DataMember(Name = "channel")]
        private string channel;

        public TriggerChannel? Channel => channel?.ToEnum<TriggerChannel>(); //volume, threshold, speed

        public string Units
        {
            get
            {
                if (unitSize != null && UnitTime != null)
                    return $"{unitSize}/{UnitTime}";
                else if (unitSize != null && period != null)
                    return $"{unitSize}/{period}";
                else if (unitSize == null && UnitTime == null && period == null)
                    return null;
                else
                    throw new Exception("not sure how to format units"); //todo: fix this up
            }
        }

        [DataMember(Name = "unitsize")]
        private string unitSizeStr; //volume, speed

        private TriggerUnitSize? unitSize => unitSizeStr?.DescriptionToEnum<TriggerUnitSize>();

        [DataMember(Name = "unittime")]
        private string unitTimeStr; //speed

        private TriggerUnitTime? UnitTime => unitTimeStr?.DescriptionToEnum<TriggerUnitTime>();

        [DataMember(Name = "period")]
        private string periodStr; //volume

        private TriggerPeriod? period => periodStr?.DescriptionToEnum<TriggerPeriod>();

        [DataMember(Name = "onnotificationid")]
        private string onNotificationAction;

        /// <summary>
        /// Notification action that will occur when trigger is activated.
        /// Applies to: State, Volume, Threshold, Change, Speed Triggers
        /// </summary>
        public NotificationActionDescriptor OnNotificationAction => new NotificationActionDescriptor(onNotificationAction);

        [DataMember(Name = "offnotificationid")]
        private string offNotificationAction;

        /// <summary>
        /// Notification action that will occur when trigger is deactivated.
        /// Applies to: State, Threshold, Speed Triggers
        /// </summary>
        public NotificationActionDescriptor OffNotificationAction => offNotificationAction == null ? null : new NotificationActionDescriptor(offNotificationAction);

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

        private string condition;

        [DataMember(Name = "condition")] //speed, threshold?
        private string conditionStr;

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
        public NotificationActionDescriptor EscalationNotificationAction => escalationNotificationAction == null ? null : new NotificationActionDescriptor(escalationNotificationAction);

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
