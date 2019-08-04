using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using PrtgAPI.CodeGenerator.CSharp;
using PrtgAPI.CodeGenerator.Model;
using PrtgAPI.CodeGenerator.Xml;

namespace PrtgAPI.CodeGenerator
{
    public class PrtgClientGenerator
    {
        public static string Generate(string xmlConfigPath)
        {
            var path = Path.GetFullPath(xmlConfigPath);

            var model = GetDocument(path);
            
            var config = new DocumentConfig(model.Templates, model.Resources, model.CommonParameters);

            var csharpRegions = model.Methods.Regions.Select(r => r.Serialize(config)).ToList();

            return Write(csharpRegions);
        }

        internal static Document GetDocument(string path)
        {
            var text = File.ReadAllText(path);

            using (var reader = XmlReader.Create(new StringReader(text), new XmlReaderSettings { IgnoreWhitespace = true }))
            {
                var serializer = new XmlSerializer(typeof(DocumentXml));

                var xmlDoc = (DocumentXml)serializer.Deserialize(reader);

                var model = new Document(xmlDoc);

                return model;
            }
        }

        private static string Write(List<Region> regions)
        {
            var writer = new RegionWriter();
            writer.Write(regions);

            return writer.ToString().TrimEnd('\r', '\n');
        }

        private static System.Exception GetInvalidXml(XDocument response, InvalidOperationException ex)
        {
            var stream = GetStream(response);

            var xmlReader = (XmlReader)new XmlTextReader(stream)
            {
                WhitespaceHandling = WhitespaceHandling.Significant,
                Normalization = true,
                XmlResolver = null
            };

            var regex = new Regex("(.+\\()(.+)(, )(.+)(\\).+)");
            var line = Convert.ToInt32(regex.Replace(ex.Message, "$2"));
            var position = Convert.ToInt32(regex.Replace(ex.Message, "$4"));

            while (xmlReader.Read())
            {
                IXmlLineInfo xmlLineInfo = (IXmlLineInfo)xmlReader;

                if (xmlLineInfo.LineNumber == line - 1)
                {
                    var xml = xmlReader.ReadOuterXml();

                    var prevSpace = xml.LastIndexOf(' ', position) + 1;
                    var nextSpace = xml.IndexOf(' ', position);

                    var length = nextSpace - prevSpace;

                    var str = length > 0 ? xml.Substring(prevSpace, length) : xml;

                    return new InvalidOperationException(str, ex);
                }
            }

            return null;
        }

        private static Stream GetStream(XDocument response)
        {
            var stream = new MemoryStream();
            response.Save(stream);
            stream.Position = 0;

            return stream;
        }
    }
}
