using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Linq.Expressions.Visitors;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq.Expressions
{
    enum IllegalType
    {
        LeftRightSameProperty,
        NotSingleProperty,
        AndSameProperty,
        OrDifferentProperty,
        ExclusiveOr
    }

    /// <summary>
    /// Reduces <see cref="ExpressionType.AndAlso"/> and <see cref="ExpressionType.OrElse"/> <see cref="BinaryExpression"/> objects
    /// to a form that is considered valid by PRTG (A AND B), (A1 OR A2)
    /// </summary>
    class IllegalBinaryReducer
    {
        internal List<Expression> illegalExpressionsForSplitRequest { get; } = new List<Expression>();
        internal List<Expression> illegalIgnoredExpressions { get; } = new List<Expression>();

        public bool IsIllegal { get; private set; }

        private bool strict;

        public IllegalBinaryReducer(bool strict)
        {
            this.strict = strict;
        }

        public Expression Analyze(BinaryExpression expression)
        {
            if (IsAndOrOr(expression))
            {
                Logger.Log($"Attempting to reduce expression '{expression}'", Indentation.Five);

                var simplest = GetSimplest(expression);

                if (simplest != expression)
                    Logger.Log($"Expression '{expression}' was reduced to '{simplest}'", Indentation.Six);
                else
                    Logger.Log($"Expression '{expression}' could not be reduced any further", Indentation.Six);

                return simplest;
            }
            else
                Logger.Log($"Expression '{expression}' is not an AND/OR. Cannot be reduced further", Indentation.Five);

            return expression;
        }

        private Expression GetSimplest(BinaryExpression expr)
        {
            //If we are (A && B) or (A || B) and cannot be simpler
            //Analyze our children and remove any illegal expressions
            if (!IsAndOrOr(expr.Left) && !IsAndOrOr(expr.Right))
                return RemoveIllegal(expr);

            //We are (A xx (B xx C) or ((A xx B) xx C)
            //Recurse

            var left = TryRecurseSimplest(expr.Left);
            var right = TryRecurseSimplest(expr.Right);

            if (left != expr.Left || right != expr.Right)
                expr = Expression.MakeBinary(expr.NodeType, left, right);

            //Now finally, we must validate both sides of the expression. In order to do this, we recursively analyze the left
            //and right of each node. If an expression occurs on both the left and right and there is an illegal operator between them,
            //that's illegal.
            //For example, we will identify that (A1 && B1) || A2 is illegal because there is an A on each side separated by an OR
            var groupExpr = CompareGroups(expr);

            return groupExpr;
        }

        private Expression TryRecurseSimplest(Expression expr)
        {
            var binary = SubstituteExpression.Strip(expr) as BinaryExpression;

            if (binary != null)
            {
                var reduced = Analyze(binary);

                return reduced;
            }

            return expr;
        }

        private Expression CompareGroups(BinaryExpression expr)
        {
            var result = CompareGroupsInternal(expr);

            return result.Item1;
        }

        private Tuple<Expression, List<PropertyExpression>> CompareGroupsInternal(BinaryExpression expr)
        {
            var left = CompareSide(expr.Left);
            var right = CompareSide(expr.Right);

            if (IllegalCondition(left.Item2, right.Item2, expr.NodeType))
            {
                if (expr.NodeType == ExpressionType.OrElse)
                    illegalExpressionsForSplitRequest.Add((BinaryExpression)right.Item1);

                return Tuple.Create(left.Item1, left.Item2);
            }
            else
            {
                left.Item2.AddRange(right.Item2);
                return Tuple.Create((Expression) expr, left.Item2);
            }
        }

        private bool IllegalCondition(List<PropertyExpression> leftProperties, List<PropertyExpression> rightProperties, ExpressionType nodeType)
        {
            if (nodeType == ExpressionType.AndAlso)
            {
                foreach (var member in leftProperties)
                {
                    //Cannot do A1 && A2
                    var andProp = rightProperties.FirstOrDefault(p => p.PropertyInfo.Name == member.PropertyInfo.Name && !p.Keep);

                    if (andProp != null)
                    {
                        if (strict)
                            throw Error.UnsupportedFilterExpression(andProp, null, IllegalType.AndSameProperty);

                        return true;
                    }
                }
            }
            else if (nodeType == ExpressionType.OrElse)
            {
                foreach (var member in leftProperties)
                {                    
                    //Cannot do A1 || B1
                    var orProp = rightProperties.FirstOrDefault(p => p.PropertyInfo.Name != member.PropertyInfo.Name);

                    if (orProp != null)
                    {
                        if (strict)
                            throw Error.UnsupportedFilterExpression(member, orProp, IllegalType.OrDifferentProperty);

                        return true;
                    }

                    //Cannot do A1 || A2 if "A" can only be specified once in the expression. e.g. logs do not allow OR on DateTime ranges
                    if (rightProperties.Any(p => p.PropertyInfo.Name == member.PropertyInfo.Name && p.ExclusiveOr))
                    {
                        if (strict)
                            throw Error.UnsupportedFilterExpression(member, null, IllegalType.ExclusiveOr);

                        return true;
                    }
                }
            }

            return false;
        }

        private Tuple<Expression, List<PropertyExpression>> CompareSide(Expression expression)
        {
            var members = new List<PropertyExpression>();

            if (expression is BinaryExpression) //We're an A xx B
            {
                var result = CompareGroupsInternal((BinaryExpression) expression);
                members.AddRange(result.Item2);
            }
            else //We're a Prop == value
                members.AddRange(GetProperties(expression));

            return Tuple.Create(expression, members);
        }

        private Expression RemoveIllegal(BinaryExpression expr)
        {
            //Analayze an (A && B) or (A || B), with the implication being A and B are most likely independent
            //BinaryExpressions
            var left = expr.Left;
            var right = expr.Right;

            PropertyExpression leftProperty = GetMemberAccess(left);
            PropertyExpression rightProperty = GetMemberAccess(right);

            //Validate whether any expressions did something illegal like Prop1 == Prop2
            if (leftProperty == null && rightProperty == null)
            {
                Logger.Log("Cannot compare between two properties in a single expression. Eliminating expression", Indentation.Six);
                return ReturnIllegal(null, expr, IllegalType.LeftRightSameProperty);
            }

            if (leftProperty == null)
            {
                Logger.Log($"Reducing expression '{expr}' to '{right}' as '{left}' did not contain a property expression", Indentation.Six);
                return ReturnIllegal(right, left, IllegalType.NotSingleProperty);
            }

            if (rightProperty == null)
            {
                Logger.Log($"Reducing expression '{expr}' to '{left}' as '{right}' did not contain a property expression", Indentation.Six);
                return ReturnIllegal(left, right, IllegalType.NotSingleProperty);
            }

            if (expr.NodeType == ExpressionType.AndAlso)
            {
                //Cannot AND between the same property. Request a single property and sort the expression out client side
                if (leftProperty.PropertyInfo.Name == rightProperty.PropertyInfo.Name && leftProperty.Keep == false)
                {
                    Logger.Log($"Cannot AND between property {leftProperty.PropertyInfo.Name}. Reducing to '{left}'", Indentation.Six);
                    return ReturnIllegal(left, right, IllegalType.AndSameProperty);
                }

                //Valid expression
                return expr;
            }

            if (expr.NodeType == ExpressionType.OrElse)
            {
                //Cannot OR between different properties. Request a single property and generate an additional permutation
                //for the other condition
                if (leftProperty.PropertyInfo.Name != rightProperty.PropertyInfo.Name)
                {
                    Logger.Log($"Cannot OR between different properties '{leftProperty.PropertyInfo.Name}' and '{rightProperty.PropertyInfo.Name}'. Adding '{right}' to secondary request and reducing to '{left}'", Indentation.Six);
                    illegalExpressionsForSplitRequest.Add(right);
                    return ReturnIllegal(left, right, IllegalType.OrDifferentProperty);
                }

                //Valid expression
                return expr;
            }

            return expr;
        }

        private Expression ReturnIllegal(Expression expr, Expression ignoredExpression, IllegalType illegalType)
        {
            IsIllegal = true;

            if (strict)
                throw Error.UnsupportedFilterExpression(expr, ignoredExpression, illegalType);

            //if expr is null, the WHOLE THING was illegal (e.g. prop1 == prop1), so just forget it
            if (expr != null && ignoredExpression != null && illegalType != IllegalType.OrDifferentProperty)
                illegalIgnoredExpressions.Add(ignoredExpression);

            //If expr is null, that indicates we don't have a PropertyExpression (because we've got an AndAlso BinaryExpression
            //inside a Select(). In which case, just return the original ignoredExpression
            return expr ?? ignoredExpression;
        }

        private PropertyExpression GetMemberAccess(Expression expr)
        {
            var properties = GetProperties(expr);

            //The expression is a valid simple expression in the form Prop == value
            if (properties.Count == 1)
                return properties.Single();

            if (strict && properties.Count > 1)
                throw Error.InvalidPropertyCount(expr, properties);

            //The expression either refers to multiple properties or no properties at all;
            //evaluate client side. If we're strict, we'll throw in our caller.
            return null;
        }

        private bool IsAndOrOr(Expression node)
        {
            return ExpressionSearcher.Search(node, e => e.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse).Any();
        }

        private List<PropertyExpression> GetProperties(Expression expression)
        {
            var results = ExpressionSearcher.Search<PropertyExpression>(expression);

            return results.Cast<PropertyExpression>().ToList();
        }
    }
}