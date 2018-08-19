using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PrtgAPI.Request.Serialization
{
    /// <summary>
    /// Deserializes XML returned from a PRTG Request.
    /// </summary>
    /// <typeparam name="TObject">The type of objects to create from the request.</typeparam>
    [ExcludeFromCodeCoverage]
    internal class XmlDeserializer<TObject>
    {
        /// <summary>
        /// Total number of objects returned by the request.
        /// </summary>
        [XmlAttribute("totalcount")]
        public int TotalCount { get; set; }

        /// <summary>
        /// Version of PRTG running on the server.
        /// </summary>
        [XmlElement("prtg-version")]
        public string Version { get; set; }

        /// <summary>
        /// List of objects of type <typeparamref name="TObject"/> returned from the request.
        /// </summary>
        [XmlElement("item")]
        public List<TObject> Items { get; set; }

        internal static XmlDeserializer<TObject> DeserializeList(XDocument doc, bool deserializeAll = true)
        {
            return DeserializeInternal<XmlDeserializer<TObject>>(doc, null, deserializeAll);
        }

        internal static TObject DeserializeType(XDocument doc)
        {
            return DeserializeInternal<TObject>(doc, null, true);
        }

        internal static void UpdateType(XDocument doc, object target)
        {
            var deserializer = new XmlSerializer(target.GetType());

            deserializer.DeserializeExisting(doc, target);
        }

        private static T DeserializeInternal<T>(XDocument doc, object target, bool deserializeAll)
        {
            var deserializer = new XmlSerializer(typeof(T));

            var obj = target == null ? deserializer.Deserialize(doc, null, deserializeAll) : deserializer.DeserializeExisting(doc, target);
            var data = (T)obj;

            return data;
        }
    }
}
