namespace PrtgAPI.Tree.Internal
{
    /// <summary>
    /// Represents an abstract collection of tree orphans and serves as the underlying store for a <see cref="NodeList"/>.
    /// </summary>
    internal abstract class OrphanList : TreeOrphan
    {
        /// <summary>
        /// Gets the number of elements contained in the <see cref="OrphanList"/>.
        /// </summary>
        internal abstract int Count { get; }

        /// <summary>
        /// Retrieves the orphan at the specified index.
        /// </summary>
        /// <param name="index">The index of the orphan to retrieve.</param>
        /// <returns>The orphan at the specified index.</returns>
        internal abstract TreeOrphan Item(int index);

        /// <summary>
        /// Initializes a new instance of the <see cref="OrphanList"/> class.
        /// </summary>
        protected OrphanList() : base((int) TreeNodeType.Collection)
        {
        }
    }
}
