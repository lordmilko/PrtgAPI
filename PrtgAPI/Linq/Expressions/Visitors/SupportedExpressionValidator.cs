using System;
using System.Linq.Expressions;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Validates whether an <see cref="Expression"/> contains any child nodes that cannot be properly parsed.<para/>
    /// Generally speaking, any expression considered illegal by the compiler is blacklisted here (just in case
    /// the expression was constructed manually)
    /// </summary>
    class SupportedExpressionValidator : ExpressionVisitor
    {
        public static Expression Validate(Expression expr)
        {
            Logger.Log($"Identifying illegal expressions for tree '{expr}'", Indentation.One);

            var validate = new SupportedExpressionValidator();

            var result = validate.Visit(expr);

            Logger.Log("No illegal expressions found", Indentation.Two);

            return result;
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return null;

            switch (node.NodeType)
            {
                #region Binary

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return base.Visit(node);

                case ExpressionType.Assign:
                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                    throw Unsupported(node);

                #endregion
                #region Unary

                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Decrement:
                case ExpressionType.Increment:
                case ExpressionType.IsTrue:
                case ExpressionType.IsFalse:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.OnesComplement:
                case ExpressionType.Quote:
                case ExpressionType.Throw:
                case ExpressionType.TypeAs:
                case ExpressionType.UnaryPlus:
                case ExpressionType.Unbox:
                    return base.Visit(node);

                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.PostDecrementAssign:
                    throw Unsupported(node);

                #endregion
                #region Miscellaneous

                case ExpressionType.Call:           //MethodCallExpression
                case ExpressionType.Conditional:
                case ExpressionType.Constant:       //ConstantExpression
                case ExpressionType.DebugInfo:
                case ExpressionType.Default:        //DefaultExpression
                case ExpressionType.Extension:
                case ExpressionType.Index:
                case ExpressionType.Invoke:
                case ExpressionType.Lambda:         //LambdaExpression
                case ExpressionType.ListInit:
                case ExpressionType.MemberAccess:   //MemberExpression
                case ExpressionType.MemberInit:     //MemberInitExpression
                case ExpressionType.New:            //NewExpression
                case ExpressionType.NewArrayInit:   //NewArrayExpression
                case ExpressionType.NewArrayBounds: //NewArrayExpression
                case ExpressionType.Parameter:      //ParameterExpression
                case ExpressionType.RuntimeVariables:
                case ExpressionType.TypeEqual:
                case ExpressionType.TypeIs:         //TypeBinaryExpression
                    return base.Visit(node);

                case ExpressionType.Block:
                case ExpressionType.Dynamic:
                case ExpressionType.Goto:
                case ExpressionType.Label:
                case ExpressionType.Loop:
                case ExpressionType.Switch:
                case ExpressionType.Try:
                    throw Unsupported(node);

                #endregion

                default:
                    throw Unsupported(node);
            }
        }

        private Exception Unsupported(Expression node)
        {
            Logger.Log($"Found unsupported expression '{node}' of type {node.NodeType}. Aborting query.");
            return new InvalidOperationException($"Cannot process specified expression: node type '{node.NodeType}' is not supported.");
        }
    }
}
