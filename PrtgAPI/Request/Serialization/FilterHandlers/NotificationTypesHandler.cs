using System.Diagnostics.CodeAnalysis;

namespace PrtgAPI.Request.Serialization.FilterHandlers
{
    class NotificationTypesHandler : FilterHandler
    {
        public override bool TryFilter(FilterOperator op, string value)
        {
            return false;
        }

        [ExcludeFromCodeCoverage]
        public override string ValidDescription => $"'{Property.NotificationTypes}' {FilterOperator.Equals.ToString().ToLower()} '{true}'";

        public override bool Unsupported => true;
    }
}
