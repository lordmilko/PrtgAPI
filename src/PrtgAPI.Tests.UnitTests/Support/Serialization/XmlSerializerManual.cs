using System;
using System.Xml;
using PrtgAPI.Linq.Expressions.Serialization;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.Tests.UnitTests.Support.Serialization
{
    abstract class XmlSerializerManual : XmlExpressionSerializerBase
    {
        protected XmlSerializerManual(XmlReader reader) : base(reader)
        {
            Init();
        }

        protected abstract void Init();

        protected TableData<T> Deserialize<T>(
            int numProperties,
            Func<TableData<T>,
            bool[], bool> processAttributes,
            Func<TableData<T>, bool[], bool> processElements,
            Action<bool[]> validateValueTypes)
        {
            reader.MoveToContent();

            return ReadElement(numProperties, processAttributes, processElements, validateValueTypes);
        }

        public static object DeserializeObjectProperty(ObjectProperty property, string rawValue)
        {
            var result = DeserializeObjectPropertyInternal(property, rawValue);

            if (result == null && rawValue != string.Empty)
                return rawValue;

            return result;
        }

        private static object DeserializeObjectPropertyInternal(ObjectProperty property, string rawValue)
        {
            var serializer = new SingleObjectPropertySerializerManual();

            serializer.ElementName = property.ToString().ToLower();

            switch(property)
            {
                case ObjectProperty.Active:
                    return serializer.ToBool(rawValue);
                case ObjectProperty.InheritWindowsCredentials:
                    return serializer.ToNullableBool(rawValue);
                case ObjectProperty.Interval:
                    return new TableSettings { intervalStr = rawValue }.Interval;
                default:
                    throw new NotImplementedException(property.ToString());
            }
        }

        protected T ReadElement<T>(
            int numProperties,
            Func<T, bool[], bool> processAttributes,
            Func<T, bool[], bool> processElements,
            Action<bool[]> validateValueTypes) where T : new()
        {
            var obj = CreateElement<T>();

            var flagArray = new bool[numProperties];

            while (reader.MoveToNextAttribute())
            {
                AttributeName = (object)reader.Name;

                if (!processAttributes(obj, flagArray))
                {
                    SkipUnknownNode();
                }
            }

            reader.MoveToElement();

            if (reader.IsEmptyElement)
            {
                reader.Skip();
                validateValueTypes(flagArray);
                return obj;
            }

            reader.ReadStartElement();

            reader.MoveToContent();

            while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != XmlNodeType.None)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    ElementName = (object)reader.Name;

                    if (!processElements(obj, flagArray))
                    {
                        SkipUnknownNode();
                    }
                }
                else if (reader.NodeType == XmlNodeType.Text)
                {
                    if (!ProcessText(obj))
                        SkipUnknownNode();
                }
                else
                {
                    SkipUnknownNode();
                }

                reader.MoveToContent();
            }

            reader.ReadEndElement();

            validateValueTypes(flagArray);

            return obj;
        }

        protected virtual T CreateElement<T>() where T : new()
        {
            return new T();
        }

        protected virtual bool ProcessText<T>(T obj)
        {
            return false;
        }

        #region Nullable Primitives

        protected internal int? ToNullableInt32(string str)
        {
            str = ToNullableString(str);

            if (str == null)
                return null;

            return ToInt32(str);
        }

        protected internal double? ToNullableDouble(string str)
        {
            str = ToNullableString(str);

            if (str == null)
                return null;

            return ToDouble(str);
        }

        protected internal bool? ToNullableBool(string str)
        {
            str = ToNullableString(str);

            if (str == null)
                return null;

            return ToBool(str);
        }

        protected internal TimeSpan? ToNullableTimeSpan(string str)
        {
            str = ToNullableString(str);

            if (str == null)
                return null;

            return ToTimeSpan(str);
        }

        protected internal DateTime? ToNullableDateTime(string str)
        {
            str = ToNullableString(str);

            if (str == null)
                return null;

            try
            {
                return TypeHelpers.ConvertFromPrtgDateTime(XmlConvert.ToDouble(str));
            }
            catch (Exception ex)
            {
                throw Fail(ex, str, typeof(DateTime));
            }
        }

        #endregion
    }
}
