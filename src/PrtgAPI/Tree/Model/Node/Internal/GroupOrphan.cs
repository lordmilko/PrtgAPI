using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a <see cref="Group"/> in the PRTG Object Tree that does not have a child -> parent relationship.
    /// </summary>
    internal class GroupOrphan : PrtgOrphan<Group>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupOrphan"/> class with a specified group and children.
        /// </summary>
        /// <param name="group">The group to encapsulate in this orphan.</param>
        /// <param name="children">The children of this orphan.</param>
        internal GroupOrphan(Group group, IEnumerable<PrtgOrphan> children) : base(group, children, PrtgNodeType.Group)
        {
        }

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor{TResult}.VisitGroup(GroupOrphan)"/> method
        /// and returns a value of a type specified by the <paramref name="visitor"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to return.</typeparam>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        /// <returns>The result of visiting this orphan.</returns>
        [ExcludeFromCodeCoverage]
        internal override T Accept<T>(PrtgOrphanVisitor<T> visitor) => visitor.VisitGroup(this);

        /// <summary>
        /// Dispatches this orphan to the visitor's <see cref="PrtgOrphanVisitor.VisitGroup(GroupOrphan)"/> method.
        /// </summary>
        /// <param name="visitor">The visitor to visit this orphan with.</param>
        [ExcludeFromCodeCoverage]
        internal override void Accept(PrtgOrphanVisitor visitor) => visitor.VisitGroup(this);

        /// <summary>
        /// Creates a new <see cref="GroupOrphan"/> if the specified <paramref name="group"/> and <paramref name="children"/>
        /// differ from the values stored in this object.<para/>
        /// </summary>
        /// <param name="group">The group object to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        internal override PrtgOrphan<Group> Update(Group group, IEnumerable<PrtgOrphan> children)
        {
            if (group != Value || children != Children)
                return Group(group, children);

            return this;
        }

        /// <summary>
        /// Creates a new <see cref="GroupNode"/> from this orphan.
        /// </summary>
        /// <param name="parent">The parent of the node.</param>
        /// <returns>A <see cref="GroupNode"/> that encapsulates this orphan.</returns>
        protected override TreeNode ToNodeCore(TreeNode parent) => new GroupNode(this, (PrtgNode) parent);

        protected override void ValidateChildren(IEnumerable<PrtgOrphan> children)
        {
            foreach (var child in children)
            {
                switch (child.Type)
                {
                    case PrtgNodeType.Device:
                    case PrtgNodeType.Group:
                        if (Value.Id == WellKnownId.Root)
                            throw new InvalidOperationException($"Node '{child} (ID: {child.Value.Id})' of type '{child.Type}' cannot be a child of the PRTG Root Node. Only probes may directly descend from the root node.");

                        if (child.Value.Id == WellKnownId.Root)
                            throw new InvalidOperationException("Cannot add the PRTG Root Node as the child of another group.");

                        break;
                    case PrtgNodeType.Probe:
                        if (Value.Id != WellKnownId.Root)
                            throw new InvalidOperationException($"Node '{child} (ID: {child.Value.Id})' of type '{child.Type}' cannot be a child of a group that is not the PRTG Root Node.");

                        break;
                    default:
                        ValidateCommonChild(child);
                        break;
                }
            }
        }
    }
}
