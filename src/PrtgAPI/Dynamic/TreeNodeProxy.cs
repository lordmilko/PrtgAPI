using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using PrtgAPI.Tree;

namespace PrtgAPI.Dynamic
{
    class TreeNodeProxy<TTreeNode> : DynamicProxy<TreeNode<TTreeNode>> where TTreeNode : TreeNode<TTreeNode>
    {
        public override bool TryGetMember(TreeNode<TTreeNode> instance, GetMemberBinder binder, out object value)
        {
            //todo: need to be able to ignore case. should the indexer perhaps ALWAYS be ignoring case?
            //maybe we should have an indexer overload that takes a bool saying whether or not to ignore case?
            //group["Servers", true]
            var children = instance[binder.Name, true];

            if (children != null)
            {
                value = children;
                return true;
            }

            value = null;
            return false;
        }

        [ExcludeFromCodeCoverage]
        public override IEnumerable<string> GetDynamicMemberNames(TreeNode<TTreeNode> instance)
        {
            return instance.Children.Select(c => c.Name);
        }
    }
}
