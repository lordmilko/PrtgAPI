using System.Collections.Generic;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a <see cref="Sensor"/> in the PRTG Object Tree that does not have a child -> parent relationship.
    /// </summary>
    internal class SensorOrphan : PrtgOrphan<ISensor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SensorOrphan"/> class with a specified sensor and children.
        /// </summary>
        /// <param name="sensor">The sensor to encapsulate in this orphan.</param>
        /// <param name="children">The children of this orphan.</param>
        internal SensorOrphan(ISensor sensor, IEnumerable<PrtgOrphan> children) : base(sensor, children, PrtgNodeType.Sensor)
        {
        }

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor{TResult}.VisitSensor(SensorOrphan)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        /// <returns>The result of visiting this orphan.</returns>
        internal override T Accept<T>(PrtgOrphanVisitor<T> visitor) => visitor.VisitSensor(this);

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor.VisitSensor(SensorOrphan)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        internal override void Accept(PrtgOrphanVisitor visitor) => visitor.VisitSensor(this);

        /// <summary>
        /// Creates a new <see cref="SensorOrphan"/> if the specified <paramref name="sensor"/> and <paramref name="children"/>
        /// differ from the values stored in this object.<para/>
        /// </summary>
        /// <param name="sensor">The sensor object to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        internal override PrtgOrphan<ISensor> Update(ISensor sensor, IEnumerable<PrtgOrphan> children)
        {
            if (sensor != Value || children != Children)
                return Sensor(sensor, children);

            return this;
        }

        /// <summary>
        /// Creates a new <see cref="SensorNode"/> from this orphan.
        /// </summary>
        /// <param name="parent">The parent of the node.</param>
        /// <returns>A <see cref="SensorNode"/> that encapsulates this orphan.</returns>
        protected override TreeNode ToNodeCore(TreeNode parent) => new SensorNode(this, (PrtgNode) parent);

        protected override void ValidateChildren(IEnumerable<PrtgOrphan> children)
        {
            foreach (var child in children)
                ValidateCommonChild(child);
        }
    }
}
