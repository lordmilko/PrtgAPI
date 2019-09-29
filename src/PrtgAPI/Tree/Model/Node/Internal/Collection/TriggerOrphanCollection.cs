using System.Collections.Generic;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a collection of <see cref="TriggerOrphan"/> objects in the PRTG Object Tree.
    /// </summary>
    internal class TriggerOrphanCollection : PrtgOrphanCollection
    {
        internal override string Name => "Triggers";

        internal TriggerOrphanCollection(IEnumerable<TriggerOrphan> children) : base(children)
        {
        }

        protected override TreeNode ToNodeCore(TreeNode parent) => new TriggerNodeCollection(this, (PrtgNode) parent);

        internal override PrtgOrphan Update(ITreeValue value, IEnumerable<PrtgOrphan> children)
        {
            return ValidateAndUpdate<TriggerOrphan>(children, PrtgNodeType.Trigger, TriggerCollection);
        }
    }
}
