using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree.Converters.Tree
{
    /// <summary>
    /// Provides methods for generating and encapsulating <see cref="Device"/> objects.
    /// </summary>
    class DeviceFactory : ObjectFactory
    {
        /// <summary>
        /// Gets the type of object managed by this factory.
        /// </summary>
        public override PrtgNodeType Type => PrtgNodeType.Device;

        /// <summary>
        /// Retrieves all <see cref="Device"/> objects of a specified parent.
        /// </summary>
        /// <param name="parentId">The object to retrieve devices from.</param>
        /// <returns>A list of devices under the specified parent.</returns>
        public override List<ITreeValue> Objects(int parentId) => client.GetDevices(Property.ParentId, parentId).Cast<ITreeValue>().ToList();

        /// <summary>
        /// Asynchronously retrieves all <see cref="Device"/> objects of a specified parent.
        /// </summary>
        /// <param name="parentId">The object to retrieve devices from.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A list of devices under the specified parent.</returns>
        public override async Task<List<ITreeValue>> ObjectsAsync(int parentId, CancellationToken token) =>
            (await client.GetDevicesAsync(Property.ParentId, parentId, token).ConfigureAwait(false)).Cast<ITreeValue>().ToList();

        /// <summary>
        /// Encapsulates a <see cref="Device"/> and its children as a <see cref="DeviceOrphan"/>.
        /// </summary>
        /// <param name="value">The device to encapsulate.</param>
        /// <param name="children">The children of the device.</param>
        /// <returns>A <see cref="DeviceOrphan"/> encapsulating the specified <see cref="Device"/> and children.</returns>
        public override PrtgOrphan Orphan(ITreeValue value, IEnumerable<PrtgOrphan> children) =>
            PrtgOrphan.Device((Device) value, children);

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceFactory"/> class.
        /// </summary>
        /// <param name="client">The client this factory should use for executing API requests.</param>
        internal DeviceFactory(PrtgClient client) : base(client)
        {
        }
    }
}
