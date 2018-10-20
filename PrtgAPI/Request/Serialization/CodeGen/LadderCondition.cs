using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using XmlMapping = PrtgAPI.Request.Serialization.XmlMapping;

namespace PrtgAPI.Linq.Expressions.Serialization
{
    /// <summary>
    /// Construct an expression like
    /// 
    /// if(!flagArray[0] &amp;&amp; reader.Name == objid)
    /// {
    ///     obj.Name = reader.ReadElementString();
    ///     flagArray[0] = true;
    /// }
    /// </summary>
    class LadderCondition
    {
        private XmlSerializerGenerator generator;

        Expression Target => generator.Target;
        XmlMapping mapping;
        int mappingIndex;

        public static bool MappingIsList(XmlMapping mapping)
        {
            var type = mapping.PropertyCache.Property.PropertyType;

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        public LadderCondition(XmlMapping mapping, int mappingIndex, XmlSerializerGenerator generator)
        {
            this.mapping = mapping;
            this.mappingIndex = mappingIndex;
            this.generator = generator;
        }

        internal ConditionalExpression GetCondition(ref int mappingNameIndex)
        {
            if(mapping.AttributeType == Request.Serialization.XmlAttributeType.Text)
            {
                var flagIndex = Expression.Constant(mappingIndex);
                var flag = Expression.ArrayAccess(XmlExpressionConstants.SerializerFlags, flagIndex);              //flagsArray[0]

                var trueBlock = GeneratePropertyBlock(flag);

                return Expression.IfThen(Expression.Constant(true), trueBlock);
            }
            else
            {
                var result = MakeElementCheckCondition(ref mappingNameIndex);
                var notFlagAndNameEqual = result.Item1;
                var flag = result.Item2;

                var trueBlock = GeneratePropertyBlock(flag);

                return Expression.IfThen(notFlagAndNameEqual, trueBlock);
            }
        }

        /// <summary>
        /// Create an expression like !flagsArray[0] &amp;&amp; (object)reader.Name == nameArray[0]
        /// </summary>
        /// <param name="mappingNameIndex">Index of the mapping name being analyzed.</param>
        /// <returns>The condition to check against and the flag of the flagArray that was used.</returns>
        private Tuple<BinaryExpression, IndexExpression> MakeElementCheckCondition(ref int mappingNameIndex)
        {
            Expression notFlag = null;
            IndexExpression flag = null;

            if (!MappingIsList(mapping))
            {
                var flagIndex = Expression.Constant(mappingIndex);
                flag = Expression.ArrayAccess(XmlExpressionConstants.SerializerFlags, flagIndex);                 //flagsArray[0]
                notFlag = Expression.Not(flag);                                                                   //!flagsArray[0]
            }

            Expression isNameEqual = null;

            foreach (var attributeName in mapping.AttributeValue)
            {
                var attributeValueIndex = Expression.Constant(mappingNameIndex);
                var name = Expression.ArrayIndex(XmlExpressionConstants.SerializerNames, attributeValueIndex);    //nameArray[0]

                var expr = Expression.Equal(XmlExpressionConstants.Serializer_Name(mapping.AttributeType), name); //(object)reader.Name == nameArray[0]

                if (isNameEqual == null)
                    isNameEqual = expr;
                else
                    isNameEqual = Expression.OrElse(isNameEqual, expr);                                           //(object)reader.Name == nameArray[0] || (object)reader.Name == nameArray[1]

                mappingNameIndex++;
            }

            var condition = isNameEqual;

            if (notFlag != null)
                condition = Expression.AndAlso(notFlag, isNameEqual);                                             //!flagsArray[0] && (object)reader.Name == nameArray[0]

            return Tuple.Create((BinaryExpression)condition, flag);
        }

        /// <summary>
        /// Generate an expression like { obj.Name = "Ping"; flagsArray[0] = true; }
        /// </summary>
        /// <param name="flag">Flag that should be set.</param>
        /// <returns>An expression that represents the operation.</returns>
        private BlockExpression GeneratePropertyBlock(Expression flag)
        {
            if (MappingIsList(mapping))
                return GenerateListPropertyBlock();
            else
                return GenerateNormalPropertyBlock(flag);
        }

        private BlockExpression GenerateListPropertyBlock()
        {
            var listInit = InitializeListIfNull(Target, mapping);

            var itemGenerator = new XmlSerializerGenerator(mapping.PropertyCache.Property.PropertyType.GetGenericArguments()[0], null, false);
            
            var makeItem = Expression.Invoke(                                                         //ReadSensor()
                itemGenerator.MakeReadElement(parentProperty: mapping.PropertyCache),
                itemGenerator.GetInnerReadElementParameters(generator.update, true)
            );
            
            //Assign the result of makeitem to a local variable before adding to the list
            //to prevent a million local variables being created.
            var temp = Expression.Variable(makeItem.Type, "temp");
            var assign = Expression.Assign(temp, makeItem);
            var add = XmlExpressionConstants.ListAdd(listInit.Item1, temp);

            var block = Expression.Block(
                typeof(void),
                new ParameterExpression[] { temp },
                new Expression[] { assign, add }
            );

            return block;
        }

        /// <summary>
        /// Generate an expression like
        /// 
        /// if (obj.Items == null)
        ///     obj.Items = new List();
        /// </summary>
        /// <param name="target">The object containing an item property.</param>
        /// <param name="mapping">The mapping for the list property.</param>
        /// <returns>An expression for initializing an list of items.</returns>
        public static Tuple<MemberExpression, ConditionalExpression> InitializeListIfNull(Expression target, XmlMapping mapping)
        {
            var memberAccess = Expression.MakeMemberAccess(target, mapping.PropertyCache.Property);   //obj.Items
            var @null = Expression.Constant(null, mapping.PropertyCache.Property.PropertyType);

            var isNull = Expression.Equal(memberAccess, @null);                                      //obj.Items == null
            var newList = Expression.New(mapping.PropertyCache.Property.PropertyType);               //new List()
            var assignNew = Expression.Assign(memberAccess, newList);
            var ifNullThenNew = Expression.IfThen(isNull, assignNew);                                //if(obj.Items == null) obj.Items = new List()

            return Tuple.Create(memberAccess, ifNullThenNew);
        }

        private BlockExpression GenerateNormalPropertyBlock(Expression flag)
        {
            var deserializer = new ValueDeserializer(mapping, generator);
            var value = deserializer.Deserialize();

            var memberAccess = Expression.MakeMemberAccess(Target, mapping.PropertyCache.Property); //obj.Name
            var memberAssignment = Expression.Assign(memberAccess, value);                          //obj.Name = "Ping"
            var setFlag = Expression.Assign(flag, Expression.Constant(true));                       //flagsArray[0] = true

            var block = Expression.Block(                                                           // {
                typeof(void),
                memberAssignment,                                                                   //     obj.Name = "Ping";
                setFlag                                                                             //     flagsArray[0] = true;
            );                                                                                      // }

            return block;
        }
    }
}
