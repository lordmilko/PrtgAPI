using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;

namespace PrtgAPI.Request.Serialization
{
    /// <summary>
    /// Represents an object contained in a table data response.
    /// </summary>
    /// <typeparam name="TObject">The type of object contained in the response.</typeparam>
    [ExcludeFromCodeCoverage]
    internal class TableData<TObject>
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

    }
}
