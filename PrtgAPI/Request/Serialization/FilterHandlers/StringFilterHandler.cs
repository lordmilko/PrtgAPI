namespace PrtgAPI.Request.Serialization.FilterHandlers
{
    class StringFilterHandler : FilterHandler
    {
        public override bool TryFilter(FilterOperator op, string value)
        {
            if (op == FilterOperator.NotEquals || op == FilterOperator.GreaterThan || op == FilterOperator.LessThan)
                return false;

            return true;
        }

        public override string ValidDescription => "string values 'Equal' or 'Contain' other string values";
    }
}
