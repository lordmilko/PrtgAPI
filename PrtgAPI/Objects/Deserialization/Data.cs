using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using PrtgAPI.Helpers;

namespace PrtgAPI
{
    /// <summary>
    /// Deserializes XML returned from a PRTG Request.
    /// </summary>
    /// <typeparam name="T">The type of objects to create from the request.</typeparam>
    public class Data<T>
    {
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
        /// Initializes a new instance of the <see cref="T:PrtgAPI.Data`1"/> class.
        /// </summary>
        /// <param name="doc">XML returned from a PRTG Server Request.</param>
        /// <returns></returns>
        public static Data<T> Deserialize(XDocument doc)
        {
            var deserializer = new XmlSerializer(typeof(Data<T>), new XmlRootAttribute(doc.Root.Name.ToString()));
            var obj = deserializer.Deserialize(doc.ToStream());
            var data = (Data<T>)obj;

            return data;
        }
    }
}
