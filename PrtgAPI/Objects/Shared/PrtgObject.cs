using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Objects.Shared
{
    /// <summary>
    /// Properties that apply to all PRTG objects.
    /// </summary>
    public class PrtgObject
    {
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
            set { comments = value.Trim(); }
        }
    }
}
