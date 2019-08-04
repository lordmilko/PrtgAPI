using System.Linq.Expressions;

namespace PrtgAPI.Linq.Expressions
{
    abstract class ExpressionEx : Expression
    {
        public override ExpressionType NodeType => ExpressionType.Extension;

        protected ExpressionEx(ExpressionTypeEx type)
        {
            NodeTypeEx = type;
        }

        public ExpressionTypeEx NodeTypeEx { get; }
    }
}
