using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for retrieving <see cref="NotificationAction"/> objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class NotificationActionParameters : TableParameters<NotificationAction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationActionParameters"/> class.
        /// </summary>
        public NotificationActionParameters() : base(Content.Notifications)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationActionParameters"/> class with one or more conditions to filter results by.
        /// </summary>
        /// <param name="filters">A list of conditions to filter results by.</param>
        public NotificationActionParameters(params SearchFilter[] filters) : this()
        {
            SearchFilters = filters.ToList();
        }

        internal NotificationActionParameters(int[] objectIds) : base(Content.Notifications)
        {
            AddFilters(new SearchFilter(Property.Id, objectIds));
        }
    }
}
