using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using PrtgAPI.Request.Serialization;
using XmlMapping = PrtgAPI.Request.Serialization.XmlMapping;

namespace PrtgAPI.Linq.Expressions.Serialization
{
    /// <summary>
    /// Construct an expression like
    /// 
    /// if (!flagArray[0] &amp;&amp; reader.Name == objid)
    /// {
    ///     obj.Name = reader.ReadElementString();
    ///     flagArray[0] = true;
    /// }
    /// else if (...)
    /// {
    ///     ...
    /// }
    /// ...
    /// </summary>
    class LadderBuilder
    {
        private XmlSerializerGenerator generator;
        XmlAttributeType attributeType;
        List<XmlMapping> Mappings => generator.Mappings;

        public LadderBuilder(XmlSerializerGenerator generator, XmlAttributeType attributeType)
        {
            this.generator = generator;
            this.attributeType = attributeType;
        }

        public Expression GetLadder()
        {
            var castReaderName = Expression.Convert(XmlExpressionConstants.XmlReader_Name, typeof(object));
            var assignReaderName = Expression.Assign(XmlExpressionConstants.Serializer_Name(attributeType), castReaderName);

            var conditions = generator.ForEachMappingValue<ConditionalExpression>(BuildConditions);

            var elseIf = new ElseIfExpression(conditions.ToArray(), XmlExpressionConstants.Serializer_SkipUnknownNode);

            if (Mappings.Where(m => m.AttributeType == attributeType).Count() > 0)
            {
                return Expression.Block(
                    assignReaderName,
                    elseIf
                );
            }
            else
                return elseIf;
        }

        private bool BuildConditions(XmlMapping mapping, int i, ref int mappingNameIndex, ref List<ConditionalExpression> result)
        {
            if (mapping.AttributeType == attributeType)
            {
                var blockBuilder = new LadderCondition(mapping, i, generator);

                var condition = blockBuilder.GetCondition(ref mappingNameIndex);

                result.Add(condition);
                return true;
            }

            return false;
        }
    }
}
