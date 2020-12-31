using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    class ConditionReducer : PrtgExpressionVisitor
    {
        private bool strict;

        public static Expression Reduce(Expression condition, bool strict)
        {
            Logger.Log($"Reducing condition {condition}", Indentation.Five);

            var reducer = new ConditionReducer(strict);

            var newNode = MethodReplacer.Replace(condition);

            var reduced = reducer.Visit(newNode);

            if (condition == reduced)
                Logger.Log($"Expression '{condition}' could not be reduced any further", Indentation.Six);

            return reduced;
        }

        public ConditionReducer(bool strict)
        {
            this.strict = strict;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            node = ReduceDoubleFalse(node);

            var left = Visit(node.Left);
            var right = Visit(node.Right);
            var op = node.NodeType;

            //Update the node based on the result of visiting our children
            if (left != node.Left || right != node.Right)
                node = Expression.MakeBinary(op, left, right);

            var newNode = CleanCast(node);

            Expression outExpr;

            //Given an expression like ((cond == val) == true), reduce the expression to (cond == val)
            if (ReduceTrue(newNode, out outExpr))
                newNode = outExpr;

            //Given an expression like ((cond == val) != false),
            //reduce the expression to (cond == val)
            if (ReduceNotFalse(newNode, out outExpr))
                newNode = outExpr;

            //Given an expression like (cond == val) == false, flip the expression to (cond != val)
            if (FlipFalse(newNode, out outExpr))
                newNode = outExpr;

            //Given an expression like (Enum1.Val == (Enum)Enum2.Val), reduce it to True
            if (DifferentEnumTypes(newNode, out outExpr))
                newNode = outExpr;

            //If one half of our condition cannot be evaluated server side, reduce it to
            //True
            if (HalfEvaluable(newNode, out outExpr))
                newNode = outExpr;

            //If we have something like true != false, evaluate it and replace
            //it with the resulting value
            if (BoolAgainstBool(newNode, out outExpr))
                newNode = outExpr;

            return newNode;
        }

        private bool DifferentEnumTypes(Expression node, out Expression outExpr)
        {
            var binary = node as BinaryExpression;

            if (binary != null)
            {
                if (DifferentEnumTypesInternal(node, binary.Left, binary.Right, out outExpr))
                    return true;

                if (DifferentEnumTypesInternal(node, binary.Right, binary.Left, out outExpr))
                    return true;
            }

            outExpr = null;
            return false;
        }

        private bool DifferentEnumTypesInternal(Expression node, Expression first, Expression second, out Expression outExpr)
        {
            var firstUnwrapped = SubstituteExpression.Strip(ExpressionHelpers.UnwrapCast(first));
            var secondUnwrapped = SubstituteExpression.Strip(ExpressionHelpers.UnwrapCast(second));

            if (IsEnum(firstUnwrapped) && IsEnum(secondUnwrapped))
            {
                var property = ExpressionSearcher.Search(firstUnwrapped, p => p is PropertyExpression).FirstOrDefault();

                if (property != null)
                {
                    var comparison = ExpressionSearcher.Search(secondUnwrapped, e => IsEnum(e) && e.NodeType != ExpressionType.Convert && e.NodeType != ExpressionType.ConvertChecked);

                    var firstComparison = comparison.FirstOrDefault();

                    if (firstComparison != null)
                    {
                        if (firstComparison.Type.IsEnum && firstComparison.Type != property.Type)
                        {
                            if (strict)
                                throw Error.InvalidEnumComparison(node, firstUnwrapped, secondUnwrapped);

                            Logger.Log($"Condition {node} compares enums of different types. Reducing to 'True'", Indentation.Six);

                            outExpr = Expression.Constant(true);
                            return true;
                        }
                        else
                        {
                            var constant = firstComparison as ConstantExpression;

                            if (constant != null)
                            {
                                if (property.Type != constant.Value.GetType())
                                {
                                    Logger.Log($"Condition {node} compares enums of different types. Reducing to 'True'", Indentation.Six);

                                    if (strict)
                                        throw Error.InvalidEnumComparison(node, firstUnwrapped, secondUnwrapped);

                                    outExpr = Expression.Constant(true);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            outExpr = null;
            return false;
        }

        private bool IsEnum(Expression expr)
        {
            if (expr.Type.IsEnum || expr.Type == typeof(Enum))
                return true;

            var constant = expr as ConstantExpression;

            if (constant?.Value != null)
            {
                if (constant.Value.GetType().IsEnum || constant.Value.GetType() == typeof(Enum))
                    return true;
            }

            return false;
        }

        public BinaryExpression ReduceDoubleFalse(BinaryExpression expr)
        {
            var left = SubstituteExpression.Strip(expr.Left);
            var right = SubstituteExpression.Strip(expr.Right);

            if (IsFalse(left, expr.NodeType) && right is BinaryExpression)
                return ReduceDoubleFalseInternal(expr, (BinaryExpression) right);

            if (IsFalse(right, expr.NodeType) && left is BinaryExpression)
                return ReduceDoubleFalseInternal(expr, (BinaryExpression) left);

            return expr;
        }

        private BinaryExpression ReduceDoubleFalseInternal(BinaryExpression original, BinaryExpression binaryInternal)
        {
            var innerLeft = SubstituteExpression.Strip(binaryInternal.Left);
            var innerRight = SubstituteExpression.Strip(binaryInternal.Right);

            if (IsFalse(innerLeft, binaryInternal.NodeType))
                return ReduceDoubleFalseInternalInternal(innerRight);

            if (IsFalse(innerRight, binaryInternal.NodeType))
                return ReduceDoubleFalseInternalInternal(innerLeft);

            return original;
        }

        private BinaryExpression ReduceDoubleFalseInternalInternal(Expression innerInnerExpr)
        {
            if (innerInnerExpr is BinaryExpression)
                return ReduceDoubleFalse((BinaryExpression)innerInnerExpr);

            return ReduceDoubleFalse(Expression.MakeBinary(ExpressionType.Equal, innerInnerExpr, Expression.Constant(true)));
        }

        private Expression CleanCast(Expression node)
        {
            var binary = node as BinaryExpression;

            if (binary != null)
            {
                BinaryExpression cleaned;

                if (CleanCastInternal(binary.Left, binary.Right, node.NodeType, out cleaned))
                    return cleaned;

                if (CleanCastInternal(binary.Right, binary.Left, node.NodeType, out cleaned))
                    return cleaned;
            }

            return node;
        }

        private bool CleanCastInternal(Expression first, Expression second, ExpressionType nodeType, out BinaryExpression cleaned)
        {
            if (first.NodeType == ExpressionType.Convert || first.NodeType == ExpressionType.ConvertChecked)
            {
                var unwrappedFirst = ExpressionHelpers.UnwrapCast(first);

                if (EqualCastType(unwrappedFirst, second))
                {
                    cleaned = Expression.MakeBinary(nodeType, unwrappedFirst, second);
                    return true;
                }

                //Normally second will have already been pre-evaluated, however if we had first.Equals(second),
                //we're now operating on ((object)first) == ((object)second), in which case we need to unwrap.
                var unwrappedSecond = ExpressionHelpers.UnwrapCast(second);

                if (EqualCastType(unwrappedFirst, unwrappedSecond))
                {
                    cleaned = Expression.MakeBinary(nodeType, unwrappedFirst, unwrappedSecond);
                    return true;
                }
            }

            cleaned = null;
            return false;
        }

        private bool EqualCastType(Expression first, Expression second)
        {
            if (first.Type == second.Type)
                return true;

            return false;
        }

        private bool FlipFalse(Expression node, out Expression flipped)
        {
            var binary = node as BinaryExpression;

            if (binary != null)
            {
                if (Flip(IsFalse, binary, binary.Left, binary.Right, out flipped))
                    return true;

                if (Flip(IsFalse, binary, binary.Right, binary.Left, out flipped))
                    return true;
            }

            flipped = null;
            return false;
        }

        private bool ReduceNotFalse(Expression node, out Expression reduced)
        {
            if (node.NodeType == ExpressionType.NotEqual)
            {
                var binary = (BinaryExpression) node;

                Func<Expression, bool> isTrue = e => IsFalse(e, ExpressionType.Equal);

                if (ReduceIsTrueInternal(binary.Left, binary.Right, isTrue, out reduced))
                {
                    Logger.Log($"Reduced '{node}' to '{reduced}'", Indentation.Six);
                    return true;
                }

                if (ReduceIsTrueInternal(binary.Right, binary.Left, isTrue, out reduced))
                {
                    Logger.Log($"Reduced '{node}' to '{reduced}'", Indentation.Six);
                    return true;
                }
            }

            reduced = null;
            return false;
        }

        private bool ReduceIsTrueInternal(Expression primary, Expression secondary, Func<Expression, bool> isTrue, out Expression reduced)
        {
            if (isTrue(primary) && IsLegalCondition(secondary))
            {
                reduced = secondary;
                return true;
            }

            reduced = null;
            return false;
        }

        private bool ReduceTrue(Expression node, out Expression reduced)
        {
            if (node.NodeType == ExpressionType.Equal)
            {
                var binary = (BinaryExpression) node;

                Func<Expression, bool> isTrue = e => IsTrue(e, ExpressionType.Equal);

                if (ReduceIsTrueInternal(binary.Left, binary.Right, isTrue, out reduced))
                {
                    Logger.Log($"Reduced '{node}' to '{reduced}'", Indentation.Six);
                    return true;
                }

                if (ReduceIsTrueInternal(binary.Right, binary.Left, isTrue, out reduced))
                {
                    Logger.Log($"Reduced '{node}' to '{reduced}'", Indentation.Six);
                    return true;
                }
            }

            reduced = null;
            return false;
        }

        private bool HalfEvaluable(Expression node, out Expression reduced)
        {
            var binary = node as BinaryExpression;

            if (binary != null)
            {
                //We can't be FULLY illegal; otherwise, we don't have a PropertyExpression and we would have been
                //pre-evaluated
                if (OnlyOneHalfLegal(binary.Left, binary.Right) || OnlyOneHalfLegal(binary.Right, binary.Left))
                {
                    Logger.Log($"Unable to further reduce multi-part expression '{node}'. Reducing to 'True'", Indentation.Six);

                    if (strict)
                        throw Error.MultiPartExpression(binary);

                    //Only one side was a binary expression. We can't evaluate conditions like cond == bool server side
                    reduced = Expression.Constant(true);
                    return true;
                }
            }

            reduced = null;
            return false;
        }

        private bool BoolAgainstBool(Expression node, out Expression reduced)
        {
            var binary = node as BinaryExpression;

            if (binary != null)
            {
                if (ExpressionHelpers.IsBool(binary.Left) && ExpressionHelpers.IsBool(binary.Right))
                {
                    //We have some kind of boolean expression like true != false.
                    //Evaluate it and replace it with its resulting value

                    var l = SubstituteExpression.Strip(binary.Left);
                    var r = SubstituteExpression.Strip(binary.Right);
                    var b = Expression.MakeBinary(node.NodeType, l, r);

                    var lambda = Expression.Lambda(b);
                    var result = lambda.Compile().DynamicInvoke();

                    reduced = Expression.Constant(result);
                    return true;
                }
            }

            reduced = null;
            return false;
        }

        private bool Flip(Func<Expression, ExpressionType, bool> checkBool, BinaryExpression parent, Expression primary, Expression secondary,
            out Expression flipped)
        {
            if (checkBool(primary, parent.NodeType) && IsInvertableCondition(secondary))
            {
                var currentType = GetTypeToFlip(secondary);

                var newOp = FlipOp(currentType);

                if (newOp != null)
                {
                    flipped = FlipExpr(secondary, primary, newOp.Value);
                    return true;
                }
            }

            flipped = null;
            return false;
        }

        private ExpressionType GetTypeToFlip(Expression side)
        {
            //If you do (A == B).Equals(false), the inner operator will be inverted to NotEquals (A != B).
            //But if you do !((A == B).Equals(true)), the condition will be inverted to Equals (A == B - WRONG),
            //which is not what we want. Therefore, always pretend we're == false
            if (side is MethodCallExpression)
                return ExpressionType.Equal;

            if (side is BinaryExpression)
                return side.NodeType;

            return side.NodeType;
        }

        private bool OnlyOneHalfLegal(Expression legal, Expression notLegal)
        {
            return IsLegalCondition(legal) && !IsLegalCondition(notLegal);
        }

        private bool IsInvertableCondition(Expression node)
        {
            if (node is BinaryExpression)
                return true;

            var method = node as MethodCallExpression;

            if (method != null)
            {
                if (method.Method.Name == nameof(object.Equals) && method.Arguments.Count == 1)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Flip an expression like ((A == B) == false) to (A != B)
        /// </summary>
        /// <param name="expr">The condition of the expression whose <see cref="ExpressionType"/> should be flipped.</param>
        /// <param name="otherExpr">The part of the expression that is is causing the flippage.</param>
        /// <param name="type">The new type to use for the expression.</param>
        /// <returns>The simplified <see cref="BinaryExpression"/>.</returns>
        [ExcludeFromCodeCoverage]
        private BinaryExpression FlipExpr(Expression expr, Expression otherExpr, ExpressionType type)
        {
            var binary = expr as BinaryExpression;

            if (binary != null)
            {
                //Flip an expression like ((A == B) == false)) to (A != B)
                return Expression.MakeBinary(type, binary.Left, binary.Right);
            }

            //It shouldn't be possible to have method Equals as this will have been replaced by the method reducer
            var method = expr as MethodCallExpression;

            if (method != null)
            {
                if (method.Method.Name == nameof(object.Equals) && method.Arguments.Count == 1)
                {
                    //Flip an expression like ((A == B).Equals(false)) to (A != B)
                    return Expression.MakeBinary(type, method.Object, method.Arguments[0]);
                }
            }

            var constant = expr as ConstantExpression;

            //It shouldn't be possible to have a constant as these will have already been pre-evaluated
            //or truncated
            if (constant != null)
            {
                if (ExpressionHelpers.IsBool(expr))
                {
                    var newValue = !(bool) constant.Value;

                    return Expression.MakeBinary(type, otherExpr, Expression.Constant(newValue));
                }
            }

            throw new NotImplementedException($"Don't know how to flip expr '{expr}' with otherExpr '{otherExpr}' with expression tyoe '{type}'.");
        }

        private bool IsLegalCondition(Expression node)
        {
            //We DON'T use IsBinaryCondition here, since we want to know whether we might have something illegal
            //like ((s.Id + 1) == 3)
            return ExpressionSearcher.Search(node, e => (e is BinaryExpression & e.NodeType != ExpressionType.ArrayIndex) || IsMethodCondition(e)).Count > 0;
        }

        internal static bool IsBinaryCondition(Expression expr)
        {
            if (expr == null)
                return false;

            switch (expr.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsMethodCondition(Expression expr)
        {
            var method = expr as MethodCallExpression;

            if (method != null)
            {
                switch (method.Method.Name)
                {
                    case nameof(ToString):
                        return false;

                    default:
                        return true;
                }
            }

            return false;
        }

        internal static bool IsFalse(Expression node, ExpressionType type)
        {
            var constant = node as ConstantExpression;

            if (constant?.Value is bool)
            {
                var val = (bool) constant.Value;

                if (val == false && type == ExpressionType.Equal)
                    return true;

                if (val == true && type == ExpressionType.NotEqual)
                    return true;
            }
            else
            {
                if (type == ExpressionType.Convert || type == ExpressionType.ConvertChecked)
                {
                    return IsFalse(ExpressionHelpers.UnwrapCast(node), type);
                }
            }

            return false;
        }

        private bool IsTrue(Expression node, ExpressionType type)
        {
            var constant = node as ConstantExpression;

            if (constant?.Value is bool)
            {
                var val = (bool)constant.Value;

                if (val == true && type == ExpressionType.Equal)
                    return true;

                if (val == false && type == ExpressionType.NotEqual)
                    return true;
            }

            return false;
        }

        private ExpressionType? FlipOp(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:
                    return ExpressionType.NotEqual;
                case ExpressionType.NotEqual:
                    return ExpressionType.Equal;
                case ExpressionType.LessThan:
                    return ExpressionType.GreaterThanOrEqual;
                case ExpressionType.LessThanOrEqual:
                    return ExpressionType.GreaterThan;
                case ExpressionType.GreaterThan:
                    return ExpressionType.LessThanOrEqual;
                case ExpressionType.GreaterThanOrEqual:
                    return ExpressionType.LessThan;
                default:
                    return null;
            }
        }
    }
}
