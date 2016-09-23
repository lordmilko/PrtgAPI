using System;
using System.Collections;
using System.Text;
using System.Web;
using PrtgAPI.Helpers;

namespace PrtgAPI
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
            //private set { url = value.ToLower(); } //bug: this will mess up your password if it contains uppercase letters
            private set { url = value; }
        }

        public PrtgUrl(string server, string username, string passhash, XmlFunction function, Parameters.Parameters parameters) :
            this(server, username, passhash, function.GetDescription(), parameters)
        {
        }

        public PrtgUrl(string server, string username, string passhash, JsonFunction function, Parameters.Parameters parameters) :
            this(server, username, passhash, function.GetDescription(), parameters)
        {
        }

        public PrtgUrl(string server, string username, string passhash, CommandFunction function, Parameters.Parameters parameters) :
            this(server, username, passhash, function.GetDescription(), parameters)
        {
        }

        private PrtgUrl(string server, string username, string passhash, string function, Parameters.Parameters parameters)
        {
            StringBuilder url = new StringBuilder();

            if (server.StartsWith("http://") || server.StartsWith("https://"))
            {
                url.Append(server);
            }
            else
            {
                url.Append($"https://{server}");
            }

            url.Append(string.Format($"/api/{function}"));

            foreach (var p in parameters.GetParameters())
            {
                AddParameter(url, p.Key, p.Value);
            }

            if (!usernameFound)
                AddParameter(url, Parameter.Username, username);

            if (!passFound)
            {
                if (passhash == null)
                    throw new ArgumentNullException(nameof(passhash), $"A password or passhash must be specified. Please specify a passhash in the {nameof(passhash)} parameter, or a password or passhash in the {nameof(parameters)} parameter");

                AddParameter(url, Parameter.PassHash, passhash);
            }

            Url = url.ToString();
        }

        private void AddParameter(StringBuilder url, Parameter parameter, object value)
        {
            if (parameter == Parameter.Username)
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

            //get the content. if its not a password, capitalize it

            url.Append(delim + GetUrlComponent(parameter, value));
        }

        private static string GetUrlComponent(Parameter parameter, object value)
        {
            var parameterType = parameter.GetParameterType();
            var description = parameter.GetDescription();

            //Format String Parameter

            string s = value as string;
            if (s != null)
            {
                return FormatSingleParameterWithValEncode(description, s);
            }

            //Format Enum Parameter

            Enum e = value as Enum;
            if (e != null)
            {
                return FormatSingleParameterWithValEncode(description, e.ToString());
            }

            //Format IEnumerable Parameter

            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                if (parameterType == ParameterType.MultiValue)
                {
                    var str = GetMultiValueStr(enumerable);
                    return FormatSingleParameterWithoutValEncode(description, str);
                }

                if (parameterType == ParameterType.MultiParameter)
                {
                    return FormatMultiParameter(enumerable, description);
                }

                throw new NotImplementedException();
            }

            return FormatSingleParameterWithValEncode(description, Convert.ToString(value));
        }

        private static string FormatSingleParameterWithValEncode(string name, string val)
        {
            return FormatSingleParameterWithoutValEncode(name, HttpUtility.UrlEncode(val));
        }

        private static string FormatSingleParameterWithoutValEncode(string name, string val)
        {
            if (name == Parameter.Password.GetDescription())
                return $"{name.ToLower()}={val}";

            return $"{name}={val}".ToLower();
        }

        private static string GetMultiValueStr(IEnumerable enumerable)
        {
            var builder = new StringBuilder();

            foreach (var obj in enumerable)
            {
                builder.Append(HttpUtility.UrlEncode(Convert.ToString(obj)) + ",");
            }

            builder.Length--;

            return builder.ToString().ToLower();
        }

        private static string FormatMultiParameter(IEnumerable enumerable, string description)
        {
            var builder = new StringBuilder();

            foreach (var val in enumerable)
            {
                string query;

                if (description == Parameter.FilterXyz.GetDescription())
                {
                    query = ((ContentFilter)val).ToString();
                }
                else
                {
                    query = FormatSingleParameterWithValEncode(description, Convert.ToString(val));
                }

                builder.Append(query + "&");
            }

            builder.Length--;

            return builder.ToString();
        }
    }
}
