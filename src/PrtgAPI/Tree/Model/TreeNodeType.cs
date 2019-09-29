namespace PrtgAPI.Tree
{
    /// <summary>
    /// Specifies types of nodes that a <see cref="TreeNode"/> can represent.
    /// </summary>
    public enum TreeNodeType
    {
        /// <summary>
        /// A simple node; an atomic tree component.
        /// </summary>
        Node = 0,

        /// <summary>
        /// A node that represents a collection of other nodes.
        /// </summary>
        Collection = 6,

        /// <summary>
        /// A node that represents a grouping of nodes, such as a group of nodes with a common name.
        /// </summary>
        Grouping = 7
    }

    /// <summary>
    /// Specifies types of nodes that a <see cref="PrtgNode"/> can represent.
    /// </summary>
    public enum PrtgNodeType
    {
        /// <summary>
        /// A node that represents a <see cref="PrtgAPI.Sensor"/>.
        /// </summary>
        Sensor = 0,

        /// <summary>
        /// A node that represents a <see cref="PrtgAPI.Device"/>.
        /// </summary>
        Device = 1,

        /// <summary>
        /// A node that represents a <see cref="PrtgAPI.Group"/>.
        /// </summary>
        Group = 2,

        /// <summary>
        /// A node that represents a <see cref="PrtgAPI.Probe"/>.
        /// </summary>
        Probe = 3,

        /// <summary>
        /// A node that represents a <see cref="PrtgAPI.NotificationTrigger"/>.
        /// </summary>
        Trigger = 4,

        /// <summary>
        /// A node that represents a property and value of an <see cref="IPrtgObject"/>.
        /// </summary>
        Property = 5,

        /// <summary>
        /// A node that represents a collection of other nodes.
        /// </summary>
        Collection = 6,

        /// <summary>
        /// A node that represents a grouping of nodes, such as a group of nodes with a common name.
        /// </summary>
        Grouping = 7
    }
}
