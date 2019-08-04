using System.Collections.Generic;
using System.Xml.Serialization;

namespace PrtgAPI.NotificationActions
{
    /// <summary>
    /// Setings that apply to Program Notification Actions.
    /// </summary>
    [XmlRoot(NotificationAction.CategoryProgram)]
    public class NotificationActionProgramSettings : BaseNotificationActionSettings
    {
        /// <summary>
        /// The name of the EXE/Script file to execute.
        /// </summary>
        [XmlElement("injected_address")]
        public string ExeFile { get; set; }

        /// <summary>
        /// Parameters to pass to the <see cref="ExeFile"/>.
        /// </summary>
        [XmlElement("injected_message")]
        public string Parameters { get; set; }

        /// <summary>
        /// The domain or local hostname used to run the <see cref="ExeFile"/> in custom security context.
        /// </summary>
        [XmlElement("injected_windowslogindomain")]
        public string WindowsDomain { get; set; }

        /// <summary>
        /// The username used to run the <see cref="ExeFile"/> in custom security context.
        /// </summary>
        [XmlElement("injected_windowsloginusername")]
        public string WindowsUserName { get; set; }

        [XmlElement("injected_windowsloginpassword")]
        internal string windowsPassword { get; set; }

        /// <summary>
        /// Whether a password has been specified for running the <see cref="ExeFile"/> in a custom security context.
        /// </summary>
        public bool HasWindowsPassword => !string.IsNullOrEmpty(windowsPassword);

        /// <summary>
        /// The duration (in seconds) the program can run for before PRTG will terminate the process.
        /// </summary>
        [XmlElement("injected_timeout")]
        public int Timeout { get; set; }

        internal override void ToString(List<object> targets)
        {
            targets.Add(ExeFile);
        }
    }
}