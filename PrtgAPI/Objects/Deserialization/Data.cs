using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using PrtgAPI.Helpers;

namespace PrtgAPI.Objects.Deserialization
{
    /// <summary>
    /// Deserializes XML returned from a PRTG Request.
    /// </summary>
    /// <typeparam name="T">The type of objects to create from the request.</typeparam>
    public class Data<T>
    {
        /// <summary>
        /// Total number of objects returned by the request.
        /// </summary>
        [XmlAttribute("totalcount")]
        public string TotalCount { get; set; }

        /// <summary>
        /// Version of PRTG running on the server.
        /// </summary>
        [XmlElement("prtg-version")]
        public string Version { get; set; }

        /// <summary>
        /// List of objects of type <typeparamref name="T"/> returned from the request.
        /// </summary>
        [XmlElement("item")]
        public List<T> Items { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Data{T}"/> class.
        /// </summary>
        /// <param name="doc">XML returned from a PRTG Server Request.</param>
        /// <returns></returns>
        public static Data<T> DeserializeList(XDocument doc)
        {
            return DeserializeInternal<Data<T>, T>(doc);
        }

        public static T DeserializeType(XDocument doc)
        {
            return DeserializeInternal<T, T>(doc);
        }

#pragma warning disable 693
        private static T DeserializeInternal<T, TInner>(XDocument doc)
#pragma warning restore 693
        {
            try
            {
                var deserializer = new XmlSerializer(typeof(T));
                var obj = deserializer.Deserialize(doc);
                //var deserializer = new XmlSerializer(typeof (T), new XmlRootAttribute(doc.Root.Name.ToString()));
                //var obj = deserializer.Deserialize(doc.ToStream());
                var data = (T) obj;

                return data;
            }
            catch (InvalidOperationException ex)
            {
                Exception xmlException = null;

                try
                {
                    xmlException = GetInvalidXml(doc, ex, typeof (TInner));
                }
                catch
                {
                }

                if (xmlException != null)
                    throw xmlException;

                throw;
            }
        }

        private static Exception GetInvalidXml(XDocument response, InvalidOperationException ex, Type type)
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

                    return new XmlDeserializationException(type, str, ex);
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
