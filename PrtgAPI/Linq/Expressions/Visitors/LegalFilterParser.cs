using System.Collections.Generic;
using System.Linq.Expressions;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    class LegalFilterParser : LinqExpressionVisitor
    {
        public List<Expression> IllegalExpressionsForSplitRequest { get; } = new List<Expression>();

        public List<Expression> IllegalIgnoredFilters { get; } = new List<Expression>();

        public bool IsIllegal { get; private set; }

        private bool strict;

        public LegalFilterParser(bool strict)
        {
            this.strict = strict;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Logger.Log($"Visiting expression '{node}'", Indentation.Three);

            switch (node.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    var illegalFilterReducer = new IllegalBinaryReducer(strict);
                    Logger.Log("Expression contains AND/OR. Identifying illegal expressions", Indentation.Four);
                    var result = illegalFilterReducer.Analyze(node);
                    IllegalExpressionsForSplitRequest.AddRange(illegalFilterReducer.illegalExpressionsForSplitRequest);
                    IllegalIgnoredFilters.AddRange(illegalFilterReducer.illegalIgnoredExpressions);
                    IsIllegal = illegalFilterReducer.IsIllegal;
                    return result;
                default:
                    Logger.Log("Expression is not an AND/OR. Returning as is", Indentation.Four);

                    //IllegalFilterReducer aims to eliminate illegal AND/OR combinations. An illegal single
                    //instance BinaryExpression could still have been specified (e.g. s.Id = s.Id), however
                    //we don't care about that here; when we collect a list of condition candidates, we'll
                    //catch any further violations upon further analysis
                    return base.VisitBinary(node);
            }
        }
    }
}