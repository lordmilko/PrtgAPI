using System;
using System.Diagnostics.CodeAnalysis;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for retrieving a large number of objects over a series of smaller requests.
    /// </summary>
    public class PageableParameters : BaseParameters
    {
        /// <summary>
        /// Represents the default number of items to retrieve per request when streaming.
        /// </summary>
        public const int DefaultPageSize = 500;

        private int startOffset;

        /// <summary>
        /// Gets the index of the first object of this data type.<para/>
        /// When specifying <see cref="Start"/>, this value should be taken into account to ensure the correct index is targeted.
        /// Based on whether the start offset is "0" or "1", a start position of "2" could be interpreted by PRTG as either "3" or "2".
        /// </summary>
        public int StartOffset
        {
            get { return startOffset; }
            internal set
            {
                startOffset = value;
                Start = value;
            }
        }

        /// <summary>
        /// Gets or sets the record index to start at, relative to <see cref="StartOffset"/>.<para/>
        /// When modifying this property, <see cref="StartOffset"/> should be taken into account to ensure the correct index is being targeted.<para/>
        /// If a large <see cref="Count"/> is specified that overlaps with the records skipped by this value, this value may be partially or completely ignored.
        /// </summary>
        public int? Start
        {
            get { return (int?)this[Parameter.Start]; }
            set { this[Parameter.Start] = value; }
        }

        /// <summary>
        /// Gets or sets the number of records to return per request. If this value is null, all records will be returned.<para/>
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
        /// Gets or sets the page of the response to return. Requests can be split over multiple pages to increase the speed of each individual request.<para/>
        /// Modifying this value increments or decrements <see cref="Start"/> by <see cref="Count"/> records. If <see cref="Count"/> is null, modifying this value will have no effect.
        /// </summary>
        public int Page
        {
            get
            {
                if (Count <= 0)
                    return 0;

                if (Start == 0 || Start == null || Count == null)
                    return 1;
                return (Start.Value / Count.Value) + 1;
            }
            set
            {
                if (Count <= 0)
                    throw new InvalidOperationException($"Count must be 'null' (unlimited) or greater than 0 in order to specify request page.");

                var baseStart = Start;

                //If we're now at 503, then our initial start was 3
                if (Count != null)
                    baseStart %= Count;

                Start = (value - 1) * Count;

                if (baseStart != null)
                {
                    //Count was null, so the page didn't increase. Restore our baseStart
                    if (Start == null)
                        Start = baseStart;
                    else //If we're now at 1000, bump it up to 1003
                        Start += baseStart;
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of records to retrieve per <see cref="Page"/> when streaming.<para/>If <see cref="Count"/>
        /// is lower than this value, all records will be retrieved in a single request and this value will be ignored.
        /// </summary>
        public int PageSize { get; set; } = DefaultPageSize;

        /// <summary>
        /// Gets or sets the <see cref="Property"/> to sort the response by.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public Property? SortBy
        {
            get { return GetSortBy(); }
            set { SetSortBy(value, true); }
        }

        /// <summary>
        /// Gets or sets the direction to sort the returned objects by. By default, when <see cref="SortBy"/> is specified objects are sorted ascending from lowest to highest. 
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

        internal virtual void ShallowClone(PageableParameters newParameters)
        {
            foreach (var parameter in GetParameters())
                newParameters[parameter.Key] = parameter.Value;

            newParameters.Cookie = Cookie;
            newParameters.PageSize = PageSize;
            newParameters.sortDirection = sortDirection;
            newParameters.startOffset = startOffset;
        }
    }
}
