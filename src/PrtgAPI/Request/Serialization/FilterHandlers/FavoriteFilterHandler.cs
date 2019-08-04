namespace PrtgAPI.Request.Serialization.FilterHandlers
{
    class FavoriteFilterHandler : FilterHandler
    {
        public override bool TryFilter(FilterOperator op, string value)
        {
            if (op == FilterOperator.NotEquals && value == "0")
                return true;

            if (value == "0" || (op != FilterOperator.Equals && op != FilterOperator.Contains))
                return false;

            return true;
        }

        public override string ValidDescription => $"'{Property.Favorite}' {FilterOperator.Equals.ToString().ToLower()} '{true}'";
    }
}
