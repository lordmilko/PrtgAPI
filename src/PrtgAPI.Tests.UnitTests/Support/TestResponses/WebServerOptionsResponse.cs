using System.Text;

namespace PrtgAPI.Tests.UnitTests.Support.TestResponses
{
    class WebServerOptionsResponse : IWebResponse
    {
        public bool IsEnglish { get; set; }

        public WebServerOptionsResponse(bool isEnglish = true)
        {
            IsEnglish = isEnglish;
        }

        public string GetResponseText(ref string address)
        {
            var builder = new StringBuilder();

            builder.Append("\r<select class=\"combo\"  data-rule-required=\"true\" name=\"languagefile_\" id=\"languagefile_\"  needsserverrestart=\"true\">");
            builder.Append("<option value=\"german.lng\">Deutsch</option>");

            if (IsEnglish)
                builder.Append("<option value=\"english.lng\" selected=\"selected\" >English</option>");
            else
                builder.Append("<option value=\"english.lng\">English</option>");

            builder.Append("<option value=\"dutch.lng\">Nederlands</option>");
            builder.Append("<option value=\"brazilian.lng\">Português (Brasil)</option>");
            builder.Append("<option value=\"russian.lng\">Pyсский (Russian)</option>");

            if (IsEnglish)
                builder.Append("<option value=\"japanese.lng\">日本語 (Japanese)</option>");
            else
                builder.Append("<option value=\"japanese.lng\" selected=\"selected\" >日本語 (Japanese)</option>");


            builder.Append("<option value=\"simplifiedchinese.lng\">简体中文 (Simplified Chinese)</option>");
            builder.Append("</select>");

            return builder.ToString();
        }
    }
}
