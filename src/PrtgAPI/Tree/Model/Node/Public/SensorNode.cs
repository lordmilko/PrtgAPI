using System.Collections.Generic;
using PrtgAPI.Tree.Internal;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a <see cref="Sensor"/> in the PRTG Object Tree.
    /// </summary>
    public class SensorNode : PrtgNode<ISensor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SensorNode"/> class with the orphan this node encapsulates and the parent of this node.
        /// </summary>
        /// <param name="orphan">The orphan this node encapsulates.</param>
        /// <param name="parent">The parent of this node.</param>
        internal SensorNode(SensorOrphan orphan, PrtgNode parent) : base(orphan, parent)
        {
        }

        /// <summary>
        /// Dispatches this node to the visitor's <see cref="PrtgNodeVisitor{TResult}.VisitSensor(SensorNode)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>The result of visiting this node.</returns>
        [ExcludeFromCodeCoverage]
        public override T Accept<T>(PrtgNodeVisitor<T> visitor) => visitor.VisitSensor(this);

        /// <summary>
        /// Dispatches this node to the visitor's <see cref="PrtgNodeVisitor.VisitSensor(SensorNode)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        [ExcludeFromCodeCoverage]
        public override void Accept(PrtgNodeVisitor visitor) => visitor.VisitSensor(this);

        /// <summary>
        /// Creates a new <see cref="SensorNode"/> if the specified <paramref name="sensor"/> and <paramref name="children"/>
        /// differ from the values stored in this object.<para/>
        /// </summary>
        /// <param name="sensor">The sensor object to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        public override PrtgNode<ISensor> Update(ISensor sensor, IEnumerable<PrtgNode> children)
        {
            if (sensor != Value || children != Children)
                return Sensor(sensor, children);

            return this;
        }
    }
}
