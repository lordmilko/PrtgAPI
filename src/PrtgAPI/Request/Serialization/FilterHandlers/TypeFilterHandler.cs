
namespace PrtgAPI.Request.Serialization.FilterHandlers
{
    class TypeFilterHandler : FilterHandler
    {
        public override bool TryFilter(FilterOperator op, string value)
        {
            if (value.ToLower() == "sensor")
                return false;

            if (op == FilterOperator.NotEquals || op == FilterOperator.GreaterThan || op == FilterOperator.LessThan)
                return false;

            return true;
        }

        public override string ValidDescription => "the most 'derived' object type is specified. Sensor objects can only be filtered by their individual 'sub-types' such as 'ping' and 'wmivolume'";
    }
}
