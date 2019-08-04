using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    class LinqQueryBuilder<T> : ExpressionVisitor
    {
        private ConsecutiveCallManager manager = new ConsecutiveCallManager();
        private PropertyEvaluator<T> propertyEvaluator;

        private LinqInitializer init;
        private bool strict;

        public static Expression Parse(Expression expr, bool strict)
        {
            Logger.Log($"Parsing LINQ tree from expression '{expr}'", Indentation.One);

            var builder = new LinqQueryBuilder<T>(strict);

            var result = builder.Visit(expr);

            Logger.Log($"Transformed LINQ tree to expression '{result}'", Indentation.Two);

            return result;
        }

        public LinqQueryBuilder(bool strict)
        {
            propertyEvaluator = new PropertyEvaluator<T>(strict);
            init = new LinqInitializer(this, propertyEvaluator.Evaluate);
            this.strict = strict;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (ExpressionHelpers.IsQueryMethod(node))
            {
                Logger.Log($"Processing method 'Queryable.{node.Method.Name}'", Indentation.Two);

                switch (node.Method.Name)
                {
                    case nameof(Queryable.Any):
                        return Call(node, new AnyLinqExpression(node, init));
                    case nameof(Queryable.Count):
                        return Call(node, new CountLinqExpression(node, init));
                    case nameof(Queryable.First):
                    case nameof(Queryable.FirstOrDefault):
                        return Call(node, new FirstLinqExpression(node, init));
                    case nameof(Queryable.Last):
                    case nameof(Queryable.LastOrDefault):
                        if (strict)
                            throw Error.UnsupportedOptionalPredicate(node);
                        
                        return Call(node, new LastLinqExpression(node, init));
                    case nameof(Queryable.OrderBy):
                    case nameof(Queryable.OrderByDescending):
                        return Call(node, new OrderByLinqExpression(node, GetSortDirection(node.Method), init));
                    case nameof(Queryable.Select):
                        return Call(node, new SelectLinqExpression(node, init));
                    case nameof(Queryable.SelectMany):
                        return Call(node, new SelectManyLinqExpression(node, init));
                    case nameof(Queryable.Skip):
                        return Call(node, new SkipLinqExpression(node, init));
                    case nameof(Queryable.Take):
                        return Call(node, new TakeLinqExpression(node, init));
                    case nameof(Queryable.Where):
                        return Call(node, new WhereLinqExpression(node, init));
                    default:

                        Logger.Log($"Encountered unsupported method {node.Method.Name}");

                        if (strict)
                            throw Error.UnsupportedLinqExpression(node.Method.Name);

                        var result = base.VisitMethodCall(node);
                        manager.Call(node.Method.Name, null);
                        return result;
                }
            }

            return base.VisitMethodCall(node);
        }

        private Expression Call(MethodCallExpression node, LinqExpression linqExpression)
        {
            if (manager.Call(node.Method.Name, linqExpression) && linqExpression.CanUse(manager))
                return linqExpression;

            if (strict)
                throw Error.UnconsecutiveLinqExpression(node.Method.Name);

            return linqExpression.Reduce();
        }

        private SortDirection GetSortDirection(MethodInfo method)
        {
            if (method.Name == nameof(Queryable.OrderBy))
                return SortDirection.Ascending;

            return SortDirection.Descending;
        }
    }
}
