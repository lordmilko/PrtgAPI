using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace PrtgAPI.Utilities
{
    [ExcludeFromCodeCoverage]
    static class XDocumentUtilities
    {
        internal static XDocument SanitizeXml(string str)
        {
            try
            {
                return XDocument.Parse(str);
            }
            catch (XmlException)
            {
                return XDocument.Parse(SanitizeStr(str));
            }
        }

        internal static string SanitizeStr(string str)
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
