using System;
using System.Diagnostics;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.TreeNodes
{
    [DebuggerDisplay("Name: {Name,nq}, Id: {Id}, ParentId: {ParentId}")]
    class SensorNode : TreeNode<SensorItem>
    {
        public SensorNode(string name) : base(name)
        {
        }

        public override SensorItem GetTestItem()
        {
            if (Id == -1)
                throw new InvalidOperationException("Id was not initialized");

            if (ParentId == -1)
                throw new InvalidOperationException("ParentId was not initialized");

            return new SensorItem(
                probe: GetProbe(this).Name,
                group: GetGroup(this).Name,
                name: Name,
                objid: Id.ToString(),
                parentId: ParentId.ToString()
            );
        }

        private ITreeNode GetGroup(ITreeNode node)
        {
            if (node is GroupNode)
                return node;

            if (node is ProbeNode)
                return node;

            if (node.Parent == null)
                return null;

            return GetGroup(node.Parent);
        }
    }
}
