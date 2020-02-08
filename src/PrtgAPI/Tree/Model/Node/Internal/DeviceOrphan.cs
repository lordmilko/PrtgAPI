using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a <see cref="Device"/> in the PRTG Object Tree that does not have a child -> parent relationship.
    /// </summary>
    internal class DeviceOrphan : PrtgOrphan<IDevice>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceOrphan"/> class with a specified device and children.
        /// </summary>
        /// <param name="device">The device to encapsulate in this orphan.</param>
        /// <param name="children">The children of this orphan.</param>
        internal DeviceOrphan(IDevice device, IEnumerable<PrtgOrphan> children) : base(device, children, PrtgNodeType.Device)
        {
        }

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor{TResult}.VisitDevice(DeviceOrphan)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        /// <returns>The result of visiting this orphan.</returns>
        [ExcludeFromCodeCoverage]
        internal override T Accept<T>(PrtgOrphanVisitor<T> visitor) => visitor.VisitDevice(this);

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor.VisitDevice(DeviceOrphan)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        [ExcludeFromCodeCoverage]
        internal override void Accept(PrtgOrphanVisitor visitor) => visitor.VisitDevice(this);

        /// <summary>
        /// Creates a new <see cref="DeviceOrphan"/> if the specified <paramref name="device"/> and <paramref name="children"/>
        /// differ from the values stored in this object.<para/>
        /// </summary>
        /// <param name="device">The device object to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        internal override PrtgOrphan<IDevice> Update(IDevice device, IEnumerable<PrtgOrphan> children)
        {
            if (device != Value || children != Children)
                return Device(device, children);

            return this;
        }

        /// <summary>
        /// Creates a new <see cref="DeviceNode"/> from this orphan.
        /// </summary>
        /// <param name="parent">The parent of the node.</param>
        /// <returns>A <see cref="DeviceNode"/> that encapsulates this orphan.</returns>
        protected override TreeNode ToNodeCore(TreeNode parent) => new DeviceNode(this, (PrtgNode) parent);

        protected override void ValidateChildren(IEnumerable<PrtgOrphan> children)
        {
            foreach (var child in children)
            {
                switch (child.Type)
                {
                    case PrtgNodeType.Sensor:
                        break;
                    default:
                        ValidateCommonChild(child);
                        break;
                }
            }
        }
    }
}
