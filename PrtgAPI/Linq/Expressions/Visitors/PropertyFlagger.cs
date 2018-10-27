using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Attributes;
using PrtgAPI.Reflection;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    /// <summary>
    /// Flags properties as being mandatory and exclusive or.
    /// </summary>
    class PropertyFlagger : LinqExpressionVisitor
    {
        private Property[] targets;

        public PropertyFlagger(params Property[] targets)
        {
            this.targets = targets;
        }

        protected override Expression VisitProperty(PropertyExpression node)
        {
            var parameters = node.PropertyInfo.
                GetAttributes<PropertyParameterAttribute>()
                .Select(a => (Property)a.Property);

            if (parameters.Any(p => targets.Any(t => t == p)))
                return new PropertyExpression((MemberExpression)node.Expression, node.PropertyInfo)
                {
                    Keep = true,
                    ExclusiveOr = true
                };

            return node;
        }
    }
}