using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Reflection;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;

namespace PrtgAPI.Request.Serialization
{
    class XmlReflectionSerializerImpl
    {
        private Type outerType;
        private bool deserializeAll = true;

        public XmlReflectionSerializerImpl(Type type)
        {
            outerType = type;
        }

        public object DeserializeExisting(XDocument doc, object obj)
        {
            var item = doc.Descendants("item").First();

            var properties = item.Elements().Select(
                s => s.Name.ToString()
            ).Where(n => n != "objid" && n != "name").ToList();

            return Deserialize(outerType, obj, item, properties.ToArray());
        }

        public object Deserialize(XDocument doc, string[] properties, bool deserializeAll)
        {
            this.deserializeAll = deserializeAll;
            return Deserialize(doc, properties);
        }

        private object Deserialize(XDocument doc, params string[] properties)
        {
            return Deserialize(outerType, doc.Elements().First(), properties);
        }

        private object Deserialize(Type type, XElement elm, params string[] properties)
        {
            var obj = Activator.CreateInstance(type, true);

            return Deserialize(type, obj, elm, properties);
        }

        public static object DeserializeRawPropertyValue(Enum property, string rawName, string rawValue)
        {
            var typeLookup = property.GetEnumAttribute<TypeLookupAttribute>().Class;
            var deserializer = new XmlReflectionSerializerImpl(typeLookup);

            var elementName = $"{HtmlParser.DefaultPropertyPrefix}{rawName.TrimEnd('_')}";

            var xml = new XDocument(
                new XElement("properties",
                    new XElement(elementName, rawValue)
                )
            );

            var settings = deserializer.Deserialize(xml, elementName, null);

            var value = settings.GetTypeCache().Properties.First(p => p.Property.Name == property.ToString()).Property.GetValue(settings);

            if (value == null && rawValue != string.Empty)
                return rawValue;

            return value;
        }

        private object Deserialize(Type type, object obj, XElement elm, params string[] properties)
        {
            var mappings = ReflectionCacheManager.Map(type).Cache;

            if (properties != null && properties.Length > 0)
            {
                mappings = mappings.Where(m => properties.Any(p => m.AttributeValue.Any(v => v == p))).ToList();
            }

            foreach (var mapping in mappings)
            {
                try
                {
                    switch (mapping.AttributeType)
                    {
                        case XmlAttributeType.Element:
                            ProcessXmlElement(obj, mapping, elm);
                            break;
                        case XmlAttributeType.Attribute:
                            ProcessXmlAttribute(obj, mapping, elm);
                            break;
                        case XmlAttributeType.Text:
                            ProcessXmlText(obj, mapping, elm);
                            break;
                        default:
                            throw new NotSupportedException(); //todo: add an appropriate failure message
                    }
                }
                catch (Exception)
                {
                    //throw new Exception("An error occurred while trying to deserialize property " + mapping.Property.Name);
                    throw;
                }
            }

            return obj;
        }

        private void ProcessXmlElement(object obj, XmlMapping mapping, XElement elm)
        {
            var type = mapping.PropertyCache.Property.PropertyType;
            
            if (typeof (IEnumerable).IsAssignableFrom(type) && type != typeof (string))
            {
                ProcessEnumerableXmlElement(obj, mapping, elm);
            }
            else
            {
                if (mapping.PropertyCache.Property.PropertyType.GetTypeCache().GetAttribute<XmlRootAttribute>() != null)
                {
                    var elms = mapping.AttributeValue.Select(a => elm.Elements(a)).First(x => x != null).FirstOrDefault();

                    var result = elms != null ? Deserialize(mapping.PropertyCache.Property.PropertyType, elms) : null;

                    mapping.PropertyCache.Property.SetValue(obj, result);
                }
                else
                    ProcessSingleXmlElement(obj, mapping, elm);
            }
        }

        private void ProcessEnumerableXmlElement(object obj, XmlMapping mapping, XElement elm)
        {
            var type = mapping.PropertyCache.Property.PropertyType;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                ProcessListEnumerableXmlElement(obj, mapping, elm);
            }
            else
            {
                ProcessNonListEnumerableXmlElement(obj, mapping, elm);
            }
        }

        private void ProcessListEnumerableXmlElement(object obj, XmlMapping mapping, XElement elm)
        {
            var type = mapping.PropertyCache.Property.PropertyType;
            var underlyingType = type.GetGenericArguments().First();

            var list = Activator.CreateInstance(type);

            var elms = mapping.AttributeValue.Select(a => elm.Elements(a)).First(x => x != null).ToList();

            foreach (var e in elms)
            {
                ((IList)list).Add(Deserialize(underlyingType, e));
            }

            mapping.PropertyCache.Property.SetValue(obj, list);
        }

        private void ProcessNonListEnumerableXmlElement(object obj, XmlMapping mapping, XElement elm)
        {
            var attribute = mapping.PropertyCache.GetAttribute<SplittableStringAttribute>(true);

            if (attribute != null)
            {
                var value = mapping.AttributeValue.Select(a => elm.Element(a)).FirstOrDefault(x => x != null);

                value = NullifyMissingValue(value);

                object finalVal = value;

                if (value != null)
                {
                    finalVal = value.Value.Trim().Split(attribute.Characters, StringSplitOptions.RemoveEmptyEntries);
                }

                mapping.PropertyCache.Property.SetValue(obj, finalVal);
            }
            else
                throw new NotSupportedException(); //todo: add an appropriate failure message
        }

        private void ProcessSingleXmlElement(object obj, XmlMapping mapping, XElement elm)
        {
            var value = mapping.GetSingleXElementAttributeValue(elm);

            //priority is priority_raw on the root group, priority everywhere else. we need an alt xmlelement
            //ok we have one, we now need to treat attributevalue as a list

            try
            {
                ProcessXmlText(obj, mapping, value);
            }
            catch (Exception ex) when (!(ex is XmlDeserializationException))
            {
                throw new XmlDeserializationException(mapping.PropertyCache.Property.PropertyType.GetUnderlyingType(), value?.ToString() ?? "null", ex);
            }
        }

        private XElement NullifyMissingValue(XElement value)
        {
            if (string.IsNullOrEmpty(value?.Value))
            {
                return null;
            }

            return value;
        }

        private XAttribute NullifyMissingValue(XAttribute value)
        {
            if (string.IsNullOrEmpty(value?.ToString()) || string.IsNullOrEmpty(value.Value))
            {
                return null;
            }

            return value;
        }

        //todo: profile the performance of my method vs the .net method

        private void ProcessXmlAttribute(object obj, XmlMapping mapping, XElement elm)
        {
            var value = mapping.GetSingleXAttributeAttributeValue(elm);

            try
            {
                value = NullifyMissingValue(value);
                var finalValue = GetValue(value, mapping.PropertyCache, value?.Value, elm);

                mapping.PropertyCache.SetValue(obj, finalValue);
            }
            catch (Exception ex) when (!(ex is XmlDeserializationException))
            {
                throw new XmlDeserializationException(mapping.PropertyCache.Property.PropertyType.GetUnderlyingType(), XmlAttributeExceptionNode(value), ex);
            }
        }

        private string XmlAttributeExceptionNode(XAttribute attrib)
        {
            if (attrib == null)
                return null;

            var parent = attrib.Parent;

            var name = parent.Name.ToString();
            var value = parent.Value?.ToString();

            var builder = new StringBuilder();
            builder.Append($"<{name} {attrib.ToString()}");

            if (string.IsNullOrEmpty(value))
                builder.Append(" />");
            else
                builder.Append($"{value}</{name}>");

            return builder.ToString();
        }

        private void ProcessXmlText(object obj, XmlMapping mapping, XElement elm)
        {
            var type = mapping.PropertyCache.Property.PropertyType;

            var nullifiedElm = NullifyMissingValue(elm);

            var finalValue = GetValue(nullifiedElm, mapping.PropertyCache, nullifiedElm?.Value, nullifiedElm);

            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null && finalValue == null)
            {
                if (!deserializeAll)
                    return;

                //AttributeValue is null when processing XmlTextAttribute
                var elementName = mapping.AttributeValue.First() ?? elm.Name.LocalName;

                throw new XmlDeserializationException($"An error occurred while attempting to deserialize XML element '{elementName}': cannot assign 'null' to value type '{type.Name}'."); //value types cant be null
            }

            mapping.PropertyCache.SetValue(obj, finalValue);
        }

        private object GetValue(XObject mandatory, PropertyCache propertyCache, object value, XElement elm)
        {
            var final = mandatory == null ? null : GetValueInternal(propertyCache.Property.PropertyType, value, elm);

            var attrib = propertyCache.GetAttribute<PropertyParameterAttribute>();

            if (attrib != null)
            {
                if (attrib.Property.GetType() == typeof(Property))
                {
                    var converter = attrib.Property.GetEnumFieldCache().GetAttributes<ValueConverterAttribute>().FirstOrDefault();

                    if (converter != null)
                    {
                        return converter.Converter.Deserialize(final);
                    }
                }
            }

            return final;
        }

        private object GetValueInternal(Type type, object value, XElement elm)
        {
            if (type.IsPrimitive)
                return GetPrimitiveValue(type, value);

            if (type == typeof (string))
                return value.ToString();

            if (type.IsEnum)
                return GetEnumValue(type, value, elm);

            if (type == typeof (DateTime))
                return TypeHelpers.ConvertFromPrtgDateTime(XmlConvert.ToDouble(value.ToString()));

            if (type == typeof (TimeSpan))
                return TypeHelpers.ConvertFromPrtgTimeSpan(XmlConvert.ToDouble(value.ToString()));

            var underlying = type.GetCacheValue().Underlying;

            if (underlying != null)
                return GetValueInternal(underlying, value, elm);

            return null;
        }

        private object GetPrimitiveValue(Type type, object value)
        {
            var str = value.ToString();

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int32:
                    return XmlConvert.ToInt32(str);
                case TypeCode.Boolean:
                    return ToBoolean(str.ToLower());
                case TypeCode.Int16:
                    return XmlConvert.ToInt16(str);
                case TypeCode.Int64:
                    return XmlConvert.ToInt64(str);
                case TypeCode.Single:
                    return XmlConvert.ToSingle(str);
                case TypeCode.Double:
                    return ToDouble(str);
                case TypeCode.Decimal:
                    return XmlConvert.ToDecimal(str);
                case TypeCode.Char:
                    return XmlConvert.ToChar(str);
                case TypeCode.Byte:
                    return XmlConvert.ToByte(str);
                case TypeCode.SByte:
                    return XmlConvert.ToSByte(str);
                case TypeCode.UInt16:
                    return XmlConvert.ToUInt16(str);
                case TypeCode.UInt32:
                    return XmlConvert.ToUInt32(str);
                case TypeCode.UInt64:
                    return XmlConvert.ToUInt64(str);
            }

            throw new NotSupportedException(); //TODO - say the type is not deserializable
        }

        private object GetEnumValue(Type type, object value, XElement elm)
        {
            var e = EnumExtensions.XmlToEnumAnyAttrib(value.ToString(), type, null, allowFlags: false, allowParse: false);

            if (e == null)
            {
                var badXml = elm.ToString();

                var name = elm.Name.ToString();

                var index = name.LastIndexOf("_raw");

                if (index != -1)
                {
                    //We are the raw element, so get the normal element and add it
                    name = name.Substring(0, index);

                    var normalElm = elm.Parent?.Element(name);

                    if (normalElm != null)
                        badXml = normalElm + badXml;
                }

                var msg = elm.Parent?.Element("message");

                if (msg != null)
                    badXml = badXml + msg.ToString().Replace("&lt;", "<").Replace("&gt;", ">");

                throw new XmlDeserializationException($"Could not deserialize value '{value}' as it is not a valid member of type '{type}'. Could not process XML '{badXml}'");
            }

            return e;
        }

        private static bool ToBoolean(string s)
        {
            s = TrimString(s);

            if (s == "-1")
                return true;

            return XmlConvert.ToBoolean(s);
        }

        private static readonly char[] WhitespaceChars = { ' ', '\t', '\n', '\r' };

        // Trim a string using XML whitespace characters 
        private static string TrimString(string value)
        {
            return value.Trim(WhitespaceChars);
        }

        //Custom ToDouble with culture specific formatting (for values retrieved from scraping HTML)
        private static double ToDouble(string s)
        {
            s = TrimString(s);
            if (s == "-INF")
                return double.NegativeInfinity;
            if (s == "INF")
                return double.PositiveInfinity;

            var numberStyle = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;

            double dVal;

            //XML values should always be InvariantCulture. If value was scraped from HTML, value will be the CurrentCulture of the PRTG Server.
            if (!double.TryParse(s, numberStyle, NumberFormatInfo.InvariantInfo, out dVal))
            {
                dVal = double.Parse(s, numberStyle, NumberFormatInfo.CurrentInfo);
            }

            if (dVal == 0 && s[0] == '-')
            {
                return -0d;
            }
            return dVal;
        }
    }
}
