using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
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
        public int Id { get; set; }

        /// <summary>
        /// Name of this object.
        /// </summary>
        [XmlElement("name")]
        [PropertyParameter(nameof(Property.Name))]
        public string Name { get; set; }

        // ################################## All Objects ##################################

        
    }
}
