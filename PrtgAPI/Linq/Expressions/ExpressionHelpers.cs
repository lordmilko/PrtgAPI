using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Reflection;

namespace PrtgAPI.Linq.Expressions
{
    static class ExpressionHelpers
    {
        [ExcludeFromCodeCoverage]
        public static Type GetElementType(this Expression expression)
        {
            var type = expression.Type;

            var enumerable = FindIEnumerable(type);

            if (enumerable == null)
                return type;

            return enumerable.GetGenericArguments()[0];
        }

        [ExcludeFromCodeCoverage]
        private static Type FindIEnumerable(Type type)
        {
            if (type == null || type == typeof(string))
                return null;

            if (type.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(type.GetElementType());

            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    var enumerableType = typeof(IEnumerable<>).MakeGenericType(arg);

                    if (enumerableType.IsAssignableFrom(type))
                        return enumerableType;
                }
            }

            var interfaces = type.GetInterfaces();

            if (interfaces.Length > 0)
            {
                foreach (var i in interfaces)
                {
                    var enumerableType = FindIEnumerable(i);

                    if (enumerableType != null)
                        return enumerableType;
                }
            }

            if (type.BaseType != null && type.BaseType != typeof(object))
                return FindIEnumerable(type.BaseType);

            return null;
        }

        internal static Expression UnwrapCast(Expression expr)
        {
            Expression child = expr;

            while (child.NodeType == ExpressionType.Convert || child.NodeType == ExpressionType.ConvertChecked)
            {
                child = ((UnaryExpression)child).Operand;
            }

            return child;
        }

        public static bool IsBool(Expression expr)
        {
            expr = SubstituteExpression.Strip(expr);

            return expr is ConstantExpression && expr.Type == typeof(bool);
        }

        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),  typeof(double),  typeof(decimal),
            typeof(long), typeof(short),   typeof(sbyte),
            typeof(byte), typeof(ulong),   typeof(ushort),
            typeof(uint), typeof(float)
        };

        public static bool IsNumeric(Type myType)
        {
            return NumericTypes.Contains(myType.GetUnderlyingType());
        }

        internal static bool IsQueryMethod(MethodCallExpression node)
        {
            //Enumerable methods cannot possibly be included in the query, unless they were included as part of the body
            //of a lambda, in which case they should be evaluated locally
            return node.Method.DeclaringType == typeof(Queryable);
        }

        /// <summary>
        /// If <paramref name="value"/> is a nullable type, creates an expression like (value == null ? null : action).
        /// If <paramref name="value"/> is not nullable, returns the expression (action).
        /// </summary>
        /// <param name="value">The value to check for null.</param>
        /// <param name="ifNotNull">The action to execute if not null.</param>
        /// <returns>If the specified value is nullable, a null conditional expression wrapping the specified action.
        /// Otherwise, simply returns the specified action.</returns>
        internal static Expression NullConditional(Expression value, Expression ifNotNull)
        {
            if (!value.Type.IsValueType || value.Type.IsNullable())
            {
                //Create an expression like (value == null)
                var isNull = Expression.Equal(value, Expression.Constant(null));

                var ifNotNullType = ifNotNull.Type;

                if (ifNotNullType.IsValueType && !ifNotNullType.IsNullable())
                {
                    ifNotNullType = typeof(Nullable<>).MakeGenericType(ifNotNullType);
                    ifNotNull = Expression.Convert(ifNotNull, ifNotNullType);
                }

                var @null = Expression.Constant(null, ifNotNullType);

                //Create an expression like (value == null ? null : action)
                var condition = Expression.Condition(isNull, @null, ifNotNull);

                return condition;
            }

            return ifNotNull;
        }

        internal static Expression BoolIfNotNull(Expression value, Expression ifNotNull)
        {
            if (!value.Type.IsValueType || Nullable.GetUnderlyingType(value.Type) != null)
            {
                //Create an expression like (value == null)
                var isNull = Expression.Equal(value, Expression.Constant(null));

                //Create an expression like (value == null ? null : action)
                var condition = Expression.Condition(isNull, Expression.Constant(false, ifNotNull.Type), ifNotNull);

                return condition;
            }

            return ifNotNull;
        }
    }
}
