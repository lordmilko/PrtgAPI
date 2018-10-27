using System.Runtime.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a user that is logged into a device.
    /// </summary>
    [DataContract]
    [Category(SystemInfoType.Users)]
    public class DeviceUserInfo : DeviceInfo
    {
        /// <summary>
        /// Type of system information represented by this object.
        /// </summary>
        public override DeviceInfoType Type => DeviceInfoType.User;

        #region Domain

        private string domainStr;

        [DataMember(Name = nameof(SysInfoProperty.Domain))]
        private string DomainStr
        {
            get { return domainStr; }
            set { SetString(value, ref domainStr); }
        }

        /// <summary>
        /// Name of the domain the user belongs to.
        /// </summary>
        public string Domain => domainStr;

        #endregion
        #region User

        private string userStr;

        [DataMember(Name = nameof(SysInfoProperty.User))]
        private string UserStr
        {
            get { return userStr; }
            set { SetString(value, ref userStr); }
        }

        /// <summary>
        /// Name of the user.
        /// </summary>
        public string User => userStr;

        #endregion

        /// <summary>
        /// Full name of the user.
        /// </summary>
        public override string Name => DisplayName;
    }
}
