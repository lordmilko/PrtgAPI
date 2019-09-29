using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a visitor capable of recursively rewriting the members of a <see cref="PrtgNode"/> in depth-first order.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class PrtgNodeRewriter : PrtgNodeVisitor<PrtgNode>
    {
        /// <summary>
        /// Visits the children of a <see cref="SensorNode"/> and replaces the node if any of its children are modified.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>If any children were modified, a new node with the modified children. Otherwise, the original node.</returns>
        protected internal override PrtgNode VisitSensor(SensorNode node)
        {
            return node.WithChildren(VisitList(node.Children));
        }

        /// <summary>
        /// Visits the children of a <see cref="DeviceNode"/> and replaces the node if any of its children are modified.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>If any children were modified, a new node with the modified children. Otherwise, the original node.</returns>
        protected internal override PrtgNode VisitDevice(DeviceNode node)
        {
            return node.WithChildren(VisitList(node.Children));
        }

        /// <summary>
        /// Visits the children of a <see cref="GroupNode"/> and replaces the node if any of its children are modified.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>If any children were modified, a new node with the modified children. Otherwise, the original node.</returns>
        protected internal override PrtgNode VisitGroup(GroupNode node)
        {
            return node.WithChildren(VisitList(node.Children));
        }

        /// <summary>
        /// Visits the children of a <see cref="ProbeNode"/> and replaces the node if any of its children are modified.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>If any children were modified, a new node with the modified children. Otherwise, the original node.</returns>
        protected internal override PrtgNode VisitProbe(ProbeNode node)
        {
            return node.WithChildren(VisitList(node.Children));
        }

        /// <summary>
        /// Visits the children of a <see cref="TriggerNode"/> and replaces the node if any of its children are modified.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>If any children were modified, a new node with the modified children. Otherwise, the original node.</returns>
        protected internal override PrtgNode VisitTrigger(TriggerNode node)
        {
            Debug.Assert(node.Children.Count == 0, "Trigger had a child. Need to implement WithChildren support for triggers");

            return node;
        }

        /// <summary>
        /// Visits the children of a <see cref="PropertyNode"/> and replaces the node if any of its children are modified.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>If any children were modified, a new node with the modified children. Otherwise, the original node.</returns>
        protected internal override PrtgNode VisitProperty(PropertyNode node)
        {
            Debug.Assert(node.Children.Count == 0, "Trigger had a child. Need to implement WithChildren support for properties");

            return node;
        }

        /// <summary>
        /// Visits the children of a <see cref="PrtgNodeCollection"/> and replaces the node if any of its children are modified.
        /// </summary>
        /// <param name="node">The node to visit.</param>
        /// <returns>If any children were modified, a new node with the modified children. Otherwise, the original node.</returns>
        protected internal override PrtgNode VisitCollection(PrtgNodeCollection node)
        {
            return node.WithChildren(VisitList(node.Children));
        }

        #region Helpers

        /// <summary>
        /// Visit the nodes in a list. If any of node is modified after being visited, this method will return a new collection containing the updated nodes.<para/>
        /// If an node is null after being visited, it will be excluded from the returned list.
        /// </summary>
        /// <param name="nodes">The nodes to visit.</param>
        /// <returns>If any node was modified, a new list containing the updated nodes. Otherwise, the original list.</returns>
        protected virtual IReadOnlyList<PrtgNode> VisitList(INodeList<PrtgNode> nodes)
        {
            return VisitorUtilities.VisitList(nodes, Visit);
        }

        #endregion
    }
}
