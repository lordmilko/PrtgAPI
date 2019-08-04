namespace PrtgAPI.Request.Serialization.FilterHandlers
{
    abstract class FilterHandler
    {
        public abstract bool TryFilter(FilterOperator op, string value);

        public abstract string ValidDescription { get; }

        public virtual bool Unsupported => false;
    }
}
