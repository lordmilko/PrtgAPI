using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree.Converters.Tree
{
    /// <summary>
    /// Provides methods for generating and encapsulating <see cref="Group"/> objects.
    /// </summary>
    class GroupFactory : ObjectFactory
    {
        /// <summary>
        /// Gets the type of object managed by this factory.
        /// </summary>
        public override PrtgNodeType Type => PrtgNodeType.Group;

        /// <summary>
        /// Retrieves all <see cref="Group"/> objects of a specified parent.
        /// </summary>
        /// <param name="parentId">The object to retrieve groups from.</param>
        /// <returns>A list of groups under the specified parent.</returns>
        public override List<ITreeValue> Objects(int parentId) => client.GetGroups(Property.ParentId, parentId).Cast<ITreeValue>().ToList();

        /// <summary>
        /// Asynchronously retrieves all <see cref="Group"/> objects of a specified parent.
        /// </summary>
        /// <param name="parentId">The object to retrieve groups from.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A list of groups under the specified parent.</returns>
        public override async Task<List<ITreeValue>> ObjectsAsync(int parentId, CancellationToken token) =>
            (await client.GetGroupsAsync(Property.ParentId, parentId, token).ConfigureAwait(false)).Cast<ITreeValue>().ToList();

        /// <summary>
        /// Encapsulates a <see cref="Group"/> and its children as a <see cref="GroupOrphan"/>.
        /// </summary>
        /// <param name="value">The group to encapsulate.</param>
        /// <param name="children">The children of the group.</param>
        /// <returns>A <see cref="GroupOrphan"/> encapsulating the specified <see cref="Group"/> and children.</returns>
        public override PrtgOrphan Orphan(ITreeValue value, IEnumerable<PrtgOrphan> children) =>
            PrtgOrphan.Group((Group) value, children);

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupFactory"/> class.
        /// </summary>
        /// <param name="client">The client this factory should use for executing API requests.</param>
        internal GroupFactory(PrtgClient client) : base(client)
        {
        }
    }
}
