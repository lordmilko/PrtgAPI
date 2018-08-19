namespace PrtgAPI.Request.Serialization.FilterHandlers
{
    class StringOrNumericFilterHandler : StringFilterHandler
    {
        public override bool TryFilter(FilterOperator op, string value)
        {
            double result;

            if (!double.TryParse(value, out result))
                return base.TryFilter(op, value);

            return true;
        }
    }
}
