using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;
using PrtgAPI.Parameters;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq.Expressions
{
    class QueryLogHelper : QueryHelper<Log>
    {
        private Stack<SearchFilter> usedIds = new Stack<SearchFilter>();

        private bool strict;

        public QueryLogHelper(bool strict)
        {
            this.strict = strict;
        }

        public override Expression FlagKeep(Expression expr)
        {
            var flagger = new PropertyFlagger(Property.DateTime);
            var result = flagger.Visit(expr);
            return result;
        }

        public override List<List<SearchFilter>> AdjustFilters(List<SearchFilter> filters)
        {
            bool removed = false;

            var newFilters = filters.Select(f =>
            {
                var newF = AdjustFilter(f);

                if (newF == null && f.Property == Property.DateTime)
                    removed = true;

                return newF;
            }).Where(f => f != null).ToList();

            return ValidateFilters(filters, newFilters, removed);
        }

        public override void FixupParameters(IParameters parameters)
        {
            var filters = parameters[Parameter.FilterXyz] as List<SearchFilter>;

            if (filters != null && filters.Count > 0)
            {
                var idFilters = filters.Where(f => f.Property == Property.Id && f.Operator == FilterOperator.Equals).ToList();

                if (idFilters.Count > 0)
                {
                    //If we had any illegal ID filters they would have been stripped out long ago, so we won't crash here
                    //from our prior FilterSet initialization in AdjustFilters
                    var unusedFilter = idFilters.First(i => !usedIds.Contains(i));

                    //It's OK that we get rid of all of the ID filters, since this was just a copy of the source SearchFilter array
                    parameters[Parameter.Id] = unusedFilter.Value;
                    parameters[Parameter.FilterXyz] = filters.Where(f => f.Property != Property.Id && f.Property != Property.Message).ToList();

                    usedIds.Push(unusedFilter);
                }
                else
                {
                    //Any Id filter remaining contains an illegal operation. Filter them all out
                    var goodFilters = filters.Where(f => f.Property != Property.Id && f.Property != Property.Message);
                    parameters[Parameter.FilterXyz] = goodFilters.ToList();
                }

                //Start and End date have been stored as DateTimes, however they need to be sent as specially formatted strings.
                //To fix this, we set the value with the existing value
                var logParameters = (LogParameters)parameters;
                logParameters.StartDate = logParameters.StartDate;
                logParameters.EndDate = logParameters.EndDate;
            }
        }

        public override IEqualityComparer<Log> GetComparer()
        {
            return new LogEqualityComparer();
        }

        private SearchFilter AdjustFilter(SearchFilter filter)
        {
            if (filter.Property == Property.DateTime)
            {
                if (filter.Operator == FilterOperator.GreaterThan)
                    return new SearchFilter(Property.StartDate, filter.Value);

                if (filter.Operator == FilterOperator.LessThan)
                    return new SearchFilter(Property.EndDate, filter.Value);

                return null; //Illegal filter. Calculate client side
            }

            return filter;
        }

        private List<List<SearchFilter>> ValidateFilters(List<SearchFilter> originalFilters, List<SearchFilter> newFilters, bool removed)
        {
            var start = newFilters.Where(f => f.Property == Property.StartDate).ToList();
            var end = newFilters.Where(f => f.Property == Property.EndDate).ToList();

            if (strict)
            {
                var supportedFilters = new[]
                {
                    Property.Id,
                    Property.StartDate,
                    Property.EndDate,
                    Property.Status,
                    Property.RecordAge
                };

                var unsupported = newFilters.Where(f => supportedFilters.All(s => s != f.Property)).ToList();

                if (unsupported.Count > 0)
                    throw Error.LogUnsupportedFilter(unsupported);
            }

            if (removed && start.Count == 0 && end.Count == 0)
            {
                var equals = originalFilters.Where(f => f.Property == Property.DateTime && f.Operator == FilterOperator.Equals).ToList();

                if (equals.Count == 1)
                {
                    newFilters.Add(new SearchFilter(Property.StartDate, equals.Single().Value));
                    newFilters.Add(new SearchFilter(Property.EndDate, equals.Single().Value));
                }
                else
                    throw new InvalidOperationException("Cannot retrieve logs as all specified date filters were invalid. At least one end of a valid date range must be specified.");
            }

            if (start.Count > 1)
            {
                //If no end is specified, logs are retrieved from the current date and time. We don't care if we have multiple starts
                var dates = start.Select(f => (DateTime) f.Value).ToList();

                if (strict)
                    throw Error.LogDuplicateDateRangeStart(dates);

                var ordered = dates.OrderBy(d => d).Skip(1).ToList();

                newFilters = newFilters.Where(f => f.Property != Property.StartDate || ordered.All(d => !f.Value.Equals(d))).ToList();
            }

            if (end.Count > 1)
            {
                //If no start is specified, logs are retrieved from the beginning of all time. We don't care if we have multiple ends
                var dates = end.Select(f => (DateTime) f.Value).ToList();

                if (strict)
                    throw Error.LogDuplicateDateRangeEnd(dates);

                var ordered = dates.OrderByDescending(d => d).Skip(1).ToList();

                newFilters = newFilters.Where(f => f.Property != Property.EndDate || ordered.All(d => !f.Value.Equals(d))).ToList();
            }

            var idFilters = newFilters.Where(f => f.Property == Property.Id).ToList();

            if (idFilters.Count > 1)
            {
                if (strict)
                    throw Error.LogDuplicateId(idFilters.Select(f => f.Value).ToList());

                //Can't filter on multiple IDs in a single request. Execute a single request for each ID.
                //We're still keeping all of the IDs we'll be targeting in our parameters; we'll select a single
                //target for each parameter set when FixupParameters is called
                return idFilters.Select(id => newFilters).ToList();
            }

            return new List<List<SearchFilter>>
            {
                newFilters
            };
        }

        public override bool CanLimitProperties(List<List<SearchFilter>> filterSets)
        {
            bool canLimit = filterSets.Count == 1;

            if (!canLimit)
            {
                Logger.Log("Ignoring filtered log properties as multiple filter sets exist", Indentation.Two);
            }

            return canLimit;
        }
    }
}
