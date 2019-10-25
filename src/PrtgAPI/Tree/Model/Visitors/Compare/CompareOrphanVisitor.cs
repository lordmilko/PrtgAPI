using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a fully abstract visitor capable of accessing the orphans of a <see cref="CompareOrphan"/>.<para/>
    /// By default this class will only visit the single <see cref="CompareOrphan"/> passed into its Visit method.<para/>
    /// To utilize a visitor with a default recursive implementation please see <see cref="CompareOrphanWalker"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal abstract class CompareOrphanVisitor
    {
        /// <summary>
        /// Visits a single <see cref="CompareOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        public virtual void Visit(CompareOrphan orphan) => orphan.Accept(this);

        /// <summary>
        /// Visits a single <see cref="TreeNodeType.Node"/> <see cref="CompareOrphan"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal abstract void VisitOrphan(CompareOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="CompareOrphanCollection"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        protected internal abstract void VisitCollection(CompareOrphanCollection orphan);
    }
}
