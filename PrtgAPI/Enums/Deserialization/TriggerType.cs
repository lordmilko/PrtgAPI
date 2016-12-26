using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies types of notification triggers that can be defined on objects.
    /// </summary>
    [DataContract]
    public enum TriggerType
    {
        /// <summary>
        /// Trigger when an object changes state.
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
