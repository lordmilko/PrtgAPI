using System.ComponentModel;
using PrtgAPI.Attributes;
using PrtgAPI.Request.Serialization.ValueConverters;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Specifies properties that can be modified on notification triggers.</para>
    /// </summary>
    public enum TriggerProperty
    {
        /// <summary>
        /// The state that causes the trigger to activate.
        /// </summary>
        [Description("nodest")]
        State,

        /// <summary>
        /// The delay (in seconds) before the <see cref="OnNotificationAction"/> executes after the trigger has activated. 
        /// </summary>
        [Description("latency")]
        Latency,

        /// <summary>
        /// The notification action to execute when the trigger has activated.
        /// </summary>
        [Description("onnotificationid")]
        [ValueConverter(typeof(NotificationActionValueConverter))]
        OnNotificationAction,

        /// <summary>
        /// The notification action to execute when the trigger has deactivated.
        /// </summary>
        [Description("offnotificationid")]
        [ValueConverter(typeof(NotificationActionValueConverter))]
        OffNotificationAction,

        /// <summary>
        /// The notification action to execute when the trigger continues to remain activated for an extended period of time.
        /// </summary>
        [Description("escnotificationid")]
        [ValueConverter(typeof(NotificationActionValueConverter))]
        EscalationNotificationAction,

        /// <summary>
        /// The delay (in seconds) before the <see cref="EscalationNotificationAction"/> executes after the trigger has activated.
        /// </summary>
        [Description("esclatency")]
        EscalationLatency,

        /// <summary>
        /// The delay (in minutes) before the <see cref="EscalationNotificationAction"/> should be re-executed. 
        /// </summary>
        [Description("repeatival")]
        RepeatInterval,

        /// <summary>
        /// The channel the trigger should apply to.
        /// </summary>
        [Description("channel")]
        Channel,

        /// <summary>
        /// The time period over which the <see cref="Threshold"/> volume is analyzed. Used in conjunction with <see cref="UnitSize"/>.
        /// </summary>
        [Description("period")]
        Period,

        /// <summary>
        /// The unit component of the data rate that causes this trigger to activate. Used in conjunction with <see cref="Period"/> or <see cref="UnitTime"/>.
        /// </summary>
        [Description("unitsize")]
        UnitSize,

        /// <summary>
        /// The condition that must be true for the trigger to activate. Used in conjunction with <see cref="Threshold"/>.
        /// </summary>
        [Description("condition")]
        Condition,

        /// <summary>
        /// The threshold that must be reached for the trigger to activate. Used in conjunction with <see cref="Condition"/>.
        /// </summary>
        [Description("threshold")]
        Threshold,

        /// <summary>
        /// The time component of the data rate that causes this trigger to activate. Used in conjunction with <see cref="UnitSize"/>.
        /// </summary>
        [Description("unittime")]
        UnitTime
    }
}
