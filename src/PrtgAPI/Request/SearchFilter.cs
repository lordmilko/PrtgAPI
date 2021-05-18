using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Linq.Expressions;
using PrtgAPI.Request;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Request.Serialization.ValueConverters;
using PrtgAPI.Utilities;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Represents a filter used to limit search results returned by a PRTG Request.</para>
    /// </summary>
    public class SearchFilter
    {
        /// <summary>
        /// Gets or sets the property to filter by.
        /// </summary>
        public Property Property { get; set; }

        /// <summary>
        /// Gets or sets the operator to use to filter <see cref="Property"/> with <see cref="Value"/>.
        /// </summary>
        public FilterOperator Operator { get; set; }

        /// <summary>
        /// Gets or sets the value to filter on.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets whether to validate and/or transform the serialized filter <see cref="Value"/> according to the <see cref="Property"/>'s required processing rules.
        /// </summary>
        public FilterMode FilterMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilter"/> class for specifying a condition where a property is equal to a specified value.
        /// </summary>
        /// <param name="property">Property to filter on.</param>
        /// <param name="value">Value to filter on.</param>
        public SearchFilter(Property property, object value) : this(property, FilterOperator.Equals, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilter"/> class with a specified property, operator and value.
        /// </summary>
        /// <param name="property">Property to filter on.</param>
        /// <param name="operator">Operator to use to filter <paramref name="property"/> with <paramref name="value"/></param>
        /// <param name="value">Value to filter on.</param>
        public SearchFilter(Property property, FilterOperator @operator, object value) : this(property, @operator, value, FilterMode.Normal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilter"/> class with a specified property, operator, value and filter mode.
        /// </summary>
        /// <param name="property">Property to filter on.</param>
        /// <param name="operator">Operator to use to filter <paramref name="property"/> with <paramref name="value"/></param>
        /// <param name="value">Value to filter on.</param>
        /// <param name="filterMode">Specifies whether to validate and/or transform the serialized <paramref name="value"/> according to the <paramref name="property"/>'s required processing rules.</param>
        public SearchFilter(Property property, FilterOperator @operator, object value, FilterMode filterMode = FilterMode.Normal)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value), "Search Filter value cannot be null.");

            if (value.ToString() == string.Empty && @operator != FilterOperator.Contains && filterMode == FilterMode.Normal)
                throw new ArgumentException($"Search Filter value cannot be an empty string. Specify a custom filter mode to override.", nameof(value));

            Property = property;
            Operator = @operator;
            Value = value;

            FilterMode = filterMode;

            ValidateFilter(Property, GetValue(Property, @operator, Value, FilterMode), Operator, filterMode);
        }

        private static string GetOperatorFormat(Property? property, object value, FilterOperator @operator, FilterMode filterMode)
        {
            var val = GetValue(property, @operator, value, filterMode);

            ValidateFilter(property, val, @operator, filterMode);

            var operatorDescription = @operator.GetDescription(false);

            if (operatorDescription == null)
                return WebUtility.UrlEncode(val);

            return $"@{operatorDescription}({WebUtility.UrlEncode(val)})";
        }

        internal static string GetValue(Property? property, FilterOperator op, object value, FilterMode filterMode = FilterMode.Normal)
        {
            string val;

            val = value.ToString();

            if (value.GetType().IsEnum)
            {
                var attrib = ((Enum)value).GetEnumAttribute<XmlEnumAttribute>();

                if (attrib != null)
                    val = attrib.Name;
                else
                {
                    var descriptionAttrib = ((Enum) value).GetEnumAttribute<DescriptionAttribute>();

                    if (descriptionAttrib != null)
                        val = descriptionAttrib.Description;
                }
            }
            else
            {
                if (value is DateTime)
                    val = TypeHelpers.ConvertToPrtgDateTime((DateTime) value).ToString(CultureInfo.InvariantCulture);
                else if (value is TimeSpan)
                    val = TypeHelpers.ConvertToPrtgTimeSpan((TimeSpan) value).ToString(CultureInfo.InvariantCulture);
                else if (value is bool)
                    val = GetBool(property, value);
                else if (value is IStringEnum)
                    val = ((IStringEnum) value).StringValue;
            }

            if (filterMode != FilterMode.Raw)
            {
                var converter = property?.GetEnumAttribute<ValueConverterAttribute>();

                if (converter != null)
                {
                    //If we leave all the padding we won't be able to detect numbers that have trailing 0's. e.g.
                    //0000000060 for 60 seconds won't provide room to detect 600 seconds (which would be 0000000600)
                    if (converter.Converter is IZeroPaddingConverter && op == FilterOperator.Contains)
                        val = ((IZeroPaddingConverter)converter.Converter).SerializeWithPadding(val, false);
                    else
                        val = converter.Converter.Serialize(val)?.ToString();
                }
            }

            return val;
        }

        private static string GetBool(Property? property, object value)
        {
            string val;

            if ((bool)value)
            {
                var attribute = property?.GetEnumAttribute<XmlBoolAttribute>();

                if (attribute != null && attribute.Positive == false)
                    val = "-1";
                else
                    val = "1";
            }
            else
                val = "0";

            return val;
        }

        private static void ValidateFilter(Property? property, string value, FilterOperator @operator, FilterMode filterMode)
        {
            if (filterMode == FilterMode.Illegal || filterMode == FilterMode.Raw)
                return;

            var attribute = property?.GetEnumAttribute<FilterHandlerAttribute>();

            if (attribute != null)
            {
                if (!attribute.Handler.TryFilter(@operator, value))
                {
                    if (attribute.Handler.Unsupported)
                        throw Error.UnsupportedProperty(property.Value);

                    throw Error.InvalidFilterValue(property.Value, @operator, value,attribute.Handler.ValidDescription);
                }
            }
        }

        /// <summary>
        /// Returns the formatted string representation of this filter for use in a <see cref="PrtgRequestMessage"/>
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            if (Value.IsIEnumerable())
                return string.Join("&", Value.ToIEnumerable().Select(ToString));

            return ToString(Value);
        }

        internal string ToString(object value)
        {
            return ToString($"{Parameter.FilterXyz.GetDescription()}{Property.GetDescription().ToLower()}", Operator, value, Property, FilterMode);
        }

        internal static string ToString(string parameter, FilterOperator @operator, object value, Property? property, FilterMode filterMode)
        {
            var format = $"{parameter}={GetOperatorFormat(property, value, @operator, filterMode)}";

            return format;
        }
    }
}
