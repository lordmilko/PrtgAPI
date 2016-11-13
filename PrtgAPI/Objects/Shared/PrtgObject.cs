using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Objects.Shared
{
    /// <summary>
    /// Properties that apply to all PRTG objects.
    /// </summary>
    public class PrtgObject
    {
        // ################################## All Object Tables ##################################

        //prtg's documentation says these belong under ObjectTable, however i believe they may belong under PrtgObject

        /// <summary>
        /// ID number used to uniquely identify this object within PRTG.
        /// </summary>
        [XmlElement("objid")]
        [PropertyParameter(nameof(Property.ObjId))]
        [PSVisible(true)]
        public int? Id { get; set; }

        /// <summary>
        /// Name of this object.
        /// </summary>
        [XmlElement("name")]
        [PropertyParameter(nameof(Property.Name))]
        [PSVisible(true)]
        public string Name { get; set; }

        // ################################## All Objects ##################################

        private string comments;

        /// <summary>
        /// Comments present on this object.
        /// </summary>
        [XmlElement("comments")]
        [PropertyParameter(nameof(Property.Comments))]
        [PSVisible(true)]
        public string Comments
        {
            get { return comments;}
            set { comments = string.IsNullOrWhiteSpace(value) ? null : value.Trim(); }
        }
    }
}
