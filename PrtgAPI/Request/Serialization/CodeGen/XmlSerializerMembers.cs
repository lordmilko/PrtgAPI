using System;
using System.Reflection;
using System.Xml;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.Linq.Expressions.Serialization
{
    static class XmlSerializerMembers
    {
        internal static PropertyInfo ElementName = typeof(XmlExpressionSerializerBase).GetProperty(nameof(XmlExpressionSerializerBase.ElementName), BindingFlags.Instance | BindingFlags.NonPublic);
        internal static FieldInfo AttributeName = typeof(XmlExpressionSerializerBase).GetField(nameof(XmlExpressionSerializerBase.AttributeName), BindingFlags.Instance | BindingFlags.NonPublic);

        internal static MethodInfo ReadElementString = GetMethod(nameof(XmlExpressionSerializerBase.ReadElementString));
        internal static MethodInfo ReadAttributeString = GetMethod(nameof(XmlExpressionSerializerBase.ReadAttributeString));
        internal static MethodInfo ReadTextString = GetMethod(nameof(XmlExpressionSerializerBase.ReadTextString));
        internal static MethodInfo Fail = GetMethod(nameof(XmlExpressionSerializerBase.Fail));
        internal static MethodInfo FailEnum = GetMethod(nameof(XmlExpressionSerializerBase.FailEnum));

        internal static MethodInfo ToNullableString = GetStaticMethod(nameof(XmlExpressionSerializerBase.ToNullableString));

        internal static MethodInfo ToInt32 = GetMethod(nameof(XmlExpressionSerializerBase.ToInt32));
        internal static MethodInfo ToDouble = GetMethod(nameof(XmlExpressionSerializerBase.ToDouble));
        internal static MethodInfo ToBool = GetMethod(nameof(XmlExpressionSerializerBase.ToBool));
        internal static MethodInfo ToTimeSpan = GetMethod(nameof(XmlExpressionSerializerBase.ToTimeSpan));
        internal static MethodInfo ToDateTime = GetMethod(nameof(XmlExpressionSerializerBase.ToDateTime));
        internal static MethodInfo ToSplittableStringArray = GetStaticMethod(nameof(XmlExpressionSerializerBase.ToSplittableStringArray));

        internal static MethodInfo ToNullableDateTimeSkipCheck = GetMethod(nameof(XmlExpressionSerializerBase.ToNullableDateTimeSkipCheck));

        internal static MethodInfo SkipUnknownNode = GetMethod(nameof(XmlExpressionSerializerBase.SkipUnknownNode));

        internal static MethodInfo String_IsNullOrEmpty = typeof(string).GetMethod(nameof(string.IsNullOrEmpty), BindingFlags.Static | BindingFlags.Public);
        internal static MethodInfo XmlNameTable_Add = typeof(XmlNameTable).GetMethod(nameof(XmlNameTable.Add), new[] { typeof(string) });
        internal static MethodInfo XmlReader_MoveToElement = typeof(XmlReader).GetMethod(nameof(XmlReader.MoveToElement));
        internal static MethodInfo XmlReader_MoveToContent = typeof(XmlReader).GetMethod(nameof(XmlReader.MoveToContent));
        internal static MethodInfo XmlReader_ReadStartElement = typeof(XmlReader).GetMethod(nameof(XmlReader.ReadStartElement), Type.EmptyTypes);
        internal static MethodInfo XmlReader_ReadEndElement = typeof(XmlReader).GetMethod(nameof(XmlReader.ReadEndElement), Type.EmptyTypes);
        internal static MethodInfo XmlReader_MoveToNextAttribute = typeof(XmlReader).GetMethod(nameof(XmlReader.MoveToNextAttribute));
        internal static MethodInfo XmlReader_Skip = typeof(XmlReader).GetMethod(nameof(XmlReader.Skip));
        internal static MethodInfo XmlReader_ReadToFollowing = typeof(XmlReader).GetMethod(nameof(XmlReader.ReadToFollowing), new[] { typeof(string) });

        internal static MethodInfo TypeHelpers_ConvertFromPrtgDateTimeInternal = typeof(TypeHelpers).GetMethod(nameof(TypeHelpers.ConvertFromPrtgDateTimeInternal), BindingFlags.Static | BindingFlags.NonPublic);
        internal static MethodInfo TypeHelpers_ConvertFromPrtgTimeSpan = typeof(TypeHelpers).GetMethod(nameof(TypeHelpers.ConvertFromPrtgTimeSpan), BindingFlags.Static | BindingFlags.NonPublic);

        internal static FieldInfo GetValueConverterInstance(Type type)
        {
            var field = type.GetField("Instance", BindingFlags.Static | BindingFlags.NonPublic);

            if (field == null)
                throw new NotImplementedException($"Type '{type}' is missing a mandatory 'Instance' field.");

            return field;
        }

        private static MethodInfo GetMethod(string name)
        {
            return typeof(XmlExpressionSerializerBase).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static MethodInfo GetStaticMethod(string name)
        {
            return typeof(XmlExpressionSerializerBase).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
        }

        internal static PropertyInfo XmlReader_IsEmptyElement = typeof(XmlReader).GetProperty(nameof(XmlReader.IsEmptyElement));
        internal static PropertyInfo XmlReader_Name = typeof(XmlReader).GetProperty(nameof(XmlReader.Name));
        internal static PropertyInfo XmlReader_NameTable = typeof(XmlReader).GetProperty(nameof(XmlReader.NameTable));
        internal static PropertyInfo XmlReader_NodeType = typeof(XmlReader).GetProperty(nameof(XmlReader.NodeType));
    }
}
