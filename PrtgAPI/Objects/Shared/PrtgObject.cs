using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Objects.Shared
{
    /// <summary>
    /// <para type="description">Base class for all PRTG objects.</para>
    /// </summary>
    public class PrtgObject
    {
        // ################################## All Object Tables ##################################

        //prtg's documentation says these belong under ObjectTable, however i believe they may belong under PrtgObject

        /// <summary>
        /// ID number used to uniquely identify this object within PRTG.
        /// </summary>
        [XmlElement("objid")]
        [PropertyParameter(nameof(Property.Id))]
        public int Id { get; set; }

        /// <summary>
        /// Name of this object.
        /// </summary>
        [XmlElement("name")]
        [PropertyParameter(nameof(Property.Name))]
        public string Name { get; set; }

        // ################################## All Objects ##################################

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
