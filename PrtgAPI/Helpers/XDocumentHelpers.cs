using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Prtg.Helpers
{
    static class XDocumentHelpers
    {
        public static Stream ToStream(this XDocument document)
        {
            var stream = new MemoryStream();
            document.Save(stream);
            stream.Position = 0;

            return stream;
        }

        public static string SanitizeXml(string str)
        {
            var builder = new StringBuilder();

            foreach (var ch in str)
            {
                if (System.Xml.XmlConvert.IsXmlChar(ch))
                    builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}
