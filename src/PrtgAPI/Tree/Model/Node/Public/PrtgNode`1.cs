using System.Collections.Generic;
using System.Diagnostics;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree
{
    /// <summary>
    /// Represents a strongly typed node found in the PRTG Object Tree.
    /// </summary>
    /// <typeparam name="TValue">The type of value this node encapsulates.</typeparam>
    [DebuggerTypeProxy(typeof(PrtgNodeDebugView<>))]
    public abstract class PrtgNode<TValue> : PrtgNode where TValue : ITreeValue
    {
        /// <summary>
        /// Gets the value from the PRTG Object Tree that this node encapsulates.
        /// </summary>
        public new TValue Value => (TValue) base.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgNode"/> class with the orphan this node encapsulates and the parent to use for this node.
        /// </summary>
        /// <param name="orphan">The orphan this node encapsulates.</param>
        /// <param name="parent">The parent of this node.</param>
        internal PrtgNode(PrtgOrphan orphan, PrtgNode parent) : base(orphan, parent)
        {
        }

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> if the specified <paramref name="value"/> and <paramref name="children"/>
        /// differ from the values stored in this object.<para/>
        /// Redirects to <see cref="Update(TValue, IEnumerable{PrtgNode})"/> to provide type safety.
        /// </summary>
        /// <param name="value">The tree value to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        internal override PrtgNode Update(ITreeValue value, IEnumerable<PrtgNode> children) => Update((TValue) value, children);

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> if the specified <paramref name="value"/> and <paramref name="children"/>
        /// differ from the values stored in this object.
        /// </summary>
        /// <param name="value">The tree value to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        public abstract PrtgNode<TValue> Update(TValue value, IEnumerable<PrtgNode> children);

        /// <summary>
        /// Creates a new <see cref="PrtgNode"/> if the specified tree <paramref name="value"/> differs from this object's <see cref="PrtgNode.Value"/>.
        /// </summary>
        /// <param name="value">The value to compare against.</param>
        /// <returns>If the value does not match this object's <see cref="PrtgNode.Value"/>, a new object containing the value. Otherwise, this object.</returns>
        public PrtgNode<TValue> WithValue(TValue value) => Update(value, Children);
    }
}
