using System.Collections.Generic;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a collection of <see cref="TriggerNode"/> objects in the PRTG Object Tree.
    /// </summary>
    public class TriggerNodeCollection : PrtgNodeCollection
    {
        internal TriggerNodeCollection(TriggerOrphanCollection orphan, PrtgNode parent) : base(orphan, parent)
        {
        }

        internal override PrtgNode Update(ITreeValue value, IEnumerable<PrtgNode> children)
        {
            return ValidateAndUpdate<TriggerNode>(children, PrtgNodeType.Trigger, TriggerCollection);
        }
    }
}
