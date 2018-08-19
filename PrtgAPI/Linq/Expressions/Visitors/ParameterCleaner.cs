using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Replaces all <see cref="ParameterExpression"/> references in merged expressions with references to a specified input <see cref="ParameterExpression"/>.<para/>
    /// (for example, (s => s.Id == 1 AND t.Active) -> (s => s.Id == 1 AND s.Active))
    /// </summary>
    class ParameterCleaner : PrtgExpressionVisitor
    {
        private ParameterExpression newParameter;

        public ParameterCleaner(ParameterExpression parameter)
        {
            newParameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return newParameter;
        }
    }
}