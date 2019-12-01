using System;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Specifies the types of descendant objects to include when constructing a <see cref="PrtgNode"/> tree.
    /// </summary>
    [Flags]
    public enum TreeParseOption
    {
        /// <summary>
        /// Include <see cref="Sensor"/> objects.
        /// </summary>
        Sensors = 1,

        /// <summary>
        /// Include <see cref="Device"/> objects.
        /// </summary>
        Devices = 2,

        /// <summary>
        /// Include <see cref="Group"/> objects.
        /// </summary>
        Groups = 4,

        /// <summary>
        /// Include <see cref="Probe"/> objects. If <see cref="Groups"/> is not specified, only the Root group will be included.
        /// </summary>
        Probes = 8,

        /// <summary>
        /// Include <see cref="PropertyValuePair"/> objects.
        /// </summary>
        Properties = 16,

        /// <summary>
        /// Include <see cref="NotificationTrigger"/> objects.
        /// </summary>
        Triggers = 32,

        /// <summary>
        /// Include only <see cref="Sensor"/>, <see cref="Device"/>, <see cref="Group"/> and <see cref="Probe"/> objects.
        /// </summary>
        Common = Sensors | Devices | Groups | Probes,

        /// <summary>
        /// Include all object types.
        /// </summary>
        All = Common | Properties | Triggers
    }
}
