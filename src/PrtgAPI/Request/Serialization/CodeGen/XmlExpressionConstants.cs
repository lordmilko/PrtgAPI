using System;
using System.Linq.Expressions;
using System.Xml;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.Linq.Expressions.Serialization
{
    static class XmlExpressionConstants
    {
        internal static ParameterExpression Serializer { get; } = Expression.Parameter(typeof(XmlExpressionSerializerImpl), "serializer");
        internal static ParameterExpression XmlReader { get; } = Expression.Parameter(typeof(XmlReader), "reader");
        internal static ParameterExpression ValidateValueTypes { get; } = Expression.Parameter(typeof(bool), "validateValueTypes");
        internal static ParameterExpression SerializerFlags { get; } = Expression.Variable(typeof(bool[]), "flagArray");
        internal static ParameterExpression SerializerNames { get; } = Expression.Variable(typeof(object[]), "nameArray");

        public static ParameterExpression[] ReadElementParameters { get; } = new ParameterExpression[] { Serializer, XmlReader, ValidateValueTypes };

        #region XmlReader
            #region Properties

        internal static MemberExpression XmlReader_IsEmptyElement { get; } = Expression.MakeMemberAccess(XmlReader, XmlSerializerMembers.XmlReader_IsEmptyElement);

        internal static Expression XmlReader_Name { get; } = Expression.MakeMemberAccess(XmlReader, XmlSerializerMembers.XmlReader_Name);

        internal static MemberExpression XmlReader_NameTable { get; } = Expression.MakeMemberAccess(XmlReader, XmlSerializerMembers.XmlReader_NameTable);

        internal static MemberExpression XmlReader_NodeType { get; } = Expression.MakeMemberAccess(XmlReader, XmlSerializerMembers.XmlReader_NodeType);

            #endregion
            #region Methods

        internal static MethodCallExpression XmlReader_MoveToContent { get; } = Expression.Call(XmlReader, XmlSerializerMembers.XmlReader_MoveToContent);

        internal static MethodCallExpression XmlReader_MoveToElement { get; } = Expression.Call(XmlReader, XmlSerializerMembers.XmlReader_MoveToElement);

        internal static MethodCallExpression XmlReader_MoveToNextAttribute { get; } = Expression.Call(XmlReader, XmlSerializerMembers.XmlReader_MoveToNextAttribute);

        internal static MethodCallExpression XmlReader_ReadStartElement { get; } = Expression.Call(XmlReader, XmlSerializerMembers.XmlReader_ReadStartElement);

        internal static MethodCallExpression XmlReader_ReadEndElement { get; } = Expression.Call(XmlReader, XmlSerializerMembers.XmlReader_ReadEndElement);

        internal static MethodCallExpression XmlReader_Skip { get; } = Expression.Call(XmlReader, XmlSerializerMembers.XmlReader_Skip);

        internal static MethodCallExpression XmlReader_ReadToFollowing(Expression str)
        {
            return Expression.Call(XmlReader, XmlSerializerMembers.XmlReader_ReadToFollowing, str);
        }

        #endregion
        #endregion

        internal static MemberExpression Serializer_ElementName { get; } = Expression.MakeMemberAccess(Serializer, XmlSerializerMembers.ElementName);

        internal static MemberExpression Serializer_AttributeName { get; } = Expression.MakeMemberAccess(Serializer, XmlSerializerMembers.AttributeName);

        internal static MemberExpression Serializer_Name(XmlAttributeType type)
        {
            switch(type)
            {
                case XmlAttributeType.Element:
                case XmlAttributeType.Text:
                    return Serializer_ElementName;
                case XmlAttributeType.Attribute:
                    return Serializer_AttributeName;
                default:
                    throw new NotImplementedException($"Don't know how to get name for attribute type '{type}'.");
            }
        }

        internal static MethodCallExpression Serializer_SkipUnknownNode { get; } = Expression.Call(Serializer, XmlSerializerMembers.SkipUnknownNode);

        internal static MethodCallExpression Serializer_ReadElementString { get; } = Expression.Call(Serializer, XmlSerializerMembers.ReadElementString);

        internal static MethodCallExpression Serializer_ReadAttributeString { get; } = Expression.Call(Serializer, XmlSerializerMembers.ReadAttributeString);

        internal static MethodCallExpression Serializer_ReadTextString { get; } = Expression.Call(Serializer, XmlSerializerMembers.ReadTextString);

        internal static MethodCallExpression ToNullableString(Expression str) =>
            Expression.Call(XmlSerializerMembers.ToNullableString, str);

        internal static MethodCallExpression Fail(string str, Type type) =>
            Expression.Call(
                Serializer,
                XmlSerializerMembers.Fail,
                Expression.Constant(null, typeof(Exception)),
                Expression.Constant(str, typeof(string)),
                Expression.Constant(type, typeof(Type))
            );

        internal static MethodCallExpression FailEnum(Expression str, Type type) =>
            Expression.Call(Serializer, XmlSerializerMembers.FailEnum, str, Expression.Constant(type, typeof(Type)));

        internal static MethodCallExpression ListAdd(Expression list, Expression item) =>
            Expression.Call(list, "Add", Type.EmptyTypes, item);

        #region Primitives

        internal static MethodCallExpression ToInt32(Expression str)
        {
            return Expression.Call(Serializer, XmlSerializerMembers.ToInt32, str);
        }

        internal static MethodCallExpression ToDouble(Expression str)
        {
            return Expression.Call(Serializer, XmlSerializerMembers.ToDouble, str);
        }

        internal static MethodCallExpression ToBool(Expression str)
        {
            return Expression.Call(Serializer, XmlSerializerMembers.ToBool, str);
        }

        internal static MethodCallExpression ToDateTime(Expression str)
        {
            return Expression.Call(Serializer, XmlSerializerMembers.ToDateTime, str);
        }

        internal static MethodCallExpression ToDateTimeFromDouble(Expression @double)
        {
            return Expression.Call(XmlSerializerMembers.TypeHelpers_ConvertFromPrtgDateTimeInternal, @double);
        }

        internal static MethodCallExpression ToTimeSpan(Expression str)
        {
            return Expression.Call(Serializer, XmlSerializerMembers.ToTimeSpan, str);
        }

        internal static MethodCallExpression ToTimeSpanFromInt32(Expression integer)
        {
            return Expression.Call(XmlSerializerMembers.TypeHelpers_ConvertFromPrtgTimeSpan, Expression.Convert(integer, typeof(double)));
        }

        internal static MethodCallExpression ToSplittableStringArray(Expression readStr, Expression chars)
        {
            return Expression.Call(XmlSerializerMembers.ToSplittableStringArray, readStr, chars);
        }

        #endregion
        #region Nullable Primitives

        internal static MethodCallExpression ToNullableDateTimeSkipCheck(Expression readStr)
        {
            return Expression.Call(Serializer, XmlSerializerMembers.ToNullableDateTimeSkipCheck, readStr);
        }

        #endregion
    }
}
