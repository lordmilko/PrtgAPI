using System;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Helpers;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgUrl"/>  for retrieving a large amount of a series of smaller requests.
    /// </summary>
    public class PageableParameters : Parameters
    {
        /// <summary>
        /// Record number to start at.
        /// </summary>
        public int? Start
        {
            get { return (int?)this[Parameter.Start]; }
            set { this[Parameter.Start] = value; }
        }

        /// <summary>
        /// Maximum number of records to return. To retrieve all records in a single request, set this value to null.<para/>
        /// If this value is less than the total number of records available, additional records can be obtained by requesting the next <see cref="Page"/>.
        /// </summary>
        public int? Count
        {
            get
            {
                var val = this[Parameter.Count];

                if (val == null)
                    return null;

                if (val.ToString() == "*")
                    return null;

                return (int)val;
            }
            set
            {
                if (value == null)
                    this[Parameter.Count] = "*";
                else
                    this[Parameter.Count] = value;
            }
        }

        /// <summary>
        /// The page of the response to return. Requests can be split over multiple pages to increase the speed of each individual request.
        /// </summary>
        public int Page
        {
            get
            {
                if (Start == 0 || Start == null || Count == null)
                    return 1;
                return (Start.Value / Count.Value) + 1;
            }
            set { Start = (value - 1) * Count; }
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

        private SortDirection sortDirection = SortDirection.Ascending;

        private Property? GetSortBy()
        {
            var val = this[Parameter.SortBy];

            if (val == null)
                return null;

            if (val is Property)
                return (Property)val;

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
                        newVal = (Property?)val;
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
    }
}
