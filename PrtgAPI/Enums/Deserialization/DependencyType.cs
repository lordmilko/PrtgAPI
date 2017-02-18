using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies the object this object is dependent on. When the dependent object goes down this object will be automatically paused.
    /// </summary>
    public enum DependencyType
    {
        /// <summary>
        /// The object is dpendent on its parent (e.g. a sensor is dependent upon its device).
        /// </summary>
        [XmlEnum("0")]
        Parent,

        /// <summary>
        /// The object is dependent on an arbitrary PRTG object.
        /// </summary>
        [XmlEnum("1")]
        Object,

        /// <summary>
        /// Indicates that the object is the "master" object of its parent. When the master object goes down, the parent (and any objects it is the <see cref="Parent"/> of) goes down. Objects should only have one master object.
        /// </summary>
        [XmlEnum("2")]
        MasterObject
    }
}
