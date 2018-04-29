using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for retrieving <see cref="Schedule"/> objects.
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

        internal ScheduleParameters(int objectId) : base(Content.Schedules)
        {
            SearchFilter = new[] {new SearchFilter(Property.Id, objectId)};
        }
    }
}