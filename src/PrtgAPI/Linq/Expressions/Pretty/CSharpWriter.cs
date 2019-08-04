using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using PrtgAPI.Request.Serialization.CodeGen;

namespace PrtgAPI.Linq.Expressions.Pretty
{
   /*
    * Based on https://github.com/jbevain/mono.linq.expressions/blob/master/Mono.Linq.Expressions/CSharpWriter.cs
    * 
    * Author:
    *   Jb Evain (jbevain@novell.com)
    *
    * (C) 2010 Novell, Inc. (http:*www.novell.com)
    *
    * Permission is hereby granted, free of charge, to any person obtaining
    * a copy of this software and associated documentation files (the
    * "Software"), to deal in the Software without restriction, including
    * without limitation the rights to use, copy, modify, merge, publish,
    * distribute, sublicense, and/or sell copies of the Software, and to
    * permit persons to whom the Software is furnished to do so, subject to
    * the following conditions:
    *
    * The above copyright notice and this permission notice shall be
    * included in all copies or substantial portions of the Software.
    *
    * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
    * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
    * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
    * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    *
    */
#if DEBUG && DEBUG_SERIALIZATION
    class CSharpWriter : ExpressionVisitor
    {
        private TextFormatter formatter;
        private SymbolDocumentInfo symbolDocument;

        public CSharpWriter(TextFormatter formatter, SymbolDocumentInfo symbolDocument)
        {
            this.formatter = formatter;
            this.symbolDocument = symbolDocument;
        }

        public LambdaExpression Print(LambdaExpression node)
        {
            VisitLambdaSignature(node);

            var body = Visit(node.Body);

            var parameters = node.Parameters.ToList();
            parameters.Add(ProxyExpression.typeBuilderParameter);

            return Expression.Lambda(body, parameters);
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return null;

            switch (node.NodeType)
            {
                case ExpressionType.ArrayIndex:
                case ExpressionType.Assign:
                case ExpressionType.AndAlso:
                case ExpressionType.Block:
                case ExpressionType.Call:
                case ExpressionType.Conditional:
                case ExpressionType.Constant:
                case ExpressionType.Convert:
                case ExpressionType.Default:
                case ExpressionType.Equal:
                case ExpressionType.Extension:
                case ExpressionType.Goto:
                case ExpressionType.Index:
                case ExpressionType.Invoke:
                case ExpressionType.Lambda:
                case ExpressionType.Loop:
                case ExpressionType.MemberAccess:
                case ExpressionType.NotEqual:
                case ExpressionType.OrElse:
                case ExpressionType.Parameter:
                case ExpressionType.New:
                case ExpressionType.NewArrayBounds:
                case ExpressionType.Not:
                case ExpressionType.Switch:
                case ExpressionType.Throw:
                    return base.Visit(node);
                default:
                    throw new NotSupportedException(node.NodeType.ToString());
            }
        }

        #region ExpressionVisitor

        protected override Expression VisitBlock(BlockExpression node)
        {
            return VisitBlockExpression(node);
        }

        protected override Expression VisitExtension(Expression node)
        {
            var ex = node as ExpressionEx;

            if (ex != null)
            {
                switch (ex.NodeTypeEx)
                {
                    case ExpressionTypeEx.ElseIf:
                        return VisitElseIf((ElseIfExpression)node);
                }

                return node;
            }

            return base.VisitExtension(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.Assign)
                return VisitAssign(node);
            else if (node.NodeType == ExpressionType.ArrayIndex)
                return VisitArrayIndex(node);
            else
                return VisitSimpleBinary(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            if (IsTernaryConditional(node))
                return VisitConditionalExpression(node);
            else
                return VisitConditionalStatement(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            Write(GetLiteral(node.Value));

            return node;
        }

        protected override Expression VisitDefault(DefaultExpression node)
        {
            Write("default");
            Write("(");
            VisitType(node.Type);
            Write(")");

            return node;
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            switch (node.Kind)
            {
                case GotoExpressionKind.Return:
                    Write("return");
                    WriteSpace();
                    return Visit(node.Value);
                case GotoExpressionKind.Break:
                    Write("break");
                    break;
                case GotoExpressionKind.Continue:
                    Write("continue");
                    break;
                case GotoExpressionKind.Goto:
                    Write("goto");
                    WriteSpace();
                    return Visit(node.Value);
                default:
                    throw new NotSupportedException();
            }

            return node;
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            Visit(node.Object);
            VisitBracketedList(node.Arguments, expression => Visit(expression));

            return node;
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            Visit(node.Expression);
            VisitArguments(node.Arguments);

            return ProxyExpression.ToMethodCallExpression(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Write(CleanGenericName(node.Name));

            return node;
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            Write("for");
            WriteSpace();
            Write("(");
            Write(";");
            Write(";");
            Write(")");
            WriteLine();

            var result = node.Update(node.BreakLabel, node.ContinueLabel, VisitAsBlock(node.Body));

            WriteLine();

            return result;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var method = node.Method;

            if (node.Object != null)
                Visit(node.Object);
            else
                VisitType(method.DeclaringType);

            Write(".");

            Write(method.Name);

            if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
                VisitGenericArguments(method.GetGenericArguments());

            VisitArguments(node.Arguments);

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null)
                Visit(node.Expression);
            else
                VisitType(node.Member.DeclaringType);

            Write(".");
            Write(node.Member.Name);

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            Write("new");
            WriteSpace();
            VisitType(node.Constructor == null ? node.Type : node.Constructor.DeclaringType);
            VisitArguments(node.Arguments);

            return node;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            if (node.NodeType == ExpressionType.NewArrayInit)
                throw new NotImplementedException();
            else if (node.NodeType == ExpressionType.NewArrayBounds)
                VisitNewArrayBounds(node);

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Write(GetParameterName(node));

            return node;
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            Write("switch");
            WriteSpace();
            Write("(");
            Visit(node.SwitchValue);
            Write(")");
            WriteLine();

            VisitBlock(() => {
                foreach (var @case in node.Cases)
                    VisitSwitchCase(@case);

                if (node.DefaultBody != null)
                {
                    Write("default");
                    Write(":");
                    WriteLine();

                    MaybeVisitAsBlock(node.DefaultBody);
                }
            });

            WriteLine();

            return node;
        }

        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            foreach (var value in node.TestValues)
            {
                Write("case");
                WriteSpace();
                Visit(value);
                Write(":");
                WriteLine();
            }

            MaybeVisitAsBlock(node.Body);

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Convert:
                    VisitConvert(node);
                    break;
                case ExpressionType.Throw:
                    VisitThrow(node);
                    break;
                default:
                    VisitSimpleUnary(node);
                    break;
            }

            return node;
        }

        #endregion
        #region Block

        internal void VisitBlock(Action action)
        {
            Write("{");
            WriteLine();
            Indent();

            action();

            Dedent();
            Write("}");
        }

        internal T VisitBlock<T>(Func<T> action) where T : Expression
        {
            T result;

            Write("{");
            WriteLine();
            Indent();

            result = action();

            Dedent();
            Write("}");

            WriteLine();

            return result;
        }

        BlockExpression VisitBlockExpression(BlockExpression node)
        {
            return VisitBlock(() => {
                VisitBlockVariables(node);

                var expressions = VisitBlockExpressions(node);

                return node.Update(node.Variables, expressions);
            });
        }

        ReadOnlyCollection<Expression> VisitBlockExpressions(BlockExpression node)
        {
            var list = new List<Expression>();

            for (int i = 0; i < node.Expressions.Count; i++)
            {
                var expression = node.Expressions[i];

                var startOffset = 0;

                if (IsActualStatement(expression) && RequiresExplicitReturn(node, i, node.Type != typeof(void)) && IsReturnStatement(expression))
                {
                    Write("return");
                    WriteSpace();
                    startOffset = "return ".Length;
                }

                var newExprs = VisitWithDebug(expression, startOffset);

                list.AddRange(newExprs);

                if (!IsActualStatement(expression))
                    continue;

                Write(";");
                WriteLine();
            }

            return new ReadOnlyCollection<Expression>(list);
        }

        static bool RequiresExplicitReturn(BlockExpression node, int index, bool returnLast)
        {
            if (!returnLast)
                return false;

            var lastIndex = node.Expressions.Count - 1;
            if (index != lastIndex)
                return false;

            var last = node.Expressions[lastIndex];
            if (last.NodeType == ExpressionType.Goto && ((GotoExpression)last).Kind == GotoExpressionKind.Return)
                return false;

            return true;
        }

        private bool IsReturnStatement(Expression expression)
        {
            return expression.NodeType != ExpressionType.Assign && expression.NodeType != ExpressionType.Throw;
        }

        void VisitBlockVariables(BlockExpression node)
        {
            foreach (var variable in node.Variables)
            {
                VisitType(variable.Type);
                WriteSpace();
                Write(GetParameterName(variable));
                Write(";");
                WriteLine();
            }

            if (node.Variables.Count > 0)
                WriteLine();
        }

        #endregion
        #region Constant

        static string GetLiteral(object value)
        {
            if (value == null)
                return "null";

            if (value.GetType().IsEnum)
                return GetEnumLiteral(value);

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    return ((bool)value) ? "true" : "false";
                case TypeCode.Char:
                    var str = Regex.Escape(value.ToString());

                    //Is the the character after the \\ equal to the original value?
                    //Doesn't need escaping!
                    if (str[1] == (char)value)
                        str = value.ToString();

                    return "'" + str + "'";
                case TypeCode.String:
                    return "\"" + ((string)value) + "\"";
                case TypeCode.Int32:
                    return ((IFormattable)value).ToString(null, System.Globalization.CultureInfo.InvariantCulture);
                default:
                    return value.ToString();
            }
        }

        static string GetEnumLiteral(object value)
        {
            var type = value.GetType();
            if (Enum.IsDefined(type, value))
                return type.Name + "." + Enum.GetName(type, value);

            throw new NotSupportedException();
        }

        #endregion
        #region Binary

        Expression VisitAssign(BinaryExpression node)
        {
            var left = Visit(node.Left);
            WriteSpace();
            Write(GetBinaryOperator(node.NodeType));
            WriteSpace();
            var right = Visit(node.Right);

            return node.Update(left, null, right);
        }

        static string GetBinaryOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.AndAlso:
                    return "&&";
                case ExpressionType.Assign:
                    return "=";
                case ExpressionType.Equal:
                    return "==";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.OrElse:
                    return "||";
                default:
                    throw new NotImplementedException(type.ToString());
            }
        }

        Expression VisitArrayIndex(BinaryExpression node)
        {
            var left = Visit(node.Left);
            Write("[");
            var right = Visit(node.Right);
            Write("]");

            return node.Update(left, null, right);
        }

        Expression VisitSimpleBinary(BinaryExpression node)
        {
            var left = VisitParenthesizedExpression(node.Left);
            WriteSpace();
            Write(GetBinaryOperator(node.NodeType));
            WriteSpace();
            var right = VisitParenthesizedExpression(node.Right);

            return node.Update(left, null, right);
        }

        #endregion
        #region Unary

        void VisitConvert(UnaryExpression node)
        {
            Write("(");
            VisitType(node.Type);
            Write(")");

            VisitParenthesizedExpression(node.Operand);
        }

        void VisitThrow(UnaryExpression node)
        {
            Write("throw");
            WriteSpace();
            Visit(node.Operand);
        }

        void VisitSimpleUnary(UnaryExpression node)
        {
            Write(GetUnaryOperator(node.NodeType));
            VisitParenthesizedExpression(node.Operand);
        }

        static string GetUnaryOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Not:
                    return "!";
                default:
                    throw new NotImplementedException(type.ToString());
            }
        }

        #endregion
        #region Conditional

        Expression VisitConditionalExpression(ConditionalExpression node)
        {
            var test = Visit(node.Test);
            WriteSpace();
            Write("?");
            WriteSpace();
            var ifTrue = Visit(node.IfTrue);
            WriteSpace();
            Write(":");
            WriteSpace();
            var ifFalse = Visit(node.IfFalse);

            return node.Update(test, ifTrue, ifFalse);
        }

        ConditionalExpression VisitConditionalStatement(ConditionalExpression node)
        {
            Write("if");
            WriteSpace();
            Write("(");

            var test = Expression.Block(VisitWithDebug(node.Test, 0));

            Write(")");
            WriteLine();

            var ifTrue = VisitAsBlock(node.IfTrue);

            if (node.IfFalse != null)
            {
                if (node.IfFalse.NodeType == ExpressionType.Default && node.IfFalse.Type == typeof(void))
                    return node.Update(test, ifTrue, node.IfFalse);

                Write("else");
                WriteLine();

                var ifFalse = VisitAsBlock(node.IfFalse);

                return node.Update(test, ifTrue, ifFalse);
            }
            else
            {
                return node.Update(test, ifTrue, node.IfFalse);
            }
        }

        Expression VisitElseIf(ElseIfExpression node)
        {
            var changed = false;
            var ifs = new List<ConditionalExpression>();
            Expression newElse = null;

            for (var i = 0; i < node.If.Length; i++)
            {
                if (i == 0)
                    Write("if");
                else
                    Write("else if");

                WriteSpace();
                Write("(");

                var test = Expression.Block(VisitWithDebug(node.If[i].Test, 0));

                Write(")");
                WriteLine();

                var ifTrue = VisitAsBlock(node.If[i].IfTrue);

                if (node.If[i].Test != test || node.If[i].IfTrue != ifTrue)
                {
                    changed = true;
                    ifs.Add(Expression.IfThenElse(test, ifTrue, node.If[i].IfFalse));
                }
                else
                    ifs.Add(node.If[i]);
            }

            if (node.Else != null)
            {
                if (node.If.Length > 0)
                {
                    Write("else");
                    WriteLine();

                    newElse = VisitAsBlock(node.Else);
                }
                else
                {
                    newElse = MaybeVisitAsBlock(node.Else, false);
                }
            }

            if (changed)
            {
                return new ElseIfExpression(ifs.ToArray(), newElse);
            }

            return node;
        }

        #endregion
        #region New Array

        void VisitNewArrayBounds(NewArrayExpression node)
        {
            Write("new");
            WriteSpace();
            VisitType(node.Type.GetElementType());

            VisitBracketedList(node.Expressions, expression => Visit(expression));
        }

        #endregion
        #region Lambda

        void VisitLambdaSignature(LambdaExpression node)
        {
            if (node.Name?.EndsWith("Outer") == true)
                Write("public static");
            else
                Write("static");
            WriteSpace();
            VisitType(node.ReturnType);
            WriteSpace();
            Write(CleanGenericName(node.Name));
            VisitParameters(node);

            WriteLine();
        }

        void VisitLambdaBody(LambdaExpression node)
        {
            if (node.Body.NodeType != ExpressionType.Block)
                VisitSingleExpressionBody(node);
            else
                VisitBlockExpressionBody(node);
        }

        void VisitSingleExpressionBody(LambdaExpression node)
        {
            VisitBlock(() => {
                if (node.ReturnType != typeof(void) && !IsStatement(node.Body))
                {
                    Write("return");
                    WriteSpace();
                }

                Visit(node.Body);

                if (!IsStatement(node.Body))
                {
                    Write(";");
                    WriteLine();
                }
            });
        }

        void VisitBlockExpressionBody(LambdaExpression node)
        {
            VisitBlockExpression((BlockExpression)node.Body);
        }

        void VisitParameters(LambdaExpression node)
        {
            VisitParenthesizedList(node.Parameters, parameter => {
                VisitType(parameter.Type);
                WriteSpace();
                Write(GetParameterName(parameter));
            });
        }

        static bool IsStatement(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Conditional:
                    return !IsTernaryConditional((ConditionalExpression)expression);
                case ExpressionType.Try:
                case ExpressionType.Loop:
                case ExpressionType.Switch:
                    return true;
                default:
                    var ext = expression as ExpressionEx;

                    if (ext != null && ext.NodeTypeEx == ExpressionTypeEx.ElseIf)
                        return true;

                    return false;
            }
        }

        #endregion
        #region Type

        void VisitType(Type type)
        {
            if (type.IsArray)
            {
                VisitArrayType(type);
                return;
            }

            if (type.IsGenericParameter)
            {
                Write(type.Name);
                return;
            }

            if (type.IsGenericType && type.IsGenericTypeDefinition)
            {
                VisitGenericTypeDefinition(type);
                return;
            }

            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                VisitGenericTypeInstance(type);
                return;
            }

            VisitSimpleType(type);
        }

        void VisitArrayType(Type type)
        {
            VisitType(type.GetElementType());
            Write("[");
            for (int i = 1; i < type.GetArrayRank(); i++)
                Write(",");
            Write("]");
        }

        void VisitGenericTypeDefinition(Type type)
        {
            Write(CleanGenericName(type.Name));
            Write("<");
            var arity = type.GetGenericArguments().Length;
            for (int i = 1; i < arity; i++)
                Write(",");
            Write(">");
        }

        void VisitGenericTypeInstance(Type type)
        {
            Write(CleanGenericName(type.Name));

            VisitGenericArguments(type.GetGenericArguments());
        }

        void VisitSimpleType(Type type)
        {
            Write(GetSimpleTypeName(type));
        }

        static string GetSimpleTypeName(Type type)
        {
            if (type == typeof(void))
                return "void";
            if (type == typeof(object))
                return "object";

            if (type.IsEnum)
                return type.Name;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Byte:
                    return "byte";
                case TypeCode.Char:
                    return "char";
                case TypeCode.Decimal:
                    return "decimal";
                case TypeCode.Double:
                    return "double";
                case TypeCode.Int16:
                    return "short";
                case TypeCode.Int32:
                    return "int";
                case TypeCode.Int64:
                    return "long";
                case TypeCode.SByte:
                    return "sbyte";
                case TypeCode.Single:
                    return "float";
                case TypeCode.String:
                    return "string";
                case TypeCode.UInt16:
                    return "ushort";
                case TypeCode.UInt32:
                    return "uint";
                case TypeCode.UInt64:
                    return "ulong";
                default:
                    return type.Name;
            }
        }

        #endregion
        #region Helpers

        private Expression MaybeVisitAsBlock(Expression node, bool indent = true)
        {
            if (node.NodeType == ExpressionType.Block)
                return VisitAsBlock(node);
            else
            {
                if (indent)
                    Indent();

                var result = VisitBlockExpressions(Expression.Block(node));

                if (indent)
                    Dedent();

                return Expression.Block(result);
            }
        }

        Expression VisitAsBlock(Expression node)
        {
            var result = Visit(node.NodeType == ExpressionType.Block ? node : Expression.Block(node));

            return result;
        }

        Expression VisitParenthesizedExpression(Expression expression)
        {
            if (RequiresParentheses(expression))
            {
                Write("(");
                var result = Visit(expression);
                Write(")");
                return result;
            }

            return Visit(expression);
        }

        static bool RequiresParentheses(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.Equal:
                case ExpressionType.Not:
                case ExpressionType.NotEqual:
                case ExpressionType.OrElse:
                    return true;
                default:
                    return false;
            }
        }

        static bool IsTernaryConditional(ConditionalExpression node)
        {
            return node.Type != typeof(void) && (node.IfTrue.NodeType != ExpressionType.Block
                || (node.IfFalse != null && node.IfFalse.NodeType != ExpressionType.Block));
        }

        void VisitGenericArguments(Type[] genericArguments)
        {
            VisitList(genericArguments, "<", VisitType, ">");
        }

        void VisitList<T>(IList<T> list, string opening, Action<T> writer, string closing)
        {
            Write(opening);

            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0)
                {
                    Write(",");
                    WriteSpace();
                }

                writer(list[i]);
            }

            Write(closing);
        }

        internal static string CleanGenericName(string name)
        {
            if (name == null)
                throw new InvalidOperationException("Cannot clean generic name of 'null'. Did you forget to name your lambda?");

            return Regex.Replace(name, "(.+)`\\d+(.*)", "$1$2");
        }

        string GetParameterName(ParameterExpression parameter)
        {
            if (!string.IsNullOrEmpty(parameter.Name))
                return parameter.Name;

            throw new NotImplementedException();
        }

        static bool IsActualStatement(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Label:
                    return false;
                case ExpressionType.Conditional:
                    return IsTernaryConditional((ConditionalExpression)expression);
                case ExpressionType.Try:
                case ExpressionType.Loop:
                case ExpressionType.Switch:
                case ExpressionType.Extension:
                    return false;
                default:
                    return true;
            }
        }

        void VisitBracketedList<T>(IList<T> list, Action<T> writer)
        {
            VisitList(list, "[", writer, "]");
        }

        void VisitParenthesizedList<T>(IList<T> list, Action<T> writer)
        {
            VisitList(list, "(", writer, ")");
        }

        void VisitArguments(IList<Expression> expressions)
        {
            VisitParenthesizedList(expressions, e =>
            {
                if (e.NodeType == ExpressionType.Constant && typeof(Type).IsAssignableFrom(e.Type))
                {
                    Write("typeof");
                    Write("(");
                    VisitType((Type)((ConstantExpression)e).Value);
                    Write(")");
                }
                else
                    Visit(e);
            });
        }

        #endregion

        private Expression[] VisitWithDebug(Expression node, int startOffset)
        {
            if (IsStatement(node)) //IsStatement works inverted for some reason
                return new[] { Visit(node) };

            var startLine = formatter.Line;
            var startRow = formatter.Row + (formatter.Indentation * 4) - startOffset;

            var newExpr = Visit(node);

            var endLine = formatter.Line;
            var endRow = formatter.Row + (formatter.Indentation * 4) + 1;

            return new[]
            {
                Expression.DebugInfo(symbolDocument, startLine, startRow, endLine, endRow),
                newExpr
            };
        }

        private bool IsBlockExpression(Expression node)
        {
            switch(node.NodeType)
            {
                case ExpressionType.Conditional:
                    if (IsTernaryConditional((ConditionalExpression)node))
                        return false;

                    return true;
                default:
                    return false;
            }
        }

        #region TextFormatter

        protected void WriteLine()
        {
            formatter.WriteLine();
        }

        protected void Write(string value)
        {
            formatter.Write(value);
        }

        protected void WriteSpace()
        {
            formatter.WriteSpace();
        }

        protected void Indent()
        {
            formatter.Indent();
        }

        protected void Dedent()
        {
            formatter.Dedent();
        }

        #endregion
    }
#endif
}
