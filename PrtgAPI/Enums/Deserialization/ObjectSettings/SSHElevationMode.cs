using System.Xml.Serialization;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies how rights elevation should be performed via SSH.
    /// </summary>
    public enum SSHElevationMode
    {
        /// <summary>
        /// Run commands as the specified user.
        /// </summary>
        [XmlEnum("1")]
        RunAsUser,

        /// <summary>
        /// Run commands as another user requiring a password using 'sudo'.
        /// </summary>
        [XmlEnum("2")]
        RunAsAnotherWithPasswordViaSudo,

        /// <summary>
        /// Run commands as another user not requiring a password using sudo.
        /// </summary>
        [XmlEnum("4")] //This is not a bug
        RunAsAnotherWithoutPasswordViaSudo,

        /// <summary>
        /// Run commands as another user using 'su'.
        /// </summary>
        [XmlEnum("3")] //This is not a bug
        RunAsAnotherViaSu
    }
}
