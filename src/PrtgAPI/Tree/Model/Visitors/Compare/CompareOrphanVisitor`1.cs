using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents a fully abstract visitor capable of accessing the orphans of a <see cref="CompareOrphan"/>
    /// and produces a value of the type specified by the <typeparamref name="TResult"/> parameter.<para/>
    /// By default this class will only visit the single <see cref="CompareOrphan"/> passed into its Visit method.<para/>
    /// To utilize a visitor with a default recursive implementation on <see cref="CompareOrphan"/> objects please see <see cref="CompareOrphanRewriter"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of value to return.</typeparam>
    [ExcludeFromCodeCoverage]
    internal abstract class CompareOrphanVisitor<TResult>
    {
        /// <summary>
        /// Visits a single <see cref="CompareOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>If the orphan was not null, the value produced by visiting it. Otherwise, the default value of <typeparamref name="TResult"/>.</returns>
        public virtual TResult Visit(CompareOrphan orphan)
        {
            if (orphan != null)
                return orphan.Accept(this);

            return default(TResult);
        }

        /// <summary>
        /// Visits a single <see cref="TreeNodeType.Node"/> <see cref="CompareOrphan"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>If the orphan was not null, the value produced by visiting it. Otherwise, the default value of <typeparamref name="TResult"/>.</returns>
        protected internal abstract TResult VisitOrphan(CompareOrphan orphan);

        /// <summary>
        /// Visits a single <see cref="CompareOrphanCollection"/> and produces a value of type <typeparamref name="TResult"/>.
        /// </summary>
        /// <param name="orphan">The orphan to visit.</param>
        /// <returns>If the orphan was not null, the value produced by visiting it. Otherwise, the default value of <typeparamref name="TResult"/>.</returns>
        protected internal abstract TResult VisitCollection(CompareOrphanCollection orphan);
    }
}
