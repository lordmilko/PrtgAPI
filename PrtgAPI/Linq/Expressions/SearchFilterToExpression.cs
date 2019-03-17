using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PrtgAPI.Attributes;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq.Expressions
{
    class SearchFilterToExpression
    {
        private SearchFilter[] filters;

        public static Func<T, bool> Parse<T>(params SearchFilter[] filters)
        {
            var converter = new SearchFilterToExpression(filters);

            return converter.Convert<T>();
        }

        public SearchFilterToExpression(params SearchFilter[] filters)
        {
            this.filters = filters;
        }

        public Func<T, bool> Convert<T>()
        {
            var parameter = Expression.Parameter(typeof(T), "o");

            var conditions = new List<Tuple<Property, Expression>>();

            foreach (var filter in filters)
            {
                var typeProps = ReflectionCacheManager.Get(typeof(T)).Properties;
                var property = typeProps.FirstOrDefault(
                    p => p.GetAttributes<PropertyParameterAttribute>().Any(a => a.Property.Equals(filter.Property))
                );

                if (property == null)
                {
                    continue;
                }

                var member = Expression.MakeMemberAccess(parameter, property.Property);

                if (filter.Value is IEnumerable && !(filter.Value is string))
                {
                    foreach (var value in (IEnumerable) filter.Value)
                    {
                        conditions.AddRange(GetConditions(member, filter.Property, filter.Operator, value));
                    }
                }
                else
                {
                    conditions.AddRange(GetConditions(member, filter.Property, filter.Operator, filter.Value));
                }
            }

            var body = GetLambdaBody(conditions);

            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

            var result = lambda.Compile();

            return result;
        }

        private IEnumerable<Tuple<Property, Expression>> GetConditions(
            MemberExpression member,
            Property property, FilterOperator op, object value)
        {
            Enum e = value as Enum;

            if (e != null)
            {
                var flags = e.GetUnderlyingFlags().ToList();

                if (flags.Count > 0)
                {
                    foreach (var flag in flags)
                    {
                        yield return Tuple.Create(property, GetCondition(member, property, op, flag));
                    }

                    yield break;
                }
            }

            yield return Tuple.Create(property, GetCondition(member, property, op, value));
        }

        private Expression GetLambdaBody(List<Tuple<Property, Expression>> conditions)
        {
            //If we didn't construct any Expressions
            if (conditions.Count == 0)
            {
                //If no filters were specified to begin with, permit everything
                if (filters.Length == 0)
                    return Expression.Constant(true);

                //If only invalid filters were specified, permit nothing.
                //If half valid, half invalid were specified, its a logical OR so True || False is OK
                //(plus, in that scenario this code won't even be reached)
                return Expression.Constant(false);
            }

            var grouped = conditions.GroupBy(c => c.Item1);

            Expression expr = null;

            foreach (var group in grouped)
            {
                var groupExpression = GetSingleGroupExpression(group);

                if (expr == null)
                    expr = groupExpression;
                else
                    expr = Expression.AndAlso(expr, groupExpression);
            }

            return expr;
        }

        private Expression GetSingleGroupExpression(IGrouping<Property, Tuple<Property, Expression>> group)
        {
            Expression expr = null;

            foreach (var filter in group)
            {
                if (expr == null)
                    expr = filter.Item2;
                else
                    expr = Expression.OrElse(expr, filter.Item2);
            }

            return expr;
        }

        private Expression GetCondition(MemberExpression member, Property property, FilterOperator op, object value)
        {
            switch (op)
            {
                case FilterOperator.Equals:
                    return Equality(ExpressionType.Equal, member, property, value);
                case FilterOperator.NotEquals:
                    return Equality(ExpressionType.NotEqual, member, property, value);
                case FilterOperator.GreaterThan:
                    return GreaterOrLessThan(ExpressionType.GreaterThan, member, property, value);
                case FilterOperator.LessThan:
                    return GreaterOrLessThan(ExpressionType.LessThan, member, property, value);
                case FilterOperator.Contains:
                    if (member.Type.IsArray)
                        return ArrayContains(member, property, value);

                    return Contains(member, property, value);
                default:
                    throw new NotImplementedException($"Don't know how to handle filter operator '{op}'.");
            }
        }

        #region Equality

        private Expression Equality(ExpressionType nodeType, MemberExpression member, Property property, object value)
        {
            if (member.Type.IsArray)
            {
                //PRTG ignores "NotEquals" against array types
                if (nodeType == ExpressionType.NotEqual)
                    return Expression.Constant(false);

                return ArrayContains(member, property, value);
            }
                

            var underlying = member.Type.GetUnderlyingType();

            if (underlying == value.GetType() && member.Type != typeof(string))
                return Expression.MakeBinary(nodeType, member, Constant(value, member.Type));

            if (IsType(underlying, typeof(string), typeof(bool), typeof(TimeSpan), typeof(DateTime)) || typeof(IStringEnum).IsAssignableFrom(member.Type) || underlying.IsEnum)
                return GetDefaultEquality(nodeType, member, property, value);

            Expression expr;

            if (IsIntegerEqual(nodeType, member, property, value, out expr))
                return expr;

            if (IsDoubleEqual(nodeType, member, property, value, out expr))
                return expr;

            throw new NotImplementedException($"Either don't know how to handle member of type {member.Type} or object of type {value.GetType()}.");
        }

        private bool IsType(Type type, params Type[] types)
        {
            if (types.Any(t => t == type))
                return true;

            return false;
        }

        private Expression GetDefaultEquality(ExpressionType nodeType, MemberExpression member, Property property, object value)
        {
            var op = nodeType == ExpressionType.Equal ? FilterOperator.Equals : FilterOperator.NotEquals;

            var serializedMember = SerializeMember(member, property, op);
            var serializedValue = SerializeValue(value, property, op);

            return IsEqualString(nodeType, serializedMember, serializedValue);
        }

        private bool IsIntegerEqual(ExpressionType nodeType, MemberExpression member, Property property, object value, out Expression result)
        {
            if (member.Type == typeof(int) || member.Type == typeof(int?))
            {
                int integer;

                if (int.TryParse(value.ToString(), out integer))
                    result = Expression.MakeBinary(nodeType, member, Constant(integer, member.Type));
                else
                    result = GetDefaultEquality(nodeType, member, property, value);

                return true;
            }

            result = null;
            return false;
        }

        private bool IsDoubleEqual(ExpressionType nodeType, MemberExpression member, Property property, object value, out Expression result)
        {
            if (member.Type == typeof(double) || member.Type == typeof(double?))
            {
                double @double;

                if (double.TryParse(value.ToString(), out @double))
                    result = Expression.MakeBinary(nodeType, member, Constant(@double, member.Type));
                else
                    result = GetDefaultEquality(nodeType, member, property, value);

                return true;
            }

            result = null;
            return false;
        }

        #endregion
        #region GreaterThan / LessThan

        private Expression GreaterOrLessThan(ExpressionType nodeType, MemberExpression member, Property property, object value)
        {
            var method = GetType().GetMethod(nameof(GreaterThanOrFalse), BindingFlags.NonPublic | BindingFlags.Static);

            var memberExpr = CastValueType(member);
            var valueExpr = CastValueType(Expression.Constant(value));

            var call = Expression.Call(
                method,
                Expression.Constant(nodeType),
                memberExpr,
                Expression.Constant(property),
                valueExpr
            );

            var ifNotNull = ExpressionHelpers.BoolIfNotNull(member, call);

            return ifNotNull;
        }

        private static bool GreaterThanOrFalse(ExpressionType nodeType, object member, Property property, object value)
        {
            var serializedMember = SearchFilter.GetValue(property, FilterOperator.GreaterThan, member);
            var serializedValue = SearchFilter.GetValue(property, FilterOperator.GreaterThan, value);

            double serializedMemberVal;
            double serializedValueVal;

            if (double.TryParse(serializedMember, out serializedMemberVal))
            {
                if (double.TryParse(serializedValue, out serializedValueVal))
                {
                    if (nodeType == ExpressionType.GreaterThan)
                        return serializedMemberVal > serializedValueVal;
                    else
                        return serializedMemberVal < serializedValueVal;
                }
            }

            return false;
        }

        #endregion
        #region Contains

        private Expression Contains(MemberExpression member, Property property, object value)
        {
            var serializedMember = SerializeMember(member, property, FilterOperator.Contains);
            var serializedValue = SerializeValue(value, property, FilterOperator.Contains);

            var containsMethod = GetType().GetMethod(nameof(CaseInsensitiveContains), BindingFlags.NonPublic | BindingFlags.Static);

            var call = Expression.Call(containsMethod, serializedMember, serializedValue);

            var conditionalCall = ExpressionHelpers.BoolIfNotNull(serializedMember, call);

            return conditionalCall;
        }

        private static bool CaseInsensitiveContains(string source, string subString)
        {
            return source.IndexOf(subString, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private Expression ArrayContains(MemberExpression member, Property property, object value)
        {
            if (member.Type.GetElementType() == typeof(string))
            {
                var method = GetType().GetMethod(nameof(ArrayContainsInternal), BindingFlags.NonPublic | BindingFlags.Static);

                var serializedValue = SerializeValue(value, property, FilterOperator.Contains);

                var call = Expression.Call(method, member, serializedValue);

                return ExpressionHelpers.BoolIfNotNull(member, call);
            }

            throw new NotImplementedException($"Don't know how to handle array of type '{member.Type.GetElementType().Name}'.");
        }

        private static bool ArrayContainsInternal(string[] arr, string val)
        {
            return arr.Any(a => CaseInsensitiveContains(a, val));
        }

        #endregion

        private Expression IsEqualString(ExpressionType nodeType, Expression first, Expression second)
        {
            var method = typeof(string).GetMethod(nameof(string.Equals), new[] {typeof(string), typeof(StringComparison)});

            var callEquals = Expression.Call(first, method, second, Expression.Constant(StringComparison.OrdinalIgnoreCase));

            Expression equal = ExpressionHelpers.BoolIfNotNull(first, callEquals);

            if (nodeType == ExpressionType.NotEqual)
                equal = Expression.Equal(equal, Expression.Constant(false));

            return equal;
        }

        /// <summary>
        /// Creates a <see cref="ConstantExpression"/> from a <paramref name="value"/>, wrapping it in a
        /// cast to a <see cref="Nullable"/> type (resulting in a <see cref="UnaryExpression"/>) based on the type of the <paramref name="memberType"/>.
        /// </summary>
        /// <param name="value">The value to wrap in an expression.</param>
        /// <param name="memberType">The member the value should be compared against.</param>
        /// <returns>A <see cref="ConstantExpression"/> if the member type is not nullable; otherwise a <see cref="UnaryExpression"/>.</returns>
        private Expression Constant(object value, Type memberType)
        {
            var constant = Expression.Constant(value);

            if (Nullable.GetUnderlyingType(memberType) != null)
            {
                return Expression.Convert(constant, memberType);
            }

            return constant;
        }

        private Expression SerializeMember(MemberExpression member, Property property, FilterOperator op)
        {
            var getValue = typeof(SearchFilter).GetMethod("GetValue", BindingFlags.NonPublic | BindingFlags.Static);

            Expression call = Expression.Call(
                getValue,
                Expression.Constant(property, typeof(Property?)), //Property
                Expression.Constant(op),
                CastValueType(member),                        //value
                Expression.Constant(FilterMode.Illegal)       //FilterMode
            );

            return ExpressionHelpers.NullConditional(member, call);
        }

        private Expression CastValueType(Expression expr)
        {
            if (expr.Type.IsValueType)
                return Expression.Convert(expr, typeof(object));

            return expr;
        }

        private Expression SerializeValue(object value, Property property, FilterOperator op)
        {
            var serialized = SearchFilter.GetValue(property, op, value);

            return Expression.Constant(serialized);
        }
    }
}
