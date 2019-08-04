using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PrtgAPI.Attributes;
using PrtgAPI.Exceptions.Internal;
using PrtgAPI.Reflection;
using PrtgAPI.Utilities;

namespace PrtgAPI.Linq.Expressions.Visitors
{
    class ConditionVisitor : PrtgExpressionVisitor
    {
        private Expression expr;

        private Type enumType;

        private Property? property;
        private List<FilterOperator> op = new List<FilterOperator>();
        private object value;

        private bool strict;

        public bool HasIllegalServerFilter { get; private set; }

        public ConditionVisitor(Expression expr, bool strict)
        {
            this.expr = expr;
            this.strict = strict;
        }

        public SearchFilter[] GetCondition()
        {
            try
            {
                Visit(expr);

                SearchFilter[] filters = {};

                if (strict)
                {
                    if (property == null)
                        throw Error.InvalidCondition(expr, nameof(Property));

                    if (string.IsNullOrWhiteSpace(value?.ToString()))
                        throw Error.InvalidCondition(expr, "value");

                    if (op.Count == 0)
                        throw Error.InvalidCondition(expr, nameof(FilterOperator));
                }

                if (property != null && !string.IsNullOrWhiteSpace(value?.ToString()))
                {
                    if (enumType != null && !value.GetType().IsEnum)
                    {
                        try
                        {
                            value = Enum.Parse(enumType, value.ToString());
                        }
                        catch
                        {
                            //It was never a valid enum member to begin with
                        }
                    }

                    //Don't validate inputs here; we'll do that in the parameter builder
                    //where we might manually throw an exception with a custom error message
                    filters = op.Select(o => new SearchFilter(property.Value, o, value, FilterMode.Illegal)).ToArray();
                }

                if (filters.Length == 0)
                    HasIllegalServerFilter = true;

                return filters;
            }
            catch (InvalidExpressionException ex)
            {
                //No need to handle strict here, since we handled it directly at the source
                Logger.Log(ex.Message, Indentation.Six);
                HasIllegalServerFilter = true;
                return new SearchFilter[] { };
            }
        }

        protected override Expression VisitBinary(BinaryExpression binary)
        {
            //If one side of the binary is another binary but the other ISNT, that means we've got something we can't evaluate

            Visit(binary.Left); //If multiple conditions, VisitBinary again. Otherwise, VisitMemberAccess

            var forwards = GetDirection(binary);

            switch (binary.NodeType)
            {
                case ExpressionType.Equal:
                    SetOperator(FilterOperator.Equals);
                    break;
                case ExpressionType.NotEqual:
                    SetOperator(FilterOperator.NotEquals);
                    break;
                case ExpressionType.GreaterThan:
                    SetOperator(forwards ? FilterOperator.GreaterThan : FilterOperator.LessThan);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    SetOperator(forwards ? FilterOperator.GreaterThan : FilterOperator.LessThan, FilterOperator.Equals);
                    break;
                case ExpressionType.LessThan:
                    SetOperator(forwards ? FilterOperator.LessThan : FilterOperator.GreaterThan);
                    break;
                case ExpressionType.LessThanOrEqual:
                    SetOperator(forwards ? FilterOperator.LessThan : FilterOperator.GreaterThan, FilterOperator.Equals);
                    break;
                default:
                    return binary;
            }

            Visit(binary.Right);

            return binary;
        }

        private bool GetDirection(BinaryExpression binary)
        {
            if (ExpressionSearcher.Search<PropertyExpression>(binary.Left).Count > 0)
                return true;

            return false;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Visit(node.Object);

            switch (node.Method.Name)
            {
                case FilterMethod.Contains:
                    SetOperator(FilterOperator.Contains);
                    break;
                case nameof(string.StartsWith):
                case nameof(string.EndsWith):
                    if (strict)
                        throw Error.WeakContainsCondition(node);

                    HasIllegalServerFilter = true;
                    SetOperator(FilterOperator.Contains);
                    break;
                case FilterMethod.Equals:
                    SetOperator(FilterOperator.Equals);
                    break;
                case FilterMethod.ToString:
                    //Classes, enums and structs look completely different to their ToString when serialized
                    if (IsIllegalToString(node))
                    {
                        if (strict)
                            throw Error.UnsupportedToStringTarget(expr, node.Object?.Type ?? node.Type);

                        throw new InvalidExpressionException($"Cannot call {nameof(ToString)} on a bool, class, enum or struct.");
                    }
                    break;
                default:
                    if (strict)
                        throw Error.UnsupportedMethodCall(expr, node.Method.Name);

                    throw new InvalidExpressionException($"{node.Method.Name} is not supported.");
            }

            Visit(node.Arguments);

            return node;
        }

        private bool IsIllegalToString(MethodCallExpression node)
        {
            if (node.Object == null)
                return true;

            var underlying = node.Object.Type.GetUnderlyingType();

            if (typeof(IStringEnum).IsAssignableFrom(underlying))
                return false;

            if (!underlying.IsPrimitive || underlying == typeof(bool))
                return true;
            
            return false;
        }

        protected override Expression VisitProperty(PropertyExpression member)
        {
            SetProperty(member.PropertyInfo);
            return member;
        }

        protected override Expression VisitConstant(ConstantExpression constant)
        {
            SetValue(constant.Value);

            return constant;
        }

        private void SetProperty(PropertyInfo info)
        {
            property = GetProperty(info);

            enumType = info.PropertyType.IsEnum ? info.PropertyType : null;
        }

        private void SetOperator(params FilterOperator[] operators)
        {
            op.AddRange(operators);
        }

        private void SetValue(object v)
        {
            if (value == null)
                value = v;
        }

        private Property GetProperty(PropertyInfo info)
        {
            var attrib = info.GetAttribute<PropertyParameterAttribute>();

            return (Property)attrib.Property;
        }
    }
}
