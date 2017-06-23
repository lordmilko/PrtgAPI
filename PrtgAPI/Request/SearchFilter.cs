using System;
using System.Xml.Serialization;
using PrtgAPI.Helpers;
using PrtgAPI.Request;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Represents a filter used to limit search results returned by a PRTG Request.</para>
    /// </summary>
    public class SearchFilter
    {
        /// <summary>
        /// Property to filter on.
        /// </summary>
        public Property Property { get; set; }

        /// <summary>
        /// Operator to use to filter <see cref="Property"/> with <see cref="Value"/>.
        /// </summary>
        public FilterOperator Operator { get; set; }

        /// <summary>
        /// Value to filter on.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilter"/> class.
        /// </summary>
        /// <param name="property">Property (property) to filter on.</param>
        /// <param name="value">Value to filter on.</param>
        public SearchFilter(Property property, object value) : this(property, FilterOperator.Equals, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchFilter"/> class with a specified operator.
        /// </summary>
        /// <param name="property">Property (property) to filter on.</param>
        /// <param name="operator">Operator to use to filter <paramref name="property"/> with <paramref name="value"/></param>
        /// <param name="value">Value to filter on.</param>
        public SearchFilter(Property property, FilterOperator @operator, object value)
        {
            Property = property;
            Operator = @operator;
            Value = value;
        }

        private static string GetOperatorFormat(object value, FilterOperator @operator)
        {
            var operatorDescription = @operator.GetDescription(false);

            var val = value.ToString();

            if (value.GetType().IsEnum)
            {
                var attrib = ((Enum)value).GetEnumAttribute<XmlEnumAttribute>();

                if (attrib != null)
                    val = attrib.Name;
            }

            if (operatorDescription == null)
                return val;
            else
                return $"@{operatorDescription}({val})";
        }

        /// <summary>
        /// Returns the formatted string representation of this filter for use in a <see cref="PrtgUrl"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(Value);
        }

        internal string ToString(object value)
        {
            return ToString($"{Parameter.FilterXyz.GetDescription()}{Property.GetDescription().ToLower()}", Operator, value);
        }

        internal static string ToString(string parameter, FilterOperator @operator, object value)
        {
            var format = $"{parameter}={GetOperatorFormat(value, @operator)}";

            return format;
        }
    }
}
