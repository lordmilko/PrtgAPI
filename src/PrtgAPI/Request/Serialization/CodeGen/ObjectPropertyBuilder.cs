using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Request;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Utilities;
using XmlMapping = PrtgAPI.Request.Serialization.XmlMapping;

namespace PrtgAPI.Linq.Expressions.Serialization
{
    class ObjectPropertyBuilder
    {
        private Type propertyType;
        private Dictionary<Type, List<PropertyCache>> propertyMap = new Dictionary<Type, List<PropertyCache>>();

        public ObjectPropertyBuilder(Type propertyType)
        {
            this.propertyType = propertyType;
        }

        public LambdaExpression BuildDeserializer()
        {
            var property = Expression.Parameter(propertyType, "property");
            var rawValue = Expression.Parameter(typeof(string), "rawValue");

            var result = Expression.Variable(typeof(object), "result");

            var internalLambda = MakeSwitchLambda(property, rawValue);

            var assignResult = result.Assign(Expression.Invoke(internalLambda, XmlExpressionConstants.Serializer, property, rawValue));

            var test = result.Equal(Expression.Constant(null)).AndAlso(rawValue.NotEqual(Expression.Constant(string.Empty)));

            var condition = Expression.Condition(test, Expression.Convert(rawValue, typeof(object)), result);

            var block = Expression.Block(
                new[] { result },
                assignResult,
                condition
            );

            return Expression.Lambda(
                block,
                "ReadObjectPropertyOuter",
                new[] { XmlExpressionConstants.Serializer, property, rawValue }
            );
        }

        private LambdaExpression MakeSwitchLambda(ParameterExpression property, ParameterExpression rawValue)
        {
            var c = ReflectionCacheManager.GetEnumCache(propertyType).Cache;

            var fields = c.Fields;

            List<SwitchCase> cases = new List<SwitchCase>();

            foreach (var f in fields)
            {
                if (f.Field.FieldType.IsEnum)
                {
                    var val = f.Field.GetValue(null);

                    Expression body = GetCaseBody((Enum)val, rawValue);

                    if (body != null)
                    {
                        if (body.NodeType != ExpressionType.Block)
                            body = Expression.Convert(body, typeof(object));

                        cases.Add(Expression.SwitchCase(body, Expression.Constant(val)));
                    }
                }
            }

            var @default = Expression.Constant(null);

            var assignName = XmlExpressionConstants.Serializer_Name(XmlAttributeType.Element).Assign(property.Call("ToString").Call("ToLower"));

            var @switch = Expression.Switch(property, @default, cases.ToArray());

            return Expression.Lambda(
                Expression.Block(
                    assignName,
                    @switch
                ),
                "ReadObjectPropertyInner",
                new[] { XmlExpressionConstants.Serializer, property, rawValue });
        }

        private Expression GetCaseBody(Enum property, Expression rawValue)
        {
            var viaObject = false;

            var typeLookup = property.GetEnumAttribute<TypeLookupAttribute>()?.Class; //ObjectPropertyInternal members don't necessarily have a type lookup

            if (typeLookup == null)
                return null;

            var mappings = ReflectionCacheManager.Map(typeLookup).Cache;
            var cache = ObjectPropertyParser.GetPropertyInfoViaTypeLookup(property);
            var xmlElement = cache.GetAttribute<XmlElementAttribute>(); //todo: what if this objectproperty doesnt point to a member with an xmlelementattribute?

            XmlMapping mapping = null;

            if (xmlElement != null)
            {
                mapping = mappings.FirstOrDefault(m => m.AttributeValue[0] == xmlElement.ElementName);
            }
            else
            {
                var mergeAttribute = property.GetEnumAttribute<MergeableAttribute>();

                //If we're a property like LocationName, we don't exist server side - we're only used for constructing requests
                if (mergeAttribute != null)
                    return null;

                //We have a property like Interval which uses a backing property instead.
                //Get the backing property so that we may extract the real value from the public
                //property
                var rawName = ObjectPropertyParser.GetObjectPropertyNameViaCache(property, cache);
                var elementName = $"{HtmlParser.DefaultPropertyPrefix}{rawName.TrimEnd('_')}";

                mapping = mappings.FirstOrDefault(m => m.AttributeValue[0] == elementName);

                if (mapping != null)
                    viaObject = true;
            }

            if (mapping != null)
            {
                var deserializer = new ValueDeserializer(mapping, null, rawValue);
                Expression initExpression = null;
                List<ParameterExpression> variables = new List<ParameterExpression>();
                var result = deserializer.Deserialize(ref initExpression, ref variables);

                if (viaObject)
                {
                    //Construct an expression like return new TableSettings { intervalStr = "60|60 seconds" }.Interval;
                    var settingsObj = Expression.Variable(typeLookup, "obj");
                    var assignObj = settingsObj.Assign(Expression.New(typeLookup));
                    var internalProp = Expression.MakeMemberAccess(settingsObj, mapping.PropertyCache.Property);
                    var assignInternal = internalProp.Assign(result);
                    var externalProp = Expression.MakeMemberAccess(settingsObj, cache.Property);

                    variables.Add(settingsObj);

                    var body = new List<Expression>();

                    if (initExpression != null)
                        body.Add(initExpression);

                    body.Add(assignObj);
                    body.Add(assignInternal);
                    body.Add(Expression.Convert(externalProp, typeof(object)));

                    return Expression.Block(
                        variables,
                        body
                    );
                }
                else
                {
                    if (initExpression != null)
                    {
                        return Expression.Block(
                            variables,
                            initExpression,
                            Expression.Convert(result, typeof(object))
                        );
                    }
                    else
                        return result;
                }
            }

            //Property is not deserializable
            return null;
        }
    }
}
