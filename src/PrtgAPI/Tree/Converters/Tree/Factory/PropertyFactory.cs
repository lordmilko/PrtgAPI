using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree.Converters.Tree
{
    /// <summary>
    /// Provides methods for generating and encapsulating <see cref="PropertyValuePair"/> objects.
    /// </summary>
    class PropertyFactory : ObjectFactory
    {
        /// <summary>
        /// Gets the type of object managed by this factory.
        /// </summary>
        public override PrtgNodeType Type => PrtgNodeType.Property;

        /// <summary>
        /// Retrieves all <see cref="PropertyValuePair"/> objects of a specified parent.
        /// </summary>
        /// <param name="parentId">The object to retrieve properties from.</param>
        /// <returns>A list of properties under the specified parent.</returns>
        public override List<ITreeValue> Objects(int parentId) =>
            client.GetObjectPropertiesRaw(parentId).Select(r => (ITreeValue) new PropertyValuePair(parentId, r.Key, r.Value)).ToList();

        /// <summary>
        /// Asynchronously retrieves all <see cref="PropertyValuePair"/> objects of a specified parent.
        /// </summary>
        /// <param name="parentId">The object to retrieve properties from.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A list of properties under the specified parent.</returns>
        public override async Task<List<ITreeValue>> ObjectsAsync(int parentId, CancellationToken token) =>
            (await client.GetObjectPropertiesRawAsync(parentId, token: token).ConfigureAwait(false)).Select(r => (ITreeValue) new PropertyValuePair(parentId, r.Key, r.Value)).ToList();

        /// <summary>
        /// Encapsulates a <see cref="PropertyValuePair"/> and its children as a <see cref="PropertyOrphan"/>.
        /// </summary>
        /// <param name="value">The property to encapsulate.</param>
        /// <param name="children">The children of the property.</param>
        /// <returns>A <see cref="PropertyOrphan"/> encapsulating the specified <see cref="PropertyValuePair"/> and children.</returns>
        public override PrtgOrphan Orphan(ITreeValue value, IEnumerable<PrtgOrphan> children) =>
            PrtgOrphan.Property((PropertyValuePair) value);

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyOrphan"/> class.
        /// </summary>
        /// <param name="client">The client this factory should use for executing API requests.</param>
        internal PropertyFactory(PrtgClient client) : base(client)
        {
        }
    }
}