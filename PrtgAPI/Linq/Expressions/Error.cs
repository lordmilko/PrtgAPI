using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq.Expressions
{
    class Error
    {
        static string localEval = $"Consider calling '{typeof(Enumerable).Name}.{nameof(Enumerable.AsEnumerable)}' first to force local evaluation.";
        private static string localSplit = $"Consider splitting your condition across multiple expressions and calling '{typeof(Enumerable).Name}.{nameof(Enumerable.AsEnumerable)}' to force local evaluation.";

        private static readonly string ambiguousPropertyExpression               = $"Cannot resolve source property of member '{{0}}', possibly due to the use of a constructor or the member not having been assigned a value. {localEval}";
        private static readonly string unsupportedLinqExpression                 = $"Method '{{0}}' is not supported. {localEval}";
        private static readonly string unconsecutiveLinqExpression               = $"Method '{{0}}' cannot be called again after being interrupted by a different LINQ expression. {localEval}";
        private static readonly string unsupportedFilterExpression_LeftRightZero = $"The expression '{{0}}' cannot be evaluated by PRTG as it did not reference any properties of the source object. {localEval}";
        private static readonly string unsupportedFilterExpression_LeftRightSame = $"The property '{{0}}' was referenced multiple times within the expression '{{1}}'. A property may only be referenced on one side of a {{2}}. {localSplit}";
        private static readonly string unsupportedFilterExpression_AndSame       = $"Cannot perform multiple AND conditions against the property '{{0}}'. An AND condition may only be performed against different properties. {localSplit}";
        private static readonly string unsupportedFilterExpression_ExclusiveOr   = $"Cannot perform logical OR against property '{{0}}': property can only be specified once in an expression. {localEval}";
        private static readonly string unsupportedFilterProperty                 = $"Property '{{0}}' cannot be evaluated server side. {localEval}";
        private static readonly string unsupportedFilterExpression_OrMultiple    = $"Cannot perform logical OR between properties '{{0}}' and '{{1}}'. An OR condition may only reference a single property. {localEval}";
        private static readonly string unsupportedFilterExpression_NoProperty    = $"The expression '{{0}}' did not contain any property references and cannot be evaluated server side. {localSplit}";
        private static readonly string unsupportedExpressionType                 = $"Expression type '{{0}}' cannot be used as part of a property expression in condition '{{1}}'. {localEval}";
        private static readonly string unsupportedCastExpression                 = $"Expression '{{0}}' of type '{{1}}' cannot be casted to type {{2}}. {localEval}";
        private static readonly string unsupportedParentExpression               = $"Expression type '{{0}}' cannot be used as the parent of a property expression in condition '{{1}}'. {localEval}";
        private static readonly string invalidCondition                          = $"Expression '{{0}}' could not be translated to a valid {nameof(SearchFilter)} as a valid {{1}} could not be found. {localEval}";
        private static readonly string invalidToStringTarget                     = $"Method '{nameof(ToString)}' cannot be used on bool, class, enum or struct type '{{0}}' in condition '{{1}}'. '{nameof(ToString)}' may only be called against simple primitive types such as numbers. {localEval}";
        private static readonly string unsupportedMethodCall                     = $"Method '{{0}}' in expression '{{1}}' cannot be evaluated server side. {localEval}";
        private static readonly string invalidPropertyCount_Zero                 = $"Condition '{{0}}' did not contain a property expression. A single source property must be referenced. {localEval}";
        private static readonly string invalidPropertyCount_Multiple             = $"Condition '{{0}}' references multiple source properties ({{1}}). Only one property may be referenced per condition. {localEval}";
        private static readonly string invalidMemberCount_Multiple               = $"Condition '{{0}}' references one or more sub-members of a property expression ({{1}}). Property sub-members cannot be evaluated when specified in a condition. {localEval}";
        private static readonly string multiPartExpression                       = $"Cannot evaluate multi-part expression '{{0}}'. {localEval}";
        private static readonly string logDuplicateRangeBound                    = $"Cannot specify multiple {{0}} DateTime bounds in a single request. One of {{1}} must be specified. {localSplit}"; 
        private static readonly string logUnsupportedFilter                      = $"Cannot filter logs by propert{{0}} {{1}}: specified filter{{2}} not supported. {localEval}";
        private static readonly string logDuplicateId                            = $"Cannot filter logs by multiple IDs in a single request. One of {{0}} must be specified. {localEval}";
        private static readonly string ambiguousCondition                        = $"Cannot evaluate expression '{{0}}' containing multiple sub-conditions ({{1}}). {localEval}";
        private static readonly string invalidEnumComparison                     = $"Cannot evaluate condition '{{0}}' between enums of type '{{1}}' and '{{2}}'. {localEval}";

        internal static Exception AmbiguousPropertyExpression(MemberExpression expression)
        {
            return new NotSupportedException(string.Format(ambiguousPropertyExpression, expression));
        }

        internal static Exception UnsupportedLinqExpression(string name)
        {
            return new NotSupportedException(string.Format(unsupportedLinqExpression, name));
        }

        internal static Exception UnconsecutiveLinqExpression(string name)
        {
            return new NotSupportedException(string.Format(unconsecutiveLinqExpression, name));
        }

        internal static Exception InvalidEnumComparison(Expression node, Expression first, Expression second)
        {
            var constant = second as ConstantExpression;

            var secondType = constant?.Value.GetType() ?? second.Type;

            return new NotSupportedException(string.Format(invalidEnumComparison, node, first.Type, secondType));
        }

        internal static Exception UnsupportedFilterExpression(Expression expr, Expression ignoredExpression, IllegalType illegalType)
        {
            var exprProperties = ExpressionSearcher.Search<PropertyExpression>(expr).Cast<PropertyExpression>().ToList();
            var ignoredExprProperties = ExpressionSearcher.Search<PropertyExpression>(ignoredExpression).Cast<PropertyExpression>().ToList();

            switch (illegalType)
            {
                //We don't reach this case because we always throw based on having multiple PropertyExpressions before
                //we can actually determine both expressions were the same
                case IllegalType.LeftRightSameProperty:
                    if (ignoredExprProperties.Count == 0)
                        return new NotSupportedException(string.Format(unsupportedFilterExpression_LeftRightZero, Clean(ignoredExpression)));

                    return new NotSupportedException(string.Format(
                        unsupportedFilterExpression_LeftRightSame,
                        ignoredExprProperties.First().PropertyInfo.Name,
                        Clean(ignoredExpression),
                        ignoredExpression.GetType()
                    ));

                case IllegalType.NotSingleProperty:
                    return new NotSupportedException(string.Format(
                        unsupportedFilterExpression_NoProperty,
                        Clean(ignoredExpression)
                    ));

                case IllegalType.AndSameProperty:
                    return new NotSupportedException(string.Format(unsupportedFilterExpression_AndSame, exprProperties.Single().PropertyInfo.Name));

                case IllegalType.OrDifferentProperty:
                    return new NotSupportedException(string.Format(
                        unsupportedFilterExpression_OrMultiple,
                        exprProperties.Single().PropertyInfo.Name,
                        ignoredExprProperties.Single().PropertyInfo.Name
                    ));

                case IllegalType.ExclusiveOr:
                    return new NotSupportedException(string.Format(
                        unsupportedFilterExpression_ExclusiveOr,
                        exprProperties.Single().PropertyInfo.Name
                    ));
            }

            throw new NotImplementedException($"Don't know how to handle illegal type '{illegalType}'.");
        }

        internal static Exception UnsupportedExpressionType(List<ExpressionType> types, Expression condition)
        {
            return new NotSupportedException(string.Format(unsupportedExpressionType, types.First(), Clean(condition)));
        }

        public static Exception UnsupportedFilterProperty(MemberExpression node)
        {
            return new NotSupportedException(string.Format(unsupportedFilterProperty, node.Member.Name));
        }

        internal static Exception UnsupportedCastExpression(PropertyExpression property, List<Expression> illegalCasts)
        {
            return new NotSupportedException(string.Format(unsupportedCastExpression, Clean(property), property.Type, string.Join(", ", $"'{illegalCasts.First().Type.Name}'")));
        }

        internal static Exception UnsupportedParentExpression(List<ExpressionType> types, Expression condition)
        {
            return new NotSupportedException(string.Format(unsupportedParentExpression, types.First(), Clean(condition)));
        }

        internal static Exception InvalidCondition(Expression condition, string reason)
        {
            return new NotSupportedException(string.Format(invalidCondition, Clean(condition), reason));
        }

        public static Exception UnsupportedToStringTarget(Expression condition, Type type)
        {
            return new NotSupportedException(string.Format(invalidToStringTarget, type.Name, Clean(condition)));
        }

        internal static Exception UnsupportedMethodCall(Expression condition, string method)
        {
            return new NotSupportedException(string.Format(unsupportedMethodCall, method, Clean(condition)));
        }

        public static Exception InvalidPropertyCount(Expression condition, List<PropertyExpression> properties)
        {
            var conditionStr = Clean(condition);

            if (properties.Count == 0)
                return new NotSupportedException(string.Format(invalidPropertyCount_Zero, conditionStr));

            return new NotSupportedException(string.Format(invalidPropertyCount_Multiple, conditionStr, string.Join(", ", Clean(properties))));
        }

        public static Exception InvalidMemberCount(Expression condition, List<MemberExpression> members)
        {
            var conditionStr = Clean(condition);

            return new NotSupportedException(string.Format(invalidMemberCount_Multiple, conditionStr, string.Join(", ", Clean(members))));
        }

        private static string Clean(Expression condition)
        {
            var binary = condition as BinaryExpression;

            string str = null;

            if (binary?.NodeType == ExpressionType.Equal)
            {
                var constant = binary.Right as ConstantExpression;

                if (constant != null && constant.Type == typeof(bool) && constant.Value.Equals(true))
                    str = ExtensionReducer.Reduce(binary.Left).ToString();
            }
            
            if (str == null)
                str = ExtensionReducer.Reduce(condition).ToString();

            if (str.StartsWith("(") && str.EndsWith(")"))
                return str.Substring(1, str.Length - 2);

            return str;
        }

        private static IEnumerable<Expression> Clean(IEnumerable<Expression> exprs)
        {
            return exprs.Select(ExtensionReducer.Reduce);
        }

        public static Exception InvalidFilterValue(Property property, FilterOperator op, string value, string validDescription)
        {
            return new NotSupportedException($"Cannot filter where property '{property}' {op.ToString().ToLower()} '{value}'. PRTG only supports filters where {validDescription}.");
        }

        public static Exception UnsupportedProperty(Property property)
        {
            return new NotSupportedException($"Cannot filter by property '{property}': filter is not supported by PRTG.");
        }

        public static Exception MultiPartExpression(BinaryExpression expression)
        {
            return new NotSupportedException(string.Format(multiPartExpression, Clean(expression)));
        }

        public static Exception LogDuplicateDateRangeStart(List<DateTime> dates)
        {
            return LogDuplicateBoundEnd("lower", dates);
        }

        public static Exception LogDuplicateDateRangeEnd(List<DateTime> dates)
        {
            return LogDuplicateBoundEnd("upper", dates);
        }

        private static Exception LogDuplicateBoundEnd(string end, List<DateTime> dates)
        {
            return new NotSupportedException(string.Format(logDuplicateRangeBound, end, dates.ToQuotedList()));
        }

        public static Exception LogUnsupportedFilter(List<SearchFilter> filters)
        {
            var plural1 = filters.Count == 1 ? "y" : "ies";
            var plural2 = filters.Count == 1 ? " is" : "s are";

            return new NotSupportedException(string.Format(logUnsupportedFilter, plural1, string.Join(", ", filters.Select(f => $"'{f.Property}'")), plural2));
        }

        public static Exception LogDuplicateId(List<object> ids)
        {
            return new NotSupportedException(string.Format(logDuplicateId, ids.ToQuotedList()));
        }

        public static Exception AmbiguousCondition(Expression condition, List<Expression> subconditions)
        {
            return new NotSupportedException(string.Format(ambiguousCondition, Clean(condition), string.Join(", ", Clean(subconditions).Select(e => $"'{Clean(e)}'"))));
        }

        public static Exception MergeMultiParameterLambda(LinqExpression node, LinqExpression innerExpr)
        {
            return new NotSupportedException($"Cannot merge method '{node.Method.Method.Name}' in expression '{ExtensionReducer.Reduce(node)}': one or more lambda expressions reference multiple lambda parameters. {localSplit}");
        }

        public static Exception UnsupportedOptionalPredicate(MethodCallExpression expr)
        {
            if (expr.Arguments.Count > 1)
                return new NotSupportedException($"Method '{expr.Method.Name}' is not supported. Consider changing your expression to '{expr.Arguments[0]}.Where({expr.Arguments[1]}).{nameof(Enumerable.AsEnumerable)}().{expr.Method.Name}()'.");

            return UnsupportedLinqExpression(expr.Method.Name);
        }

        public static Exception WeakContainsCondition(MethodCallExpression expr)
        {
            return new NotSupportedException($"Cannot call method '{expr.Method.Name}' when using strict evaluation semantics. Method is only partially equivalent to filter operator 'Contains'. {localEval}");
        }
    }
}
