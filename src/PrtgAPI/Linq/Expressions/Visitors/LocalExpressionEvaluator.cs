using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Pre-evaluates subexpressions that do not depend on data returned from PRTG.
    /// </summary>
    class LocalExpressionEvaluator : ExpressionVisitor
    {
        private List<Expression> candidates;

        public LocalExpressionEvaluator(List<Expression> candidates)
        {
            this.candidates = candidates;
        }

        public Expression Evaluate(Expression expr)
        {
            return Visit(expr);
        }

        public override Expression Visit(Expression expr)
        {
            if (expr == null)
                return null;

            if (candidates.Contains(expr))
                return EvaluateInternal(expr);

            return base.Visit(expr);
        }

        private Expression EvaluateInternal(Expression e)
        {
            Type type = e.Type;
            if (e.NodeType == ExpressionType.Convert)
            {
                // check for unnecessary convert & strip them
                var u = (UnaryExpression) e;
                if (TypeHelper.GetNonNullableType(u.Operand.Type) == TypeHelper.GetNonNullableType(type))
                {
                    e = ((UnaryExpression) e).Operand;
                }
            }
            if (e.NodeType == ExpressionType.Constant)
            {
                // in case we actually threw out a nullable conversion above, simulate it here
                // don't post-eval nodes that were already constants
                if (e.Type == type)
                {
                    return e;
                }
                else if (TypeHelper.GetNonNullableType(e.Type) == TypeHelper.GetNonNullableType(type))
                {
                    return Expression.Constant(((ConstantExpression) e).Value, type);
                }
            }
            var me = e as MemberExpression;
            if (me != null)
            {
                // member accesses off of constant's are common, and yet since these partial evals
                // are never re-used, using reflection to access the member is faster than compiling  
                // and invoking a lambda
                var ce = me.Expression as ConstantExpression;
                if (ce != null)
                {
                    return Expression.Constant(me.Member.GetValue(ce.Value), type);
                }
            }
            if (type.IsValueType)
            {
                //For example, we have a local predicate from a Where statement
                e = Expression.Convert(e, typeof(object));
            }
            Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(e);

            Func<object> fn = lambda.Compile();

            return Expression.Constant(fn(), type);
        }
    }
}
