using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree.Converters.Tree
{
    /// <summary>
    /// Represents an type that provides methods for generating and encapsulating <see cref="ITreeValue"/> objects.
    /// </summary>
    abstract class ObjectFactory
    {
        protected PrtgClient client;

        /// <summary>
        /// Gets the type of object managed by this factory.
        /// </summary>
        public abstract PrtgNodeType Type { get; }

        /// <summary>
        /// Retrieves all objects of a certain <see cref="ITreeValue"/> type of a specified parent.
        /// </summary>
        /// <param name="parentId">The object to retrieve objects from.</param>
        /// <returns>A list of objects of a certain <see cref="ITreeValue"/> type under the specified parent.</returns>
        public abstract List<ITreeValue> Objects(int parentId);

        /// <summary>
        /// Asynchronously retrieves all objects of a certain <see cref="ITreeValue"/> type of a specified parent.
        /// </summary>
        /// <param name="parentId">The object to retrieve objects from.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A list of objects of a certain <see cref="ITreeValue"/> type under the specified parent.</returns>
        public abstract Task<List<ITreeValue>> ObjectsAsync(int parentId, CancellationToken token);

        /// <summary>
        /// Encapsulates an <see cref="ITreeValue"/> and its children as a <see cref="PrtgOrphan"/>.
        /// </summary>
        /// <param name="value">The object to encapsulate.</param>
        /// <param name="children">The children of the object.</param>
        /// <returns>A <see cref="PrtgOrphan"/> encapsulating the specified <see cref="ITreeValue"/> and children.</returns>
        public abstract PrtgOrphan Orphan(ITreeValue value, IEnumerable<PrtgOrphan> children);

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectFactory"/> class.
        /// </summary>
        /// <param name="client">The client this factory should use for executing API requests.</param>
        protected ObjectFactory(PrtgClient client)
        {
            this.client = client;
        }
    }
}