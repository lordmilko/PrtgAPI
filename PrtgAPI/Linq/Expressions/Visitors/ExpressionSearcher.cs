using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    class ExpressionSearcher : BaseExpressionSearcher
    {
        private Func<Expression, bool> criteria;
        private Func<Expression, bool> parentExclusionCriteria;

        private HashSet<Expression> nodes = new HashSet<Expression>();

        public static List<Expression> Search(Expression expression, Func<Expression, bool> criteria, Func<Expression, bool> parentExclusionCriteria = null)
        {
            var searcher = new ExpressionSearcher(criteria, parentExclusionCriteria);
            searcher.Visit(expression);

            return searcher.nodes.ToList();
        }

        public static List<ExpressionType> GetTypes(Expression expr)
        {
            var searcher = new ExpressionTypeSearcher();

            searcher.Visit(expr);

            return searcher.types.ToList();
        }

        public static List<Expression> GetParents(Expression child, Expression tree)
        {
            var searcher = new ExpressionParentSearcher(child);

            searcher.Visit(tree);

            return searcher.parentStack.ToList();
        }

        public static List<Expression> Search<TExpression>(Expression expression) where TExpression : Expression
        {
            var searcher = new ExpressionSearcher(e => e is TExpression);
            searcher.Visit(expression);

            return searcher.nodes.ToList();
        }

        public ExpressionSearcher(Func<Expression, bool> criteria, Func<Expression, bool> parentExclusionCriteria = null)
        {
            this.criteria = criteria;
            this.parentExclusionCriteria = parentExclusionCriteria;
        }

        public override Expression Visit(Expression node)
        {
            if (parentExclusionCriteria != null && parentExclusionCriteria(node))
                return node;

            if (node != null && criteria(node))
                nodes.Add(node);

            return base.Visit(node);
        }
    }
}