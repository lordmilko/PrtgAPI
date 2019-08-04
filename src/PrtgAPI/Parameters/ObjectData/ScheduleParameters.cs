using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for retrieving <see cref="Schedule"/> objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class ScheduleParameters : TableParameters<Schedule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleParameters"/> class.
        /// </summary>
        public ScheduleParameters() : base(Content.Schedules)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleParameters"/> class with one or more conditions to filter results by.
        /// </summary>
        /// <param name="filters">A list of conditions to filter results by.</param>
        public ScheduleParameters(params SearchFilter[] filters) : this()
        {
            SearchFilters = filters.ToList();
        }

        internal ScheduleParameters(int objectId) : base(Content.Schedules)
        {
            AddFilters(new SearchFilter(Property.Id, objectId));
        }
    }
}