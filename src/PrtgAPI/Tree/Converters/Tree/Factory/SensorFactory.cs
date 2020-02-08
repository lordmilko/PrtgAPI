using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree.Converters.Tree
{
    /// <summary>
    /// Provides methods for generating and encapsulating <see cref="Sensor"/> objects.
    /// </summary>
    class SensorFactory : ObjectFactory
    {
        /// <summary>
        /// Gets the type of object managed by this factory.
        /// </summary>
        public override PrtgNodeType Type => PrtgNodeType.Sensor;

        /// <summary>
        /// Retrieves all <see cref="Sensor"/> objects of a specified parent.
        /// </summary>
        /// <param name="parentId">The object to retrieve sensors from.</param>
        /// <returns>A list of sensors under the specified parent.</returns>
        public override List<ITreeValue> Objects(int parentId) => client.GetSensors(Property.ParentId, parentId).Cast<ITreeValue>().ToList();

        /// <summary>
        /// Asynchronously retrieves all <see cref="Sensor"/> objects of a specified parent.
        /// </summary>
        /// <param name="parentId">The object to retrieve sensors from.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A list of sensors under the specified parent.</returns>
        public override async Task<List<ITreeValue>> ObjectsAsync(int parentId, CancellationToken token) =>
            (await client.GetSensorsAsync(Property.ParentId, parentId, token).ConfigureAwait(false)).Cast<ITreeValue>().ToList();

        /// <summary>
        /// Encapsulates a <see cref="Sensor"/> and its children as a <see cref="SensorOrphan"/>.
        /// </summary>
        /// <param name="value">The sensor to encapsulate.</param>
        /// <param name="children">The children of the sensor.</param>
        /// <returns>A <see cref="SensorOrphan"/> encapsulating the specified <see cref="Sensor"/> and children.</returns>
        public override PrtgOrphan Orphan(ITreeValue value, IEnumerable<PrtgOrphan> children) =>
            PrtgOrphan.Sensor((ISensor) value, children);

        /// <summary>
        /// Initializes a new instance of the <see cref="SensorFactory"/> class.
        /// </summary>
        /// <param name="client">The client this factory should use for executing API requests.</param>
        internal SensorFactory(PrtgClient client) : base(client)
        {
        }
    }
}
