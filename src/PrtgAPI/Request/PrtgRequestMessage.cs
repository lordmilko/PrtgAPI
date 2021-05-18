using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using PrtgAPI.Parameters;
using PrtgAPI.Reflection;
using PrtgAPI.Utilities;

namespace PrtgAPI.Request
{
    internal class PrtgRequestMessage
    {
        bool usernameFound;
        bool passFound;
        bool hasQueries;

        public string Url => Uri.OriginalString;

        public Uri Uri { get; }

        /// <summary>
        /// Gets a parser capable of modifying the request response. If a custom parser external to this request message
        /// is specified for the API request, this parser is ignored.
        /// </summary>
        public IResponseParser Parser { get; }

        internal PrtgRequestMessage(ConnectionDetails connectionDetails, XmlFunction function, IParameters parameters) :
            this(connectionDetails, GetResourcePath(function), parameters)
        {
        }

        internal PrtgRequestMessage(ConnectionDetails connectionDetails, JsonFunction function, IParameters parameters) :
            this(connectionDetails, GetResourcePath(function), parameters)
        {
        }

        internal PrtgRequestMessage(ConnectionDetails connectionDetails, CsvFunction function, IParameters parameters) :
            this(connectionDetails, GetResourcePath(function), parameters)
        {
        }

        internal PrtgRequestMessage(ConnectionDetails connectionDetails, CommandFunction function, IParameters parameters) :
            this(connectionDetails, GetResourcePath(function), parameters)
        {
        }

        internal PrtgRequestMessage(ConnectionDetails connectionDetails, HtmlFunction function, IParameters parameters) :
            this(connectionDetails, GetResourcePath(function), parameters)
        {
        }

        private PrtgRequestMessage(ConnectionDetails connectionDetails, string function, IParameters parameters)
        {
            var urlBuilder = new StringBuilder();

            urlBuilder.Append(AddUrlPrefix(connectionDetails.Server));

            urlBuilder.Append(function);

            foreach (var p in parameters.GetParameters())
            {
                AddParameter(urlBuilder, p.Key, p.Value);
            }

            if (!usernameFound && !parameters.Cookie)
                AddParameter(urlBuilder, Parameter.UserName, connectionDetails.UserName);

            if (!passFound && !parameters.Cookie)
            {
                if (connectionDetails.PassHash == null)
                    throw new ArgumentNullException(nameof(connectionDetails.PassHash), $"A password or passhash must be specified. Please specify a passhash in the {nameof(connectionDetails.PassHash)} parameter, or a password or passhash in the {nameof(parameters)} parameter.");

                AddParameter(urlBuilder, Parameter.PassHash, connectionDetails.PassHash);
            }

            Uri = new Uri(urlBuilder.ToString());

            Parser = parameters as IResponseParser;

#if DEBUG
            Debug.WriteLine(Url);
#endif
        }
        
        internal static string AddUrlPrefix(string server)
        {
            if (server.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || server.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return server;

            return $"https://{server}";
        }

        private static string GetResourcePath(Enum function)
        {
            return function.IsUndocumented() ? $"/{function.GetDescription()}" : $"/api/{function.GetDescription()}";
        }

        private void AddParameter(StringBuilder urlBuilder, Parameter parameter, object value)
        {
            if (parameter == Parameter.UserName)
                usernameFound = true;
            else if (parameter == Parameter.PassHash || parameter == Parameter.Password)
                passFound = true;

            string delim;

            if (!hasQueries)
            {
                delim = "?";
                hasQueries = true;
            }
            else
            {
                delim = "&";
            }

            var component = GetUrlComponent(parameter, value);

            if (!string.IsNullOrWhiteSpace(component))
                urlBuilder.Append(delim).Append(component);
        }

        private string GetUrlComponent(Parameter parameter, object value)
        {
            var parameterType = parameter.GetParameterType();

            if (parameter == Parameter.Custom)
                return ProcessCustomParameter(value);

            var name = parameter.GetDescription();

            return GetUrlComponentInternal(name, value, parameterType);
        }

        private string GetUrlComponentInternal(string name, object value, ParameterType parameterType)
        {
            if ((parameterType == ParameterType.MultiParameter || parameterType == ParameterType.MultiValue) && !(value is IEnumerable))
                value = new[] { value };

            if (value is string)
                return FormatSingleParameterWithValEncode(name, (string)value);

            if (value is Enum)
                return FormatSingleParameterWithValEncode(name, ((Enum)value).GetDescription(), true);

            if (value is IEnumerable)
                return ProcessIEnumerableParameter(parameterType, (IEnumerable)value, name);

            return FormatSingleParameterWithValEncode(name, value);
        }

        /// <summary>
        /// Serialize one or more <see cref="CustomParameter"/> objects.
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <returns>The serialized form of the specified value.</returns>
        private string ProcessCustomParameter(object value)
        {
            if (value == null)
                return string.Empty;

            var arr = value as IEnumerable<CustomParameter>;

            if (arr != null)
            {
                var builder = new StringBuilder();

                var list = arr.ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    builder.Append(GetUrlComponentInternal(list[i].Name, list[i].Value, list[i].ParameterType));

                    if (i < list.Count - 1)
                        builder.Append("&");
                }

                return builder.ToString();
            }

            var singleParam = value as CustomParameter;

            if (singleParam != null)
                return GetUrlComponentInternal(singleParam.Name, singleParam.Value, singleParam.ParameterType);

            throw new ArgumentException($"Expected parameter '{Parameter.Custom}' to contain one or more objects of type '{nameof(CustomParameter)}', however value was of type '{value.GetType()}'.", nameof(value));
        }

        private string ProcessIEnumerableParameter(ParameterType parameterType, IEnumerable value, string description)
        {
            if (parameterType == ParameterType.MultiValue)
            {
                var str = GetMultiValueStr(value);
                return FormatSingleParameterWithoutValEncode(description, str); //We already URL encoded each value when we constructed the MultiValueStr
            }

            if (parameterType == ParameterType.MultiParameter)
                return FormatMultiParameter(value, description);

            if (parameterType == ParameterType.SingleValue)
                throw new ArgumentException($"Parameter '{description}' is of type {ParameterType.SingleValue}, however a list of elements was specified. Please specify a single element.");

            throw new NotImplementedException($"Implementation missing for handling parameter type '{parameterType}'.");
        }

        /// <summary>
        /// Formats a parameter in the format name=value with a URL encoded value.
        /// </summary>
        /// <param name="name">The name to use for the parameter</param>
        /// <param name="val">The value to assign to the parameter</param>
        /// <param name="isEnum">Whether the specified value is an <see cref="Enum"/></param>
        /// <returns>The serialized form of the specified parameter.</returns>
        private string FormatSingleParameterWithValEncode(string name, object val, bool isEnum = false)
        {
            return FormatSingleParameterInternal(name, val, true, isEnum);
        }

        /// <summary>
        /// Formats a parameter in the format name=value without URL encoding the specified value.
        /// </summary>
        /// <param name="name">The name to use for the parameter</param>
        /// <param name="val">The value to assign to the parameter</param>
        /// <param name="isEnum">Whether the specified value is an <see cref="Enum"/> </param>
        /// <returns>The serialized form of the specified parameter.</returns>
        private string FormatSingleParameterWithoutValEncode(string name, object val, bool isEnum = false)
        {
            return FormatSingleParameterInternal(name, val, false, isEnum);
        }

        private string FormatSingleParameterInternal(string name, object val, bool encodeValue, bool isEnum = false)
        {
            string str = string.Empty;

            val = PSObjectUtilities.CleanPSObject(val);

            if (val is string)
                str = val.ToString();
            else
            {
                if (val is IMultipleSerializable)
                {
                    var ps = ((IMultipleSerializable) val).GetSerializedFormats().Select(f =>
                        FormatSingleParameterInternal(name, f, encodeValue, isEnum)
                    ).ToList();

                    if (ps.Count > 0)
                        return string.Join("&", ps);
                }
                else if (val is ISerializable)
                    str = ((ISerializable)val).GetSerializedFormat();
                else if (val is bool)
                    str = (bool) val ? "1" : "0";
                else
                    str = Convert.ToString(val);
            }

            if (isEnum && name != Parameter.Password.GetDescription())
            {
                str = str.ToLower();

                if (encodeValue)
                    str = WebUtility.UrlEncode(str);

                return $"{name.ToLower()}={str}";
            }

            if (encodeValue)
                str = WebUtility.UrlEncode(str);

            return $"{name.ToLower()}={str}";
        }

        /// <summary>
        /// Retrieves the value of a <see cref="ParameterType.MultiValue"/> parameter. Result is in the form val1,val2,val3
        /// </summary>
        /// <param name="enumerable">The values to assign to the parameter</param>
        /// <returns>The serialized list of values.</returns>
        private string GetMultiValueStr(IEnumerable enumerable)
        {
            var builder = new StringBuilder();

            foreach (var o in enumerable)
            {
                if (o != null)
                {
                    var obj = PSObjectUtilities.CleanPSObject(o);

                    string toEncode;

                    if (obj != null && obj.GetType().IsEnum)
                        toEncode = ((Enum)obj).GetDescription();
                    else
                    {
                        if (obj is ISerializable)
                            toEncode = ((ISerializable) obj).GetSerializedFormat();
                        else
                            toEncode = Convert.ToString(obj);
                    }

                    builder.Append(WebUtility.UrlEncode(toEncode) + ",");
                }
            }

            if (builder.Length > 0)
                builder.Length--;

            return builder.ToString().ToLower();
        }

        /// <summary>
        /// Formats a <see cref="ParameterType.MultiParameter"/>. Result is in the form name=val1&amp;name=val2&amp;name=val3
        /// </summary>
        /// <param name="enumerable">The values to assign to the parameter</param>
        /// <param name="description">The serialized name of the parameter</param>
        /// <returns>The serialized list of values.</returns>
        private string FormatMultiParameter(IEnumerable enumerable, string description)
        {
            var builder = new StringBuilder();

            foreach (var e in enumerable)
            {
                if (e != null)
                {
                    var val = PSObjectUtilities.CleanPSObject(e);

                    string query;

                    if (description == Parameter.FilterXyz.GetDescription())
                    {
                        var filter = (SearchFilter)val;

                        query = FormatMultiParameterFilter(filter, filter.Value);
                    }
                    else if (val != null && val.GetType().IsEnum) //If it's an enum other than FilterXyz
                    {
                        var result = FormatFlagEnum((Enum)val, v => SearchFilter.ToString(description, FilterOperator.Equals, v, null, FilterMode.Normal));

                        query = result ?? SearchFilter.ToString(description, FilterOperator.Equals, val, null, FilterMode.Normal);
                    }
                    else
                    {
                        query = FormatSingleParameterWithValEncode(description, val);
                    }

                    if (!string.IsNullOrWhiteSpace(query))
                        builder.Append(query + "&");
                }
            }

            if (builder.Length > 0)
                builder.Length--;

            return builder.ToString();
        }

        private string FormatMultiParameterFilter(SearchFilter filter, object value)
        {
            string query;

            string result = null;

            if (value == null)
                return null;

            if (value.GetType().IsEnum)
            {
                var e = (Enum)value;

                result = FormatFlagEnum(e, filter.ToString);
            }

            if (result != null)
                query = result;
            else
            {
                if (value.IsIEnumerable())
                {
                    var formatted = value.ToIEnumerable().Select(o => FormatMultiParameterFilter(filter, o));

                    query = string.Join("&", formatted.Where(f => f != null));
                }
                else
                    query = filter.ToString(value);
            }

            return query;
        }

        /// <summary>
        /// Split a bitmask enum into multiple parameters, for use with <see cref="ParameterType.MultiParameter"/> parameters.
        /// </summary>
        /// <param name="e">The enumeration to split</param>
        /// <param name="formatter">A function that defines how the enumeration value should be serialized</param>
        /// <returns>A sequence of one or more URL queries, in the format name=val1&amp;name=val2&amp;name=val3</returns>
        private string FormatFlagEnum(Enum e, Func<Enum, string> formatter)
        {
            string query = null;

            if (e.GetEnumTypeCache().GetAttributes<FlagsAttribute>().Length > 0)
            {
                var flags = e.GetUnderlyingFlags().ToList();

                if (flags.Count > 0)
                {
                    var enumBuilder = new StringBuilder();

                    foreach (var @enum in flags)
                    {
                        enumBuilder.Append(formatter(@enum) + "&");
                    }

                    enumBuilder.Length--;

                    query = enumBuilder.ToString();
                }
            }

            return query;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return Url;
        }
    }
}
