using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Attributes;
using PrtgAPI.Linq.Expressions.Visitors;
using PrtgAPI.Linq.Expressions.Visitors.Parameters;
using PrtgAPI.Reflection;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq.Expressions
{
    class ParameterBuilder<T>
    {
        private QueryHelper<T> queryHelper;

        public List<List<SearchFilter>> FilterSets { get; } = new List<List<SearchFilter>>();

        public List<Property> Properties { get; private set; } = new List<Property>();

        public SortDirection? SortDirection { get; private set; }
        public Property? SortProperty { get; private set; }

        public bool HasIllegalServerFilters { get; private set; }

        public int? Count { get; private set; }
        public int? Skip { get; private set; }

        private bool strict;

        public ParameterBuilder(QueryHelper<T> queryHelper, bool strict)
        {
            this.queryHelper = queryHelper;
            this.strict = strict;
        }

        public Expression Build(Expression expression)
        {
            Logger.Log("Identifying parameter components", Indentation.One);
            
            var legalFilterParser = new LegalFilterParser(strict);

            var propertyExpression = ParseColumns(expression);
            var flaggedExpression = queryHelper?.FlagKeep(propertyExpression) ?? propertyExpression;
            var legalExpression = GetLegalExpression(legalFilterParser, flaggedExpression);
            var filterExpression = ParseFilters(legalExpression);
            var orderExpression = ParseSort(filterExpression);
            var countExpression = ParseCount(orderExpression);

            AddIllegalExpressions(legalFilterParser);
            RemoveInvalidFilters();

            return countExpression;
        }

        private Expression GetLegalExpression(LegalFilterParser legalFilterParser, Expression expression)
        {
            Logger.Log("Identifying maximal legal expression", Indentation.Two);
            var legalExpression = legalFilterParser.Visit(expression);

            if (legalFilterParser.IsIllegal)
                HasIllegalServerFilters = legalFilterParser.IsIllegal;

            return legalExpression;
        }

        private Expression ParseColumns(Expression expression)
        {
            var columnParser = new ColumnExpressionParser();

            Logger.Log("Identifying column limiters", Indentation.Two);
            var propertyExpression = columnParser.Visit(expression);

            Properties = columnParser.HasSelect ? columnParser.Members.Select(GetProperty).ToList() : new List<Property>();

            if (Properties.Count == 0)
                Logger.Log("Did not identify any column limiters", Indentation.Three);
            else
                Logger.Log($"Limiting by properties {Properties.ToQuotedList()}", Indentation.Three);

            return propertyExpression;
        }

        private Expression ParseFilters(Expression expression)
        {
            Logger.Log("Identifying query filters", Indentation.Two);

            var filterExpressionParser = new FilterExpressionParser();

            Logger.Log("Identifying filter candidates", Indentation.Three);
            var filterExpression = filterExpressionParser.Visit(expression);

            var conditions = filterExpressionParser.Conditions;

            Logger.Log($"Found {conditions.Count} filter candidates", Indentation.Four, conditions);

            var adjustedFilters = GetFilters(conditions);

            if (adjustedFilters.Count > 0)
                FilterSets.AddRange(adjustedFilters);

            return filterExpression;
        }

        private Expression ParseSort(Expression expression)
        {
            var sortExpressionParser = new SortExpressionParser();

            Logger.Log("Identifying sort order", Indentation.Two);
            var orderExpression = sortExpressionParser.Visit(expression);

            if (sortExpressionParser.SortProperty != null)
            {
                Logger.Log($"Sorting by {sortExpressionParser.SortProperty} {sortExpressionParser.SortDirection}", Indentation.Three);
                SortProperty = GetProperty(sortExpressionParser.SortProperty);
                SortDirection = sortExpressionParser.SortDirection;
            }

            return orderExpression;
        }

        private Expression ParseCount(Expression expression)
        {
            var countExpressionParser = new CountExpressionParser();

            Logger.Log("Identifying count limiters", Indentation.Two);
            var countExpression = countExpressionParser.Visit(expression);

            if (countExpressionParser.Skip != null)
            {
                Logger.Log($"Skipping first {countExpressionParser.Skip} records", Indentation.Three);
                Skip = countExpressionParser.Skip;
            }

            if (countExpressionParser.Count != null)
            {
                Logger.Log($"Limiting count to {countExpressionParser.Count} records", Indentation.Three);
                Count = countExpressionParser.Count;
            }

            return countExpression;
        }

        private void RemoveInvalidFilters()
        {
            foreach (var set in FilterSets)
            {
                var remove = new List<SearchFilter>();

                foreach (var filter in set)
                {
                    var attrib = filter.Property.GetEnumAttribute<FilterHandlerAttribute>();

                    if (attrib != null)
                    {
                        FilterOperator op = filter.Operator;
                        string value = SearchFilter.GetValue(filter.Property, filter.Operator, filter.Value);

                        if (!attrib.Handler.TryFilter(op, value))
                        {
                            if (strict)
                            {
                                if (attrib.Handler.Unsupported)
                                    throw Error.UnsupportedProperty(filter.Property);

                                throw Error.InvalidFilterValue(filter.Property, op, filter.Value.ToString(), attrib.Handler.ValidDescription);
                            }

                            remove.Add(filter);
                        }
                    }
                }

                foreach (var r in remove)
                {
                    set.Remove(r);
                }
            }
        }

        private List<List<SearchFilter>> GetFilters(List<Expression> conditions)
        {
            Logger.Log("Evaluating filter candidates", Indentation.Four);

            //Remove conditions that were derived from methods that are not LinqExpressions.
            //For example, in Take(3).Where(s => s.Id == 1001), s.Id is a native MemberExpression
            //that was never transformed into a PropertyExpression. This is almost identical to
            //the check that is done in IsLegalFilter, however by immediately removing such candidates
            //we don't flag HasIllegalServerFilters, thereby allowing us to still do things such as set a Count
            conditions = conditions.Where(c =>
            {
                var valid = ExpressionSearcher.Search<PropertyExpression>(c).Count > 0;

                if (!valid)
                {
                    Logger.Log($"Removing condition {c} as it does not contain any property expressions", Indentation.Five);

                    if (strict)
                        throw Error.InvalidPropertyCount(c, new List<PropertyExpression>());
                }

                return valid;
            }).ToList();

            var legalFilters = conditions
                .Where(IsLegalFilter)
                .Select(GetReducedCondition)
                .Where(IsSingleCondition)
                .SelectMany(GetSearchFilter).ToList();

            var adjustedFilters = queryHelper?.AdjustFilters(legalFilters) ?? new List<List<SearchFilter>>
            {
                legalFilters
            };

            return adjustedFilters;
        }

        private void AddIllegalExpressions(LegalFilterParser parser)
        {
            foreach (var expr in parser.IllegalExpressionsForSplitRequest)
            {
                var newLegalParser = new LegalFilterParser(strict);
                var newFilterParser = new FilterExpressionParser();

                var legalExpression = newLegalParser.Visit(expr);
                var filteredExpression = newFilterParser.Visit(legalExpression);

                FilterSets.AddRange(GetFilters(newFilterParser.Conditions));

                AddIllegalExpressions(newLegalParser);
            }
        }

        private Expression GetReducedCondition(Expression condition)
        {
            var reducedCondition = ConditionReducer.Reduce(condition, strict);

            return reducedCondition;
        }

        private bool IsSingleCondition(Expression condition)
        {
            var subconditions = ExpressionSearcher.Search(
                condition,
                e => ConditionReducer.IsBinaryCondition(e) || ConditionReducer.IsMethodCondition(e)
            );

            var isSingle = subconditions.Count == 1;

            if (!isSingle && strict)
                throw Error.AmbiguousCondition(condition, subconditions);

            return isSingle;
        }

        private SearchFilter[] GetSearchFilter(Expression reducedCondition)
        {
            Logger.Log($"Creating filter for condition '{reducedCondition}'", Indentation.Five);

            var visitor = new ConditionVisitor(reducedCondition, strict);
            var result = visitor.GetCondition();

            if (result.Length == 0)
                Logger.Log($"Ignoring condition '{reducedCondition}' as it could not be translated to a complete {nameof(SearchFilter)}", Indentation.Six);

            if (visitor.HasIllegalServerFilter)
                HasIllegalServerFilters = visitor.HasIllegalServerFilter;

            return result;
        }

        #region IsLegalFilter

        private bool IsLegalFilter(Expression filter)
        {
            var ret = IsLegalFilterInternal(filter);

            if (!ret)
                HasIllegalServerFilters = true;

            return ret;
        }

        private bool IsLegalFilterInternal(Expression filter)
        {
            Logger.Log($"Determining whether expression '{filter}' is a legal filter", Indentation.Five);

            var properties = ExpressionSearcher.Search(filter, e => e is PropertyExpression).Cast<PropertyExpression>().ToList();

            if (!HasSinglePropertyExpression(filter, properties))
                return false;

            if (!HasExtraMemberExpressions(filter))
                return false;

            if (!HasOnlyLegalExpressionTypes(filter))
                return false;

            var property = properties.Single();
            var parents = ExpressionSearcher.GetParents(property, filter).ToList();

            if (!HasOnlyLegalCasts(property, parents))
                return false;

            if (!HasOnlyLegalParents(filter, parents))
                return false;

            Logger.Log("Filter did not match any exclusion criteria. Including filter", Indentation.Six);
            return true;
        }

        private bool HasSinglePropertyExpression(Expression filter, List<PropertyExpression> properties)
        {
            if (properties.Count != 1)
            {
                //It's a T reference without an actual property reference
                if (properties.All(p => p.Expression == null))
                    properties = new List<PropertyExpression>();

                Logger.Log($"Expression does not contain a single {nameof(PropertyExpression)} ({properties.Count} found). Ignoring filter.", Indentation.Six);

                if (strict)
                    throw Error.InvalidPropertyCount(filter, properties);
                
                return false;
            }

            return true;
        }

        private bool HasExtraMemberExpressions(Expression filter)
        {
            var members = ExpressionSearcher.Search(filter, e => e is MemberExpression, e => e is PropertyExpression).Cast<MemberExpression>().ToList();

            var illegalMembers = members.Where(m => !LegalPropertyMember(m)).ToList();

            //Catch instances where someone went (s.prop.SubProp == val)
            if (illegalMembers.Count > 0)
            {
                Logger.Log($"Expression contains {illegalMembers.Count} extraneous {nameof(MemberExpression)} expressions. Ignoring filter.", Indentation.Six);

                if (strict)
                    throw Error.InvalidMemberCount(filter, illegalMembers);
                
                return false;
            }

            return true;
        }

        private bool HasOnlyLegalExpressionTypes(Expression filter)
        {
            var types = ExpressionSearcher.GetTypes(filter);
            var unsupportedTypes = types.Where(t => t == ExpressionType.ArrayLength || t == ExpressionType.Throw || t == ExpressionType.DebugInfo).ToList();

            if (unsupportedTypes.Count > 0)
            {
                Logger.Log("Expression contains an illegal expression node(s) " + string.Join(", ", unsupportedTypes) +". Ignoring filter.", Indentation.Six);

                if (strict)
                    throw Error.UnsupportedExpressionType(unsupportedTypes, filter);

                return false;
            }

            return true;
        }

        private bool HasOnlyLegalCasts(PropertyExpression property, List<Expression> parents)
        {
            //Validate the casts being applied to a PropertyExpression. Does not consider casts that may be being
            //applied to the outer condition itself (i.e. (object)(s.Prop == val)
            var casts = parents.Where(p =>
                p.NodeType == ExpressionType.Convert || p.NodeType == ExpressionType.ConvertChecked ||
                p.NodeType == ExpressionType.TypeAs);

            var illegalCasts = casts.Where(c => IllegalCast(property, (UnaryExpression) c)).ToList();

            //If we've casted our PropertyExpression, the value we're comparing it to is not valid for
            //its true type, and therefore cannot be evaluated by PRTG
            if (illegalCasts.Count > 0)
            {
                Logger.Log($"Expression {property} contains an illegal cast(s): " + string.Join(", ", illegalCasts.Select(e => e.Type)) + ". Ignoring filter.", Indentation.Six);

                if (strict)
                    throw Error.UnsupportedCastExpression(property, illegalCasts);

                return false;
            }

            return true;
        }

        private bool HasOnlyLegalParents(Expression filter, List<Expression> parents)
        {
            var illegalParentTypes = new[]
            {
                ExpressionType.Coalesce,
                ExpressionType.Conditional,
                ExpressionType.Increment,
                ExpressionType.Index,
                ExpressionType.Decrement,
                ExpressionType.Negate,
                ExpressionType.NegateChecked,
                ExpressionType.RuntimeVariables,
                ExpressionType.OnesComplement,
                ExpressionType.TypeEqual
            };

            var illegalParents = parents.Select(p => p.NodeType).Where(p => illegalParentTypes.Any(i => i == p)).ToList();

            if (illegalParents.Count > 0)
            {
                Logger.Log("Expression contains illegal parent type(s): " + string.Join(", ", illegalParents) + ". Ignoring filter.");

                if (strict)
                    throw Error.UnsupportedParentExpression(illegalParents, filter);

                return false;
            }

            return true;
        }

        #endregion

        private bool LegalPropertyMember(MemberExpression expr)
        {
            return NullableValueMember(expr) || TimeSpanTotalSecondsMember(expr);
        }

        private bool NullableValueMember(MemberExpression expr)
        {
            if (expr.Member.Name == "Value" && Nullable.GetUnderlyingType(expr.Member.ReflectedType) != null)
                return true;

            return false;
        }

        private bool TimeSpanTotalSecondsMember(MemberExpression expr)
        {
            if (expr.Member.Name == nameof(TimeSpan.TotalSeconds) && expr.Member.ReflectedType == typeof(TimeSpan))
                return true;

            return false;
        }

        private bool IllegalCast(Expression property, UnaryExpression cast)
        {
            if (cast.Operand != property)
                return false;

            //Ignore something like (val1 == (object)val2)
            if (cast.Type == typeof(object))
            {
                Logger.Log("Ignoring cast to object", Indentation.Six);
                return false;
            }

            //Ignore something like (int1 = (int?)int2)
            if ((cast.Type.GetUnderlyingType()) == (property.Type.GetUnderlyingType()))
            {
                Logger.Log("Ignoring cast to same/nullable type", Indentation.Six);
                return false;
            }

            //Ignore something like (int1 == (double)int2)
            if (ExpressionHelpers.IsNumeric(cast.Type) && (ExpressionHelpers.IsNumeric(property.Type) || property.Type.IsEnum))
            {
                Logger.Log("Ignoring numeric/enum cast", Indentation.Six);
                return false;
            }

            //Ignore something like (bool1 == (bool)bool2)
            if (cast.Type == ExpressionHelpers.UnwrapCast(cast).Type)
            {
                Logger.Log($"Ignoring {cast.Type} cast back to original expression type", Indentation.Six);
                return false;
            }

            //Ignore something like (enum1Val == (Enum)Enum1.Val)
            if (cast.Type == typeof(Enum) && ExpressionHelpers.UnwrapCast(cast).Type == property.Type)
            {
                Logger.Log($"Ignoring '{cast.Type}' cast against enum type '{property.Type}' being used in comparison of value of enum type '{property.Type}'", Indentation.Six);
                return false;
            }

            return true;
        }

        private Property GetProperty(PropertyExpression member)
        {
            var attrib = member.PropertyInfo.GetAttribute<PropertyParameterAttribute>();

            return (Property)attrib.Property;
        }
    }
}
