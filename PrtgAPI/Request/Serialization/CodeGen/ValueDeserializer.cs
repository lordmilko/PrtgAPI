using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Request.Serialization.ValueConverters;
using PrtgAPI.Utilities;
using XmlMapping = PrtgAPI.Request.Serialization.XmlMapping;

namespace PrtgAPI.Linq.Expressions.Serialization
{
    class ValueDeserializer
    {
        XmlMapping mapping;
        XmlSerializerGenerator generator;
        Expression readStr;

        Type PropertyType { get; }

        Func<Expression, MethodCallExpression> valueConverter;
        Type converterType;

        /// <summary>
        /// Maps enums, value converters and nullable types to their corresponding lambdas.
        /// </summary>
        private Dictionary<Type, Expression> lambdaMap = new Dictionary<Type, Expression>();

        private object lockObj = new object();

        public ValueDeserializer(
            XmlMapping mapping,
            XmlSerializerGenerator generator,
            Expression str)
        {
            this.mapping = mapping;
            this.generator = generator;
            PropertyType = mapping.PropertyCache.Property.PropertyType;
            valueConverter = GetValueConverter();
            readStr = str;
        }

        public ValueDeserializer(XmlMapping mapping, XmlSerializerGenerator generator) : this(mapping, generator, null)
        {
            readStr = ReadString();
        }

        #region Value Converter

        private Func<Expression, MethodCallExpression> GetValueConverter()
        {
            var type = PropertyType.GetUnderlyingType();

            if (type == typeof(DateTime))
                type = typeof(double);
            else if (type == typeof(TimeSpan))
                type = typeof(int);

            var attrib = mapping.PropertyCache.GetAttribute<PropertyParameterAttribute>();

            if (attrib != null && attrib.Property != null)
            {
                var converter = attrib.Property.GetEnumFieldCache().GetAttributes<ValueConverterAttribute>().FirstOrDefault();

                if (converter != null)
                {
                    converterType = converter.Type;

                    var converterExpr = GetConverterInstance(converter.Type);

                    var methodInfo = converter.Converter.GetType().GetMethods()
                        .First(m => m.Name == nameof(IValueConverter.Deserialize)
                        && m.GetParameters().Single().ParameterType == type);

                    return e => Expression.Call(converterExpr, methodInfo, e);
                }
            }

            return null;
        }

        private Expression GetConverterInstance(Type type)
        {
            var instanceExpr = Expression.Field(null, XmlSerializerMembers.GetValueConverterInstance(type));

            return instanceExpr;
        }

        private Expression ValueOrConverted(Expression value)
        {
            if (valueConverter != null)
                return valueConverter(value);

            return value;
        }

        private Expression ConvertValue(
            Expression str,
            Func<Expression, MethodCallExpression> deserializeString,
            Func<Expression, MethodCallExpression> deserializeToConverter,
            Func<Expression, MethodCallExpression> deserializeFromConverter)
        {
            Expression val = null;

            if (valueConverter != null)
            {
                if (deserializeToConverter != null)
                    val = deserializeFromConverter(valueConverter(deserializeToConverter(str)));
                else
                    val = valueConverter(deserializeString(str));
            }
            else
                val = deserializeString(str);

            return val;
        }

        #endregion

        public Expression Deserialize()
        {
            if (PropertyType == typeof(string))
                return ValueOrConverted(XmlExpressionConstants.ToNullableString(readStr));
            else if (PropertyType == typeof(int))
                return GetValue(XmlExpressionConstants.ToInt32, false);
            else if (PropertyType == typeof(double))
                return GetValue(XmlExpressionConstants.ToDouble, false);
            else if (PropertyType == typeof(bool))
                return GetValue(XmlExpressionConstants.ToBool, false);
            else if (PropertyType == typeof(DateTime))
                return GetValue(XmlExpressionConstants.ToDateTime, false, XmlExpressionConstants.ToDouble, XmlExpressionConstants.ToDateTimeFromDouble);
            else if (PropertyType == typeof(TimeSpan))
                return GetValue(XmlExpressionConstants.ToTimeSpan, false, XmlExpressionConstants.ToInt32, XmlExpressionConstants.ToTimeSpanFromInt32);
            else if (PropertyType == typeof(string[]))
            {
                var attrib = mapping.PropertyCache.GetAttribute<SplittableStringAttribute>();

                if (attrib != null)
                    return ValueOrConverted(XmlExpressionConstants.ToSplittableStringArray(readStr, Expression.Constant(attrib.Character)));
                else
                    throw new NotImplementedException();
            }
            else if (PropertyType.IsEnum)
                return ValueOrConverted(GetEnumSwitch(readStr, PropertyType));
            else if (PropertyType == typeof(int?))
                return GetValue(XmlExpressionConstants.ToInt32, true);
            else if (PropertyType == typeof(double?))
                return GetValue(XmlExpressionConstants.ToDouble, true);
            else if (PropertyType == typeof(bool?))
                return GetValue(XmlExpressionConstants.ToBool, true);
            else if (PropertyType == typeof(DateTime?))
                return GetValue(XmlExpressionConstants.ToNullableDateTimeSkipCheck, true, XmlExpressionConstants.ToDouble, XmlExpressionConstants.ToDateTimeFromDouble);
            else if (PropertyType == typeof(TimeSpan?))
                return GetValue(XmlExpressionConstants.ToTimeSpan, true, XmlExpressionConstants.ToInt32, XmlExpressionConstants.ToTimeSpanFromInt32);
            else
            {
                var underlying = Nullable.GetUnderlyingType(PropertyType);

                if (underlying != null && underlying.IsEnum)
                    return GetNullableEnumSwitch(underlying, PropertyType);
            }

            if (PropertyType.GetTypeCache().GetAttribute<XmlRootAttribute>() != null)
            {
                var customTypeGenerator = new XmlSerializerGenerator(PropertyType, null, false);

                var customTypeLambda = customTypeGenerator.MakeReadElement();

                return Expression.Invoke(customTypeLambda, customTypeGenerator.GetInnerReadElementParameters(generator?.update ?? false, true));
            }

            return null;
        }

        private Expression GetValue(
            Func<Expression, MethodCallExpression> deserializeString,
            bool nullable,
            Func<Expression, MethodCallExpression> deserializeToConverter = null,
            Func<Expression, MethodCallExpression> deserializeFromConverter = null)
        {
            if (deserializeToConverter == null && deserializeFromConverter != null)
                throw new InvalidOperationException("Cannot specify deserialize from without deserialize to.");

            if (deserializeToConverter != null && deserializeFromConverter == null)
                throw new InvalidOperationException("Cannot specify deserialize to without deserialize from.");

            if (nullable)
            {
                return GetNullableMethod(deserializeString, deserializeToConverter, deserializeFromConverter);
            }
            else
            {
                Expression val = ConvertValue(readStr, deserializeString, deserializeToConverter, deserializeFromConverter);

                return val;
            }
        }

        #region Nullable

        private Expression GetNullableMethod(
            Func<Expression, MethodCallExpression> deserializeString,
            Func<Expression, MethodCallExpression> deserializeToConverter,
            Func<Expression, MethodCallExpression> deserializeFromConverter)
        {
            Expression lambda;

            lock (lockObj)
            {
                if (valueConverter == null)
                {
                    if (lambdaMap.TryGetValue(PropertyType, out lambda))
                        return lambda;
                }
                else
                {
                    if (lambdaMap.TryGetValue(converterType, out lambda))
                        return lambda;
                }
            }

            lambda = MakeNullableMethod(deserializeString, deserializeToConverter, deserializeFromConverter);

            lock (lockObj)
            {
                if (valueConverter == null)
                    lambdaMap[PropertyType] = lambda;
                else
                    lambdaMap[converterType] = lambda;

            }

            return Expression.Invoke(lambda, readStr, XmlExpressionConstants.Serializer);
        }

        private Expression MakeNullableMethod(
            Func<Expression, MethodCallExpression> deserializeString,
            Func<Expression, MethodCallExpression> deserializeToConverter,
            Func<Expression, MethodCallExpression> deserializeFromConverter)
        {
            var parameter = Expression.Parameter(typeof(string), "str");
            var isNullOrEmpty = Expression.Call(XmlSerializerMembers.String_IsNullOrEmpty, parameter);

            var assignment = parameter.Assign(XmlExpressionConstants.ToNullableString(parameter));

            var eval = ConvertValue(parameter, deserializeString, deserializeToConverter, deserializeFromConverter);

            var nullableType = eval.Type;

            //Generally speaking, eval.Type is not the nullable type. It's only nullable
            //when a special deserializeFromConverter method has been specified (such as in the case of
            //nullable DateTime)
            if (Nullable.GetUnderlyingType(nullableType) == null)
                nullableType = typeof(Nullable<>).MakeGenericType(eval.Type);

            var condition = Expression.Condition(
                isNullOrEmpty, //string.IsNullOrEmpty(str)
                Expression.Constant(null, nullableType),
                Expression.Convert(eval, nullableType)
            );

            var name = $"ToNullable{eval.Type.Name}";

            if (valueConverter != null)
                name += "_With" + converterType.Name;

            var lambda = Expression.Lambda(
                Expression.Block(condition),
                name,
                new[] { parameter, XmlExpressionConstants.Serializer }
            );

            return XmlSerializerGenerator.LambdaOrDelegate(lambda);
        }

        #endregion
        #region Enum

        private InvocationExpression GetEnumSwitch(Expression str, Type type)
        {
            lock (lockObj)
            {
                Expression value;

                if (lambdaMap.TryGetValue(type, out value))
                    return Expression.Invoke(value, str);
            }

            var lambda = MakeEnumLambda(type);

            lock (lockObj)
            {
                lambdaMap[type] = lambda;

                return Expression.Invoke(lambda, str, XmlExpressionConstants.Serializer);
            }
        }

        private Expression MakeEnumLambda(Type type)
        {
            var p = Expression.Parameter(typeof(string), "s");

            var c = ReflectionCacheManager.GetEnumCache(type).Cache;

            var fields = c.Fields;

            List<SwitchCase> cases = new List<SwitchCase>();

            foreach (var f in fields)
            {
                if (f.Field.FieldType.IsEnum)
                {
                    var val = f.Field.GetValue(null);

                    var caseConditions = f.Attributes.Where(v => v.Key == typeof(XmlEnumAttribute) || v.Key == typeof(XmlEnumAlternateName))
                        .SelectMany(a => a.Value).Cast<XmlEnumAttribute>().Select(a => Expression.Constant(a.Name)).ToArray();

                    if (caseConditions.Length > 0)
                        cases.Add(Expression.SwitchCase(Expression.Constant(val), caseConditions));
                }
            }

            var failEnum = XmlExpressionConstants.FailEnum(p, type);
            var throwFailEnum = Expression.Throw(failEnum, type);

            Expression switchBody;

            if (cases.Count > 0)
                switchBody = Expression.Switch(p, throwFailEnum, cases.ToArray());
            else
                switchBody = throwFailEnum;

            var lambda = Expression.Lambda(
                Expression.Block(switchBody),
                $"Read{type.Name}",
                new[] { p, XmlExpressionConstants.Serializer });

            return XmlSerializerGenerator.LambdaOrDelegate(lambda);
        }

        #endregion
        #region Nullable Enum

        private Expression GetNullableEnumSwitch(Type underlyingType, Type nullableType)
        {
            lock (lockObj)
            {
                Expression value;

                if (lambdaMap.TryGetValue(nullableType, out value))
                    return Expression.Invoke(value, readStr);
            }

            var lambda = MakeNullableEnumLambda(underlyingType, nullableType);

            lock (lockObj)
            {
                lambdaMap[nullableType] = lambda;

                return Expression.Invoke(lambda, readStr, XmlExpressionConstants.Serializer);
            }
        }

        private Expression MakeNullableEnumLambda(Type underlyingType, Type nullableType)
        {
            var str = Expression.Parameter(typeof(string), "str");

            var isNullOrEmpty = Expression.Call(XmlSerializerMembers.String_IsNullOrEmpty, str);

            var @null = Expression.Constant(null, nullableType);
            var val = GetEnumSwitch(str, underlyingType);
            var cast = Expression.Convert(val, nullableType);

            var condition = Expression.Condition(
                isNullOrEmpty, //if (string.IsNullOrEmpty(str)
                @null,         //    return null;
                cast           //return ToEnum(str)
            );

            var lambda = Expression.Lambda(
                Expression.Block(
                    condition
                ),
                $"ReadNullable{underlyingType.Name}",
                new[] { str, XmlExpressionConstants.Serializer }
            );

            return XmlSerializerGenerator.LambdaOrDelegate(lambda);
        }

        #endregion

        protected virtual Expression ReadString()
        {
            if (mapping.AttributeType == XmlAttributeType.Element)
                return XmlExpressionConstants.Serializer_ReadElementString;
            if (mapping.AttributeType == XmlAttributeType.Attribute)
                return XmlExpressionConstants.Serializer_ReadAttributeString;
            if (mapping.AttributeType == XmlAttributeType.Text)
                return XmlExpressionConstants.Serializer_ReadTextString;

            throw new NotImplementedException($"Don't know how to read string for attribute type '{mapping.AttributeType}'.");
        }
    }
}
