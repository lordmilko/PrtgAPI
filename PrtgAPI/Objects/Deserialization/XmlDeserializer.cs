using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PrtgAPI.Objects.Deserialization
{
    /// <summary>
    /// Deserializes XML returned from a PRTG Request.
    /// </summary>
    /// <typeparam name="T">The type of objects to create from the request.</typeparam>
    [ExcludeFromCodeCoverage]
    internal class XmlDeserializer<T>
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

        internal static XmlDeserializer<T> DeserializeList(XDocument doc)
        {
            return DeserializeInternal<XmlDeserializer<T>, T>(doc);
        }

        internal static T DeserializeType(XDocument doc)
        {
            return DeserializeInternal<T, T>(doc);
        }

        internal static void UpdateType(XDocument doc, T obj)
        {
            DeserializeInternal<T, T>(doc, obj);
        }

#pragma warning disable 693
        private static T DeserializeInternal<T, TInner>(XDocument doc, object target = null)
#pragma warning restore 693
        {
            var deserializer = new XmlSerializer(typeof(T));

            var obj = target == null ? deserializer.Deserialize(doc) : deserializer.DeserializeExisting(doc, target);
            var data = (T)obj;

            return data;
        }
    }
}
