using System.Collections.Generic;
using PrtgAPI.Tree.Internal;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a <see cref="Probe"/> in the PRTG Object Tree.
    /// </summary>
    public class ProbeNode : PrtgNode<IProbe>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProbeNode"/> class with the orphan this node encapsulates and the parent of this node.
        /// </summary>
        /// <param name="orphan">The orphan this node encapsulates.</param>
        /// <param name="parent">The parent of this node.</param>
        internal ProbeNode(ProbeOrphan orphan, PrtgNode parent) : base(orphan, parent)
        {
        }

        /// <summary>
        /// Dispatches this node to the visitor's <see cref="PrtgNodeVisitor{TResult}.VisitProbe(ProbeNode)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this node with.</param>
        /// <returns>The result of visiting this node.</returns>
        [ExcludeFromCodeCoverage]
        public override T Accept<T>(PrtgNodeVisitor<T> visitor) => visitor.VisitProbe(this);

        /// <summary>
        /// Dispatches this node to the visitor's <see cref="PrtgNodeVisitor.VisitProbe(ProbeNode)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        [ExcludeFromCodeCoverage]
        public override void Accept(PrtgNodeVisitor visitor) => visitor.VisitProbe(this);

        /// <summary>
        /// Creates a new <see cref="ProbeNode"/> if the specified <paramref name="probe"/> and <paramref name="children"/>
        /// differ from the values stored in this object.<para/>
        /// </summary>
        /// <param name="probe">The probe object to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        public override PrtgNode<IProbe> Update(IProbe probe, IEnumerable<PrtgNode> children)
        {
            if (probe != Value || children != Children)
                return Probe(probe, children);

            return this;
        }
    }
}
