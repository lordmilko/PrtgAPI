using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Tree.Internal;

namespace PrtgAPI.Tree.Converters.Tree
{
    /// <summary>
    /// Provides methods for generating and encapsulating <see cref="NotificationTrigger"/> objects.
    /// </summary>
    class TriggerFactory : ObjectFactory
    {
        /// <summary>
        /// Gets the type of object managed by this factory.
        /// </summary>
        public override PrtgNodeType Type => PrtgNodeType.Trigger;

        /// <summary>
        /// Retrieves all <see cref="NotificationTrigger"/> objects of a specified parent.
        /// </summary>
        /// <param name="parentId">The object to retrieve notification triggers from.</param>
        /// <returns>A list of notification triggers under the specified parent.</returns>
        public override List<ITreeValue> Objects(int parentId) =>
            client.GetNotificationTriggers(parentId).Where(t => !t.Inherited).Cast<ITreeValue>().ToList();

        /// <summary>
        /// Asynchronously retrieves all <see cref="NotificationTrigger"/> objects of a specified parent.
        /// </summary>
        /// <param name="parentId">The object to retrieve notification triggers from.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A list of notification triggers under the specified parent.</returns>
        public override async Task<List<ITreeValue>> ObjectsAsync(int parentId, CancellationToken token) =>
            (await client.GetNotificationTriggersAsync(parentId, token).ConfigureAwait(false)).Where(t => !t.Inherited).Cast<ITreeValue>().ToList();

        /// <summary>
        /// Encapsulates a <see cref="NotificationTrigger"/> and its children as a <see cref="TriggerOrphan"/>.
        /// </summary>
        /// <param name="value">The notification trigger to encapsulate.</param>
        /// <param name="children">The children of the notification trigger.</param>
        /// <returns>A <see cref="TriggerOrphan"/> encapsulating the specified <see cref="NotificationTrigger"/> and children.</returns>
        public override PrtgOrphan Orphan(ITreeValue value, IEnumerable<PrtgOrphan> children) =>
            PrtgOrphan.Trigger((NotificationTrigger) value);

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerFactory"/> class.
        /// </summary>
        /// <param name="client">The client this factory should use for executing API requests.</param>
        internal TriggerFactory(PrtgClient client) : base(client)
        {
        }
    }
}
