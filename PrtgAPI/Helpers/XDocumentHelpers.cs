using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;

namespace PrtgAPI.Helpers
{
    [ExcludeFromCodeCoverage]
    static class XDocumentHelpers
    {
        public static string SanitizeXml(string str)
        {
            var builder = new StringBuilder();

            foreach (var ch in str)
            {
                if (XmlConvert.IsXmlChar(ch))
                    builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}
