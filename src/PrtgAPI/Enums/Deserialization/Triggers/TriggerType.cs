using System.Runtime.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Specifies types of notification triggers that can be defined on objects.</para>
    /// </summary>
    [DataContract]
    public enum TriggerType
    {
        /// <summary>
        /// Trigger when an object enters a specific state.
        /// </summary>
        State,

        /// <summary>
        /// Trigger when a channel reaches a certain speed.
        /// </summary>
        Speed,

        /// <summary>
        /// Trigger when a channel processes a certain volume of data over a given time frame.
        /// </summary>
        Volume,

        /// <summary>
        /// Trigger when a channel reaches a certain value.
        /// </summary>
        Threshold,

        /// <summary>
        /// Trigger whenever an object changes.
        /// </summary>
        Change,
    }
}
