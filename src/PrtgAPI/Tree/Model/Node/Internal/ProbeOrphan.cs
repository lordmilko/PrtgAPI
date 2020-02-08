using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a <see cref="Probe"/> in the PRTG Object Tree that does not have a child -> parent relationship.
    /// </summary>
    internal class ProbeOrphan : PrtgOrphan<IProbe>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProbeOrphan"/> class with a specified probe and children.
        /// </summary>
        /// <param name="probe">The probe to encapsulate in this orphan.</param>
        /// <param name="children">The children of this orphan.</param>
        internal ProbeOrphan(IProbe probe, IEnumerable<PrtgOrphan> children) : base(probe, children, PrtgNodeType.Probe)
        {
        }

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor{TResult}.VisitProbe(ProbeOrphan)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        /// <returns>The result of visiting this orphan.</returns>
        [ExcludeFromCodeCoverage]
        internal override T Accept<T>(PrtgOrphanVisitor<T> visitor) => visitor.VisitProbe(this);

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor.VisitProbe(ProbeOrphan)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        [ExcludeFromCodeCoverage]
        internal override void Accept(PrtgOrphanVisitor visitor) => visitor.VisitProbe(this);

        /// <summary>
        /// Creates a new <see cref="ProbeOrphan"/> if the specified <paramref name="probe"/> and <paramref name="children"/>
        /// differ from the values stored in this object.<para/>
        /// </summary>
        /// <param name="probe">The probe object to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        internal override PrtgOrphan<IProbe> Update(IProbe probe, IEnumerable<PrtgOrphan> children)
        {
            if (probe != Value || children != Children)
                return Probe(probe, children);

            return this;
        }

        /// <summary>
        /// Creates a new <see cref="ProbeNode"/> from this orphan.
        /// </summary>
        /// <param name="parent">The parent of the node.</param>
        /// <returns>A <see cref="ProbeNode"/> that encapsulates this orphan.</returns>
        protected override TreeNode ToNodeCore(TreeNode parent) => new ProbeNode(this, (PrtgNode) parent);

        protected override void ValidateChildren(IEnumerable<PrtgOrphan> children)
        {
            foreach (var child in children)
            {
                switch (child.Type)
                {
                    case PrtgNodeType.Device:
                        break;
                    case PrtgNodeType.Group:
                        if (child.Value.Id == 0)
                            throw new InvalidOperationException($"Cannot add the PRTG Root Node as the child of a probe.");

                        break;
                    default:
                        ValidateCommonChild(child);
                        break;
                }
            }
        }
    }
}
