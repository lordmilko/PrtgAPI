using System.Collections.Generic;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a collection of properties of an <see cref="IPrtgObject"/> in the PRTG Object Tree.
    /// </summary>
    internal class PropertyOrphanCollection : PrtgOrphanCollection
    {
        internal override string Name => "Properties";

        internal PropertyOrphanCollection(IEnumerable<PropertyOrphan> children) : base(children)
        {
        }

        protected override TreeNode ToNodeCore(TreeNode parent) => new PropertyNodeCollection(this, (PrtgNode) parent);

        internal override PrtgOrphan Update(ITreeValue value, IEnumerable<PrtgOrphan> children)
        {
            return ValidateAndUpdate<PropertyOrphan>(children, PrtgNodeType.Property, PropertyCollection);
        }
    }
}
