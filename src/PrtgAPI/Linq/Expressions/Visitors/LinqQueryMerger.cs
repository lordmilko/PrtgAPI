using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Merges the <see cref="LambdaExpression"/>s of two sequential <see cref="LinqExpression"/> calls into a single <see cref="LambdaExpression"/>.
    /// e.g. (Where(a => b.Active).Where(b => b.Id == 1001)) => (Where(a => a.Active AND a.Id == 1001))
    /// </summary>
    class LinqQueryMerger : LinqExpressionVisitor
    {
        /// <summary>
        /// Maps a <see cref="LinqExpression"/> <see cref="Type"/> to the <see cref="LinqExpression"/> instance that will contain the merged expression.
        /// </summary>
        private Dictionary<Type, LinqExpression> combinedQueryTarget = new Dictionary<Type, LinqExpression>();

        private bool strict;

        public static Expression Parse(Expression expr, bool strict)
        {
            Logger.Log("Merging compatible LINQ expressions", Indentation.One);

            var merger = new LinqQueryMerger(strict);

            var result = merger.Visit(expr);

            if (expr != result)
                Logger.Log($"Transformed tree to expression '{result}'", Indentation.Two);
            else
                Logger.Log("Did not find any mergeable expressions", Indentation.Two);

            return result;
        }

        public LinqQueryMerger(bool strict)
        {
            this.strict = strict;
        }

        protected internal override Expression VisitWhere(WhereLinqExpression node)
        {
            return MergeLambda(
                node,
                n => n.Predicate,
                (i, o) => Expression.MakeBinary(ExpressionType.AndAlso, i.Body, o.Body),
                (s, l) => new WhereLinqExpression(node.Method, s, l)
            );
        }

        protected internal override Expression VisitOrderBy(OrderByLinqExpression node)
        {
            //Replace OrderBy1 -> OrderBy2 with a new instance of OrderBy2
            return MergeLambda(
                node,
                n => n.KeySelector,
                (i, o) => o.Body,
                (s, l) => new OrderByLinqExpression(node.Method, s, l, node.Comparer, node.SortDirection)
            );
        }

        protected internal override Expression VisitSkip(SkipLinqExpression node)
        {
            return MergeValue(
                node,
                n => n.Count,
                (i, o) => Expression.Constant(Convert.ToInt32(i.Value) + Convert.ToInt32(o.Value)),
                (s, v) => new SkipLinqExpression(node.Method, s, v)
            );
        }

        protected internal override Expression VisitTake(TakeLinqExpression node)
        {
            return MergeValue(
                node,
                n => n.Count,
                (i, o) => Expression.Constant(Math.Min(Convert.ToInt32(i.Value), Convert.ToInt32(o.Value))),
                (s, v) => new TakeLinqExpression(node.Method, s, v)
            );
        }

        private Expression MergeValue<TExpression>(
            TExpression node,
            Func<TExpression, ConstantExpression> getValue,
            Func<ConstantExpression, ConstantExpression, ConstantExpression> makeValue,
            Func<Expression, ConstantExpression, Expression> makeExpr) where TExpression : LinqExpression
        {
            return MergeExpression(
                node,
                getValue,
                makeValue,
                makeExpr
            );
        }

        private Expression MergeLambda<TExpression>(
            TExpression node,
            Func<TExpression, LambdaExpression> getLambda,
            Func<LambdaExpression, LambdaExpression, Expression> makeLambdaBody,
            Func<Expression, LambdaExpression, Expression> makeExpr) where TExpression : LinqExpression
        {
            return MergeExpression(
                node,
                getLambda,
                (inner, outer) =>
                {
                    if (inner.Parameters.Count > 1 || outer.Parameters.Count > 1)
                    {
                        //It doesn't matter whether anyone referenced one of these parameters, the method signature
                        //is different. Just stick with the first method that was called.
                        return null;
                    }

                    var newBody = makeLambdaBody(inner, outer);
                    var newLambda = Expression.Lambda(newBody, outer.Parameters);

                    if (combinedQueryTarget[typeof(TExpression)] == node)
                    {
                        newLambda = CleanLambda(newLambda);
                    }

                    return newLambda;
                },
                makeExpr
            );
        }

        private Expression MergeExpression<TLinqExpression, TParamExpression>(
            TLinqExpression node,
            Func<TLinqExpression, TParamExpression> getParam,
            Func<TParamExpression, TParamExpression, TParamExpression> makeParam,
            Func<Expression, TParamExpression, Expression> makeExpr) where TLinqExpression : LinqExpression
        {
            //Are we going to hold the merged expression, or are we simply lowly a subsequent expression.
            //We don't need to worry about updating our "reference" to the final merged expression, since we can't
            //call this method again after the chain is broken, and we recursively construct the complete thing below
            if (!combinedQueryTarget.ContainsKey(typeof(TLinqExpression)))
                combinedQueryTarget[typeof(TLinqExpression)] = node;

            //Potentially merge the sources of this expression (before merging them into us)
            var result = Visit(node.Source);

            var outerParam = getParam(node);

            if (result is TLinqExpression)
            {
                Logger.Log($"Merging expression '{result}' with '{node}'", Indentation.Two);

                var innerExpr = (TLinqExpression) result;
                var innerParam = getParam(innerExpr);

                var newParam = makeParam(innerParam, outerParam);

                //The inner or outer lambda has multiple parameters; forget merging, stick with inner
                if (newParam == null)
                {
                    if (strict)
                        throw Error.MergeMultiParameterLambda(node, innerExpr);

                    return innerExpr;
                }

                return makeExpr(innerExpr.Source, newParam);
            }
            else
                Logger.Log($"Ignoring previous expression '{result}' as it is not of type '{node.GetType().Name}'", Indentation.Two);

            return node;
        }

        private LambdaExpression CleanLambda(LambdaExpression lambda)
        {
            //Fixup all ParameterExpression references so they point to the same ParameterExpression object.
            //If the lambda took multiple parameters it will have been removed, so we don't need to clean.
            var parameterCleaner = new ParameterCleaner(lambda.Parameters.Single());

            return (LambdaExpression)parameterCleaner.Visit(lambda);
        }
    }
}
