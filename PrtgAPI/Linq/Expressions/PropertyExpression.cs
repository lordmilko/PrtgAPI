using System;
using System.Linq.Expressions;
using System.Reflection;

namespace PrtgAPI.Linq.Expressions
{
    class PropertyExpression : ExpressionEx
    {
        public PropertyInfo PropertyInfo { get; }

        public Expression Expression { get; }

        public override Type Type => PropertyInfo.PropertyType;

        public override bool CanReduce => Expression != null;

        public bool Keep { get; set; }

        /// <summary>
        /// Indicates the property can only be specified on one side of a <see cref="ExpressionType.OrElse"/>.
        /// </summary>
        public new bool ExclusiveOr { get; set; }

        public override Expression Reduce()
        {
            return Expression;
        }

        public PropertyExpression(PropertyInfo memberInfo) : base(ExpressionTypeEx.Property)
        {
            PropertyInfo = memberInfo;
        }

        public PropertyExpression(Expression node, PropertyInfo info) : base(ExpressionTypeEx.Property)
        {
            Expression = node;
            PropertyInfo = info;
        }

        public override string ToString()
        {
            return $"<root>.{PropertyInfo.Name}";
        }
    }
}