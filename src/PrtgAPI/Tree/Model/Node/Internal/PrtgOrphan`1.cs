using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a strongly typed orphan found in the PRTG Object Tree.
    /// </summary>
    /// <typeparam name="TValue">The type of value this orphan encapsulates.</typeparam>
    [DebuggerTypeProxy(typeof(PrtgOrphanDebugView<>))]
    internal abstract class PrtgOrphan<TValue> : PrtgOrphan where TValue : ITreeValue
    {
        /// <summary>
        /// Gets the value from the PRTG Object Tree that this orphan encapsulates.
        /// </summary>
        public new TValue Value => (TValue) base.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrtgOrphan{TValue}"/> class with a specified tree value and children.
        /// </summary>
        /// <param name="value">The tree value to encapsulate in this orphan.</param>
        /// <param name="children">The children of this orphan.</param>
        /// <param name="type">The type of this orphan.</param>
        internal PrtgOrphan(ITreeValue value, IEnumerable<PrtgOrphan> children, PrtgNodeType type) : base(value, children, type)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Creates a new <see cref="PrtgOrphan"/> if the specified <paramref name="value"/> and <paramref name="children"/>
        /// differ from the values stored in this object.<para/>
        /// Redirects to <see cref="Update(TValue, IEnumerable{PrtgOrphan})"/> to provide type safety.
        /// </summary>
        /// <param name="value">The tree value to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        internal override PrtgOrphan Update(ITreeValue value, IEnumerable<PrtgOrphan> children) => Update((TValue) value, children);

        /// <summary>
        /// Creates a new <see cref="PrtgOrphan"/> if the specified <paramref name="value"/> and <paramref name="children"/>
        /// differ from the values stored in this object.<para/>
        /// </summary>
        /// <param name="value">The tree value to compare against.</param>
        /// <param name="children">The children to compare against.</param>
        /// <returns>If the value or children do not match those stored in this object, a new object containing those values. Otherwise, this object.</returns>
        internal abstract PrtgOrphan<TValue> Update(TValue value, IEnumerable<PrtgOrphan> children);

        /// <summary>
        /// Creates a new <see cref="PrtgOrphan"/> if the specified tree <paramref name="value"/> differs from this object's <see cref="PrtgNode.Value"/>.
        /// </summary>
        /// <param name="value">The value to compare against.</param>
        /// <returns>If the value does not match this object's value, a new object containing the value. Otherwise, this object.</returns>
        internal PrtgOrphan<TValue> WithValue(TValue value) => Update(value, Children);
    }
}
