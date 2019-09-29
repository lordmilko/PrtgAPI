using System.Collections.Generic;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a collection of properties of an <see cref="IPrtgObject"/> in the PRTG Object Tree.
    /// </summary>
    public class PropertyNodeCollection : PrtgNodeCollection
    {
        internal PropertyNodeCollection(PropertyOrphanCollection orphan, PrtgNode parent) : base(orphan, parent)
        {
        }

        internal override PrtgNode Update(ITreeValue value, IEnumerable<PrtgNode> children)
        {
            return ValidateAndUpdate<PropertyNode>(children, PrtgNodeType.Property, PropertyCollection);
        }
    }
}
