using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the Access Rights applied to a PRTG Object.
    /// </summary>
    public enum Access
    {
        /// <summary>
        /// This object's access is inherited from its parent.
        /// </summary>
        [XmlEnumAlternateName("-1")] //what if we make this an XmlOptionalEnumAttribute or something, and then our sensorsettingsobject saves to a string, and then we handle the deserializing to Access
        [XmlEnum("Inherited")]
        Inherited,
        
        /// <summary>
        /// The object is not displayed. Logs, tickets and alarms pertaining to the object are not visible.
        /// </summary>
        [XmlEnumAlternateName("0")]
        [XmlEnum("None")]
        None,

        /// <summary>
        /// The object can be viewed but not edited.
        /// </summary>
        [XmlEnumAlternateName("100")]
        [XmlEnum("Read")]
        Read,

        //todo: have a _raw value on sensorsettings that int the _setter_ also assigns the backing property of the real property to an access enum. maybe rename the xmlenum attribute
        //to prevent issues with the normal deserializer

        /// <summary>
        /// The object can be viewed, edited and deleted.
        /// </summary>
        [XmlEnumAlternateName("200")]
        [XmlEnum("Write")]
        Write,

        /// <summary>
        /// The object can be viewed, edited and deleted. In addition, Access Rights can be modified.
        /// </summary>
        [XmlEnumAlternateName("400")]
        [XmlEnum("Full")]
        Full,

        /// <summary>
        /// All options are available.
        /// </summary>
        Admin
    }
}
