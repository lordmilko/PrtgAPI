using System;
using System.Reflection.Emit;
using System.Xml;
using PrtgAPI.Linq.Expressions.Serialization;

namespace PrtgAPI.Request.Serialization
{
    internal class XmlExpressionSerializerImpl : XmlExpressionSerializerBase
    {
        protected Delegate lambda;
#if DEBUG && DEBUG_SERIALIZATION
        protected TypeBuilder typeBuilder;
#endif

        public XmlExpressionSerializerImpl(Type type, XmlReader reader, bool getResult = true) : base(reader)
        {
            if (getResult)
            {
#if DEBUG && DEBUG_SERIALIZATION
                var result = XmlSerializerGenerator.Get(type);
                lambda = result.Item1;
                typeBuilder = result.Item2;
#else
                lambda = XmlSerializerGenerator.Get(type);
#endif
            }
        }

        public override object Deserialize(bool validateValueTypes)
        {
#if DEBUG && DEBUG_SERIALIZATION
            return lambda.DynamicInvoke(this, reader, validateValueTypes, typeBuilder);
#else
            return lambda.DynamicInvoke(this, reader, validateValueTypes);
#endif
        }

        public T Deserialize<T>(bool validateValueTypes)
        {
#if DEBUG && DEBUG_SERIALIZATION
            return ((Func<XmlExpressionSerializerImpl, XmlReader, bool, TypeBuilder, T>)lambda)(this, reader, validateValueTypes, typeBuilder);
#else
            return ((Func<XmlExpressionSerializerImpl, XmlReader, bool, T>)lambda)(this, reader, validateValueTypes);
#endif
        }

        public void Update<T>(T target)
        {
            var updateLambda = XmlSerializerGenerator.Update(typeof(T), target.GetType());

#if DEBUG && DEBUG_SERIALIZATION
            ((Action<XmlExpressionSerializerImpl, XmlReader, T, TypeBuilder>)updateLambda.Item1)(this, reader, target, updateLambda.Item2);
#else
            ((Action<XmlExpressionSerializerImpl, XmlReader, T>)updateLambda)(this, reader, target);
#endif
        }

        public object DeserializeObjectProperty(ObjectProperty property, string rawValue)
        {
#if DEBUG && DEBUG_SERIALIZATION
            return ((Func<XmlExpressionSerializerImpl, ObjectProperty, string, TypeBuilder, object>)lambda)(this, property, rawValue, typeBuilder);
#else
            return ((Func<XmlExpressionSerializerImpl, ObjectProperty, string, object>)lambda)(this, property, rawValue);
#endif
        }

        public object DeserializeObjectProperty(ObjectPropertyInternal property, string rawValue)
        {
#if DEBUG && DEBUG_SERIALIZATION
            return ((Func<XmlExpressionSerializerImpl, ObjectPropertyInternal, string, TypeBuilder, object>)lambda)(this, property, rawValue, typeBuilder);
#else
            return ((Func<XmlExpressionSerializerImpl, ObjectPropertyInternal, string, object>)lambda)(this, property, rawValue);
#endif
        }
    }
}
