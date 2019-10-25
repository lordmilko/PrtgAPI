using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Xml;
using PrtgAPI.Request.Serialization;

namespace PrtgAPI.Linq.Expressions.Serialization
{
    abstract class XmlExpressionSerializerBase
    {
        protected internal XmlReader reader;

        private object elementName;

        protected internal object ElementName
        {
            get { return elementName; }
            set
            {
                elementName = value;
                AttributeName = null;
            }
        }

        protected internal object AttributeName;
        private string PreviousElementValue;
        private string LastElementValue;
        private string LastAttributeValue;

        private XmlDocument d;

        protected XmlDocument Document
        {
            get
            {
                if (d == null)
                {
                    d = new XmlDocument(reader.NameTable);
                }
                return d;
            }
        }

        const string NullException = "An error occurred while attempting to deserialize XML element '{0}': cannot assign 'null' to value type '{1}'.";

        protected XmlExpressionSerializerBase(XmlReader reader)
        {
            this.reader = reader;
        }

        public abstract object Deserialize(bool validateValueTypes = true);

        protected internal string ReadElementString()
        {
            PreviousElementValue = LastElementValue;
            return LastElementValue = reader.ReadElementContentAsString();
        }

        protected internal string ReadAttributeString()
        {
            return LastAttributeValue = reader.ReadContentAsString();
        }

        protected internal string ReadTextString()
        {
            PreviousElementValue = LastElementValue;
            return LastElementValue = reader.ReadString();
        }

        protected internal static string ToNullableString(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;

            return s;
        }

        #region Primitives

        protected internal XmlDeserializationException Fail(Exception ex, string s, Type type)
        {
            Debug.Assert(ElementName != null, "XML element name was null");

            if (string.IsNullOrEmpty(s))
            {
                //Value types cant be null
                throw new XmlDeserializationException(string.Format(NullException, AttributeName ?? ElementName, type.Name));
            }

            var xml = new StringBuilder();

            BuildNode(xml, ElementName, LastElementValue);

            throw new XmlDeserializationException(type, xml.ToString(), ex);
        }

        protected internal XmlDeserializationException FailEnum(string s, Type type)
        {
            if (string.IsNullOrEmpty(s))
                throw new XmlDeserializationException(string.Format(NullException, ElementName, type.Name));

            var badXml = new StringBuilder();

            var nameStr = ElementName.ToString();

            var index = nameStr.IndexOf("_raw");

            if (index != -1)
            {
                nameStr = nameStr.Substring(0, index);

                BuildNode(badXml, nameStr, PreviousElementValue, false);
            }

            BuildNode(badXml, ElementName, LastElementValue);

            if (reader.ReadToFollowing("message"))
            {
                var node = WebUtility.HtmlDecode(Document.ReadNode(reader).OuterXml);

                badXml.Append(node);
            }

            throw new XmlDeserializationException($"Could not deserialize value '{s}' as it is not a valid member of type '{type}'. Could not process XML '{badXml}'.");
        }

        private void BuildNode(StringBuilder builder, object elementName, object elementValue, bool allowAttribute = true)
        {
            builder.Append($"<{elementName}");

            if (allowAttribute && AttributeName != null)
            {
                builder.Append($" {AttributeName}=\"{LastAttributeValue}\"");
            }

            if (elementValue == null)
                builder.Append(" />");
            else
                builder.Append($">{elementValue}</{elementName}>");
        }

        protected internal int ToInt32(string s)
        {
            try
            {
                return XmlConvert.ToInt32(s);
            }
            catch (Exception ex)
            {
                throw Fail(ex, s, typeof(int));
            }
        }

        //Custom ToDouble with culture specific formatting (for values retrieved from scraping HTML)
        protected internal double ToDouble(string s)
        {
            s = TrimString(s);
            if (s == "-INF")
                return double.NegativeInfinity;
            if (s == "INF")
                return double.PositiveInfinity;

            //Note the lack of NumberStyles.AllowThousands (included by default in double.Parse() with no number style specified). This will prevent
            //values from 0,1 being converted into 1 on a non-EU culture and appearing to be "successful" when they shouldn't be.
            var numberStyle = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;

            double dVal;

            try
            {
                //XML values should always be InvariantCulture. If value was scraped from HTML, value could use a comma for decimal points
                //(e.g. if European culture). If we can't convert with InvariantCulture, first let's try and convert with the user's native culture
                if (!double.TryParse(s, numberStyle, NumberFormatInfo.InvariantInfo, out dVal) && !double.TryParse(s, numberStyle, NumberFormatInfo.CurrentInfo, out dVal))
                {
                    //If neither InvariantCulture nor CurrentCulture worked, this indicates there's most likely a mismatch between the client and server cultures.
                    //Let's try and make our value InvariantCulture compliant and hope for the best
                    dVal = double.Parse(s.Replace(",", "."), numberStyle, NumberFormatInfo.InvariantInfo);
                }
            }
            catch (Exception ex)
            {
                throw Fail(ex, s, typeof(double));
            }

            if (dVal == 0 && s[0] == '-')
            {
                return -0d;
            }
            return dVal;
        }

        protected internal bool ToBool(string s)
        {
            s = TrimString(s);

            if (s == "-1")
                return true;

            try
            {
                return XmlConvert.ToBoolean(s);
            }
            catch (Exception ex)
            {
                throw Fail(ex, s, typeof(bool));
            }
        }

        protected internal TimeSpan ToTimeSpan(string s)
        {
            try
            {
                return TypeHelpers.ConvertFromPrtgTimeSpan(XmlConvert.ToDouble(s));
            }
            catch (Exception ex)
            {
                throw Fail(ex, s, typeof(TimeSpan));
            }
        }

        protected internal DateTime ToDateTime(string s)
        {
            try
            {
                return TypeHelpers.ConvertFromPrtgDateTimeInternal(XmlConvert.ToDouble(s));
            }
            catch (Exception ex)
            {
                throw Fail(ex, s, typeof(DateTime));
            }
        }

        protected internal static string[] ToSplittableStringArray(string s, params char[] chars)
        {
            if (chars.Length == 0)
                throw new ArgumentException("At least one character must be specified.", nameof(chars));

            return ToNullableString(s)?.Split(chars, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion
        #region Nullable Primitives

        protected internal DateTime? ToNullableDateTimeSkipCheck(string str)
        {
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

        protected internal void SkipUnknownNode()
        {
            if (reader.NodeType == XmlNodeType.None || reader.NodeType == XmlNodeType.Whitespace)
            {
                reader.Read();
                return;
            }

            if (reader.NodeType == XmlNodeType.EndElement || reader.NodeType == XmlNodeType.Attribute)
                return;

            if (reader.NodeType == XmlNodeType.Element)
            {
                //For debug purposes (e.g. get the invalid log status)
                ReadElementString();
                return;
            }

            //Probably an XmlNodeType.Text. Read it out of the way
            Document.ReadNode(reader);
        }

        private static readonly char[] WhitespaceChars = { ' ', '\t', '\n', '\r' };

        // Trim a string using XML whitespace characters 
        private static string TrimString(string value)
        {
            return value?.Trim(WhitespaceChars);
        }
    }
}
