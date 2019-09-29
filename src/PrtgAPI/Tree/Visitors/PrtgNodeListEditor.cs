using System;
using System.Collections.Generic;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Provides facilities for editing a <see cref="NodeList"/> containing <see cref="PrtgNode"/> objects.
    /// </summary>
    internal class PrtgNodeListEditor : PrtgNodeRewriter
    {
        PrtgNode oldNode;
        IEnumerable<PrtgNode> newNodes;
        ListEditType type;

        internal PrtgNodeListEditor(PrtgNode oldNode, IEnumerable<PrtgNode> nodes, ListEditType type)
        {
            this.oldNode = oldNode;
            this.newNodes = nodes;
            this.type = type;
        }

        protected override IReadOnlyList<PrtgNode> VisitList(INodeList<PrtgNode> nodes)
        {
            var index = nodes.IndexOf(oldNode);

            if (index >= 0 && index < nodes.Count)
            {
                switch (type)
                {
                    case ListEditType.Replace:
                        return nodes.ReplaceRange(oldNode, newNodes);
                    case ListEditType.InsertAfter:
                        return nodes.InsertRange(index + 1, newNodes);
                    case ListEditType.InsertBefore:
                        return nodes.InsertRange(index, newNodes);
                    default:
                        throw new NotImplementedException($"Don't know how to process list edit type '{type}'.");
                }
            }

            return base.VisitList(nodes);
        }
    }
}
