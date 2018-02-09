using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PrtgAPI.Helpers;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/> for retrieving data stored in tables.
    /// </summary>
    /// <typeparam name="T">The type of PRTG Object to retrieve.</typeparam>
    public class TableParameters<T> : ContentParameters<T> where T : ObjectTable
    {
        /// <summary>
        /// Filter objects to those with a <see cref="Property"/> of a certain value. Specify multiple filters to limit results further.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public virtual SearchFilter[] SearchFilter
        {
            get { return (SearchFilter[])this[Parameter.FilterXyz]; }
            set { this[Parameter.FilterXyz] = value; }
        }

        /// <summary>
        /// <see cref="Property"/> to sort response by.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public Property? SortBy
        {
            get { return GetSortBy(); }
            set { SetSortBy(value, true); }
        }

        private SortDirection sortDirection = SortDirection.Ascending;

        /// <summary>
        /// The direction to sort returned objects by. By default, when <see cref="SortBy"/> is specified objects are sorted ascending from lowest to highest. 
        /// </summary>
        public SortDirection SortDirection
        {
            get { return sortDirection; }
            set
            {
                sortDirection = value;

                SetSortBy(null, false);
            }
        }

        private Property? GetSortBy()
        {
            var val = this[Parameter.SortBy];

            if (val == null)
                return null;

            if (val is Property)
                return (Property) val;

            var str = val.ToString();

            if (str.StartsWith("-"))
            {
                var description = str.Substring(1);

                return description.DescriptionToEnum<Property>();
            }

            return str.DescriptionToEnum<Property>();
        }

        private void SetSortBy(Property? newVal, bool setSort)
        {
            if (!setSort)
            {
                var val = this[Parameter.SortBy];

                if (val != null)
                {
                    if (val is Property?)
                        newVal = (Property?) val;
                    else
                    {
                        var str = val.ToString();

                        if (str.StartsWith("-"))
                            str = str.Substring(1);

                        newVal = str.DescriptionToEnum<Property>();
                    }
                }
            }

            if (sortDirection == SortDirection.Ascending)
                this[Parameter.SortBy] = newVal;
            else
            {
                if (newVal == null)
                    this[Parameter.SortBy] = null;
                else
                    this[Parameter.SortBy] = $"-{((Enum)newVal).GetDescription().ToLower()}";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableParameters{T}"/> class.
        /// </summary>
        protected TableParameters(Content content) : base(content)
        {
        }

        /// <summary>
        /// Set the value of a filter for a property.
        /// </summary>
        /// <param name="property">The property to set the value of.</param>
        /// <param name="value">The value to filter for. If this value is null the filter will be removed.</param>
        protected void SetFilterValue(Property property, object value)
        {
            var filters = (List<SearchFilter>)this[Parameter.FilterXyz] ?? new List<SearchFilter>();

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
        /// Retrieve the value of a filter for a property.
        /// </summary>
        /// <param name="property">The property to retrieve the value of.</param>
        /// <returns></returns>
        protected object GetFilterValue(Property property)
        {
            var filters = (List<SearchFilter>)this[Parameter.FilterXyz];

            var item = filters?.FirstOrDefault(f => f.Property == property);

            return item?.Value;
        }

        /// <summary>
        /// Set the value of a <see cref="ParameterType.MultiParameter"/> property.
        /// </summary>
        /// <typeparam name="TArray">The type of array to store.</typeparam>
        /// <param name="property">The property to set the value of.</param>
        /// <param name="value">The values to filter for. If this value is null the filters will be removed.</param>
        protected void SetMultiParameterFilterValue<TArray>(Property property, TArray[] value)
        {
            var filters = (List<SearchFilter>)this[Parameter.FilterXyz] ?? new List<SearchFilter>();

            var items = filters.Where(f => f.Property == property).ToList();

            if (items.Any())
            {
                foreach (var item in items)
                {
                    filters.Remove(item);
                }
            }

            if (value != null)
            {
                items = value.Select(v => new SearchFilter(property, v)).ToList();
                filters.AddRange(items);
            }

            this[Parameter.FilterXyz] = filters;
        }

        /// <summary>
        /// Retrieve the value of a <see cref="ParameterType.MultiParameter"/> property.
        /// </summary>
        /// <typeparam name="TArray">The type of array that was previously stored.</typeparam>
        /// <param name="property">The property to retrieve the value of.</param>
        /// <returns></returns>
        protected TArray[] GetMultiParameterFilterValue<TArray>(Property property)
        {
            var filters = (List<SearchFilter>)this[Parameter.FilterXyz];

            var items = filters?.Where(f => f.Property == property).ToList();

            return items?.Select(v => v.Value).Cast<TArray>().ToArray();
        }
    }
}
