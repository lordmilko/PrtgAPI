using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Linq;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for retrieving data stored in tables.
    /// </summary>
    /// <typeparam name="T">The type of PRTG Object to retrieve.</typeparam>
    public abstract class TableParameters<T> : ContentParameters<T> where T : ITableObject, IObject
    {
        /// <summary>
        /// Gets or sets a collection of search filters used to limit search results to those that match certain criteria. This property is self initializing.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public List<SearchFilter> SearchFilters
        {
            get
            {
                var val = this[Parameter.FilterXyz];

                if (val is List<SearchFilter>)
                    return (List<SearchFilter>)val;

                var newList = (val as IEnumerable<SearchFilter>)?.ToList() ?? new List<SearchFilter>();

                this[Parameter.FilterXyz] = newList;

                return newList;
            }
            set { SetSearchFilter(value?.WithoutNull()); }
        }

        internal virtual void SetSearchFilter(List<SearchFilter> value)
        {
            this[Parameter.FilterXyz] = value?.ToList();
        }

        /// <summary>
        /// Adds one or more filters to the list of filters included in the parameter set.
        /// </summary>
        /// <param name="filters">One or more filters to add to the parameter set.</param>
        public void AddFilters(params SearchFilter[] filters)
        {
            if (filters == null)
                throw new ArgumentNullException(nameof(filters));

            if (this[Parameter.FilterXyz] == null)
                SearchFilters = filters.ToList();
            else
            {
                SearchFilters = SearchFilters.Union(filters).ToList();
            }
        }

        /// <summary>
        /// Removes one or filters from the list of filters included in the parameter set.
        /// </summary>
        /// <param name="filters">One or more filters to remove from the parameter set.</param>
        /// <returns>True if any filters were removed. otherwise, false.</returns>
        public bool RemoveFilters(params SearchFilter[] filters)
        {
            if (filters == null)
                throw new ArgumentNullException(nameof(filters));

            if (this[Parameter.FilterXyz] == null)
                return false;

            bool anyMatch = false;

            var newFilters = SearchFilters.Where(f => filters.All(r =>
            {
                if (f == r)
                    anyMatch = true;

                return f != r;
            })).ToArray();

            if (anyMatch)
                SearchFilters = newFilters.ToList();

            return anyMatch;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableParameters{T}"/> class.
        /// </summary>
        protected TableParameters(Content content) : base(content)
        {
        }

        /// <summary>
        /// Sets the value of a filter for a property.
        /// </summary>
        /// <param name="property">The property to set the value of.</param>
        /// <param name="value">The value to filter for. If this value is null the filter will be removed.</param>
        protected void SetFilterValue(Property property, object value)
        {
            var filters = this[Parameter.FilterXyz] as List<SearchFilter> ?? (((IEnumerable<SearchFilter>)this[Parameter.FilterXyz])?.ToList() ?? new List<SearchFilter>());

            var item = filters.FirstOrDefault(f => f.Property == property);

            if (item != null)
            {
                if (value == null)        //Remove the value
                    filters.Remove(item);
                else
                    item.Value = value;   //Update the value of the existing filter
            }
            else
            {
                if (value != null)
                    filters.Add(new SearchFilter(property, value));
            }

            this[Parameter.FilterXyz] = filters;
        }

        /// <summary>
        /// Retrieves the value of a filter for a property.
        /// </summary>
        /// <param name="property">The property to retrieve the value of.</param>
        /// <returns>The value of the specified filter property, or null if no such filter exists.</returns>
        protected object GetFilterValue(Property property)
        {
            var filters = (IEnumerable<SearchFilter>)this[Parameter.FilterXyz];

            var item = filters?.FirstOrDefault(f => f.Property == property);

            return item?.Value;
        }

        /// <summary>
        /// Sets the value of a <see cref="ParameterType.MultiParameter"/> property.
        /// </summary>
        /// <typeparam name="TArray">The type of array to store.</typeparam>
        /// <param name="property">The property to set the value of.</param>
        /// <param name="value">The values to filter for. If this value is null the filters will be removed.</param>
        protected void SetMultiParameterFilterValue<TArray>(Property property, TArray[] value)
        {
            var filters = this[Parameter.FilterXyz] as List<SearchFilter> ?? (((IEnumerable<SearchFilter>)this[Parameter.FilterXyz])?.ToList() ?? new List<SearchFilter>());

            var items = filters.Where(f => f.Property == property).ToList();

            foreach (var item in items)
            {
                filters.Remove(item);
            }

            if (value != null)
            {
                items = value.Select(v => new SearchFilter(property, v)).ToList();
                filters.AddRange(items);
            }

            this[Parameter.FilterXyz] = filters;
        }

        /// <summary>
        /// Retrieves the value of a <see cref="ParameterType.MultiParameter"/> property.
        /// </summary>
        /// <typeparam name="TArray">The type of array that was previously stored.</typeparam>
        /// <param name="property">The property to retrieve the value of.</param>
        /// <returns>The values of the specified filter property, or null if no such filter exists.</returns>
        protected TArray[] GetMultiParameterFilterValue<TArray>(Property property) where TArray : struct
        {
            var filters = (IEnumerable<SearchFilter>)this[Parameter.FilterXyz];

            var items = filters?.Where(f => f.Property == property).ToList();

            return items?.SelectMany(GetValues<TArray>).Where(v => v != null).Cast<TArray>().ToArray();
        }

        private IEnumerable<TArray?> GetValues<TArray>(SearchFilter filter) where TArray : struct
        {
            if (filter.Value.IsIEnumerable())
            {
                foreach (var val in filter.Value.ToIEnumerable())
                    yield return val as TArray?;
            }
            else
                yield return filter.Value as TArray?;
        }
    }
}
