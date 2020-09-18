using System.Collections.Generic;
using System.Management.Automation.Language;

namespace PrtgAPI.Utilities
{
    class AstFactory
    {
        public static CommandAst Command(params CommandElementAst[] commandElements)
        {
            return Command((IEnumerable<CommandElementAst>)commandElements);
        }

        public static CommandAst Command(IEnumerable<CommandElementAst> commandElements, TokenKind invocationOperator = TokenKind.Unknown, IEnumerable<RedirectionAst> redirections = null)
        {
            return new CommandAst(new EmptyScriptExtent(true), commandElements, invocationOperator, redirections);
        }

        public static CommandExpressionAst CommandExpression(ExpressionAst expression, IEnumerable<RedirectionAst> redirections = null)
        {
            return new CommandExpressionAst(new EmptyScriptExtent(true), expression, redirections);
        }

        public static CommandParameterAst CommandParameter(string parameterName, ExpressionAst argument, IScriptExtent errorPosition)
        {
            return new CommandParameterAst(new EmptyScriptExtent(true), parameterName, argument, errorPosition);
        }

        public static ConstantExpressionAst ConstantExpression(object value)
        {
            return new ConstantExpressionAst(new EmptyScriptExtent(true), value);
        }

        public static PipelineAst Pipeline(params CommandBaseAst[] pipelineElements)
        {
            return new PipelineAst(new EmptyScriptExtent(true), pipelineElements);
        }

        public static StringConstantExpressionAst StringConstantExpression(string value, StringConstantType stringConstantType)
        {
            return new StringConstantExpressionAst(new EmptyScriptExtent(true), value, stringConstantType);
        }

        public static VariableExpressionAst VariableExpression(string variableName)
        {
            return new VariableExpressionAst(new EmptyScriptExtent(true), variableName, false);
        }
    }
}
