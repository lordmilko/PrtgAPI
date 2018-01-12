using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using PrtgAPI.Helpers;
using PrtgAPI.Parameters;
using System.Reflection;

namespace PrtgAPI.Request
{
    class PrtgUrl
    {
        bool usernameFound;
        bool passFound;
        bool hasQueries;

        private string url;

        public string Url
        {
            get { return url; }
            private set { url = value; }
        }

        public PrtgUrl(ConnectionDetails connectionDetails, XmlFunction function, Parameters.Parameters parameters) :
            this(connectionDetails, GetResourcePath(function), parameters)
        {
        }

        public PrtgUrl(ConnectionDetails connectionDetails, JsonFunction function, Parameters.Parameters parameters) :
            this(connectionDetails, GetResourcePath(function), parameters)
        {
        }

        public PrtgUrl(ConnectionDetails connectionDetails, CommandFunction function, Parameters.Parameters parameters) :
            this(connectionDetails, GetResourcePath(function), parameters)
        {
        }

        public PrtgUrl(ConnectionDetails connectionDetails, HtmlFunction function, Parameters.Parameters parameters) :
            this(connectionDetails, GetResourcePath(function), parameters)
        {
        }

        private PrtgUrl(ConnectionDetails connectionDetails, string function, Parameters.Parameters parameters)
        {
            StringBuilder urlBuilder = new StringBuilder();

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
                    throw new ArgumentNullException(nameof(connectionDetails.PassHash), $"A password or passhash must be specified. Please specify a passhash in the {nameof(connectionDetails.PassHash)} parameter, or a password or passhash in the {nameof(parameters)} parameter");

                AddParameter(urlBuilder, Parameter.PassHash, connectionDetails.PassHash);
            }

            Url = urlBuilder.ToString();

#if DEBUG
            Debug.WriteLine(Url);
#endif
        }
        
        internal static string AddUrlPrefix(string server)
        {
            if (server.StartsWith("http://") || server.StartsWith("https://"))
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

            urlBuilder.Append(delim + GetUrlComponent(parameter, value));
        }

        private string GetUrlComponent(Parameter parameter, object value)
        {
            var parameterType = parameter.GetParameterType();

            if (parameter == Parameter.Custom)
                return ProcessCustomParameter(value);

            if ((parameterType == ParameterType.MultiParameter || parameterType == ParameterType.MultiValue) && !(value is IEnumerable))
                value = new[] { value };

            var description = parameter.GetDescription();

            if (value is string)
                return FormatSingleParameterWithValEncode(description, (string)value);

            if (value is Enum)
                return FormatSingleParameterWithValEncode(description, ((Enum)value).ToString(), true);

            if (value is IEnumerable)
                return ProcessIEnumerableParameter(parameterType, (IEnumerable)value, description);

            return FormatSingleParameterWithValEncode(description, value);
        }

        /// <summary>
        /// Serialize one or more <see cref="CustomParameter"/> objects.
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <returns></returns>
        private string ProcessCustomParameter(object value)
        {
            var arr = value as IEnumerable<CustomParameter>;

            if (arr != null)
            {
                var builder = new StringBuilder();

                var list = arr.ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    builder.Append(FormatSingleParameterWithValEncode(list[i].Name, list[i].Value));

                    if (i < list.Count - 1)
                        builder.Append("&");
                }

                return builder.ToString();
            }

            var singleParam = value as CustomParameter;

            if (singleParam != null)
                return FormatSingleParameterWithValEncode(singleParam.Name, singleParam.Value);

            throw new ArgumentException($"Expected one or more objects of type {nameof(CustomParameter)}, however argument was of type {value.GetType()}", nameof(value));
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
                throw new ArgumentException($"Parameter '{description}' is of type {ParameterType.SingleValue}, however a list of elements was specified. Please specify a single element");

            throw new NotImplementedException($"Implementation missing for handling parameter type '{parameterType}'");
        }

        /// <summary>
        /// Formats a parameter in the format name=value with a URL encoded value.
        /// </summary>
        /// <param name="name">The name to use for the parameter</param>
        /// <param name="val">The value to assign to the parameter</param>
        /// <param name="isEnum">Whether the specified value is an <see cref="Enum"/></param>
        /// <returns></returns>
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
        /// <returns></returns>
        private string FormatSingleParameterWithoutValEncode(string name, object val, bool isEnum = false)
        {
            return FormatSingleParameterInternal(name, val, false, isEnum);
        }

        private string FormatSingleParameterInternal(string name, object val, bool encodeValue, bool isEnum = false)
        {
            string str;

            if (val is string)
                str = val.ToString();
            else
            {
                if (val is IFormattable)
                    str = ((IFormattable)val).GetSerializedFormat();
                else
                    str = Convert.ToString(val);
            }

            if (isEnum && name != Parameter.Password.GetDescription())
            {
                str = str.ToLower();

                if (encodeValue)
                    str = HttpUtility.UrlEncode(str);

                return $"{name.ToLower()}={str}";
            }

            if (encodeValue)
                str = HttpUtility.UrlEncode(str);

            return $"{name.ToLower()}={str}";
        }

        /// <summary>
        /// Retrieves the value of a <see cref="ParameterType.MultiValue"/> parameter. Result is in the form val1,val2,val3
        /// </summary>
        /// <param name="enumerable">The values to assign to the parameter</param>
        /// <returns></returns>
        private string GetMultiValueStr(IEnumerable enumerable)
        {
            var builder = new StringBuilder();

            foreach (var obj in enumerable)
            {
                builder.Append(HttpUtility.UrlEncode(obj.GetType().IsEnum ? ((Enum)obj).GetDescription() : Convert.ToString(obj)) + ",");
            }

            builder.Length--;

            return builder.ToString().ToLower();
        }

        /// <summary>
        /// Formats a <see cref="ParameterType.MultiParameter"/>. Result is in the form name=val1&amp;name=val2&amp;name=val3
        /// </summary>
        /// <param name="enumerable">The values to assign to the parameter</param>
        /// <param name="description">The serialized name of the parameter</param>
        /// <returns></returns>
        private string FormatMultiParameter(IEnumerable enumerable, string description)
        {
            var builder = new StringBuilder();

            foreach (var val in enumerable)
            {
                string query;

                if (description == Parameter.FilterXyz.GetDescription())
                {
                    var filter = (SearchFilter)val;

                    string result = null;

                    if (filter.Value.GetType().IsEnum)
                    {
                        var e = (Enum) filter.Value;

                        result = FormatFlagEnum(e, v => filter.ToString(v));
                    }

                    query = result ?? ((SearchFilter)val).ToString();
                }
                else if(val.GetType().IsEnum) //If it's an enum other than FilterXyz
                {
                    var result = FormatFlagEnum((Enum)val, v => SearchFilter.ToString(description, FilterOperator.Equals, v));

                    query = result ?? SearchFilter.ToString(description, FilterOperator.Equals, val);
                }
                else
                {
                    query = FormatSingleParameterWithValEncode(description, val);
                }

                builder.Append(query + "&");
            }

            if(builder.Length > 0)
                builder.Length--;

            return builder.ToString();
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

            if (e.GetType().GetCustomAttributes<FlagsAttribute>().Any())
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
        public override string ToString()
        {
            return url;
        }
    }
}
