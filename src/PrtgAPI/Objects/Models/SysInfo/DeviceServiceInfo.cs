using System.Runtime.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a service installed on a device.
    /// </summary>
    [DataContract]
    [Category(SystemInfoType.Services)]
    public class DeviceServiceInfo : DeviceInfo
    {
        /// <summary>
        /// Type of system information represented by this object.
        /// </summary>
        public override DeviceInfoType Type => DeviceInfoType.Service;

        #region Name

        private string nameStr;

        [DataMember(Name = nameof(SysInfoProperty.Name))]
        private string NameStr
        {
            get { return nameStr; }
            set { SetString(value, ref nameStr); }
        }

        /// <summary>
        /// Name of the service.
        /// </summary>
        public override string Name => nameStr;

        #endregion
        #region Description

        private string descriptionStr;

        [DataMember(Name = nameof(SysInfoProperty.Description))]
        private string DescriptionStr
        {
            get { return descriptionStr; }
            set { SetString(value, ref descriptionStr); }
        }

        /// <summary>
        /// Description of the service.
        /// </summary>
        public string Description => descriptionStr;

        #endregion
        #region User

        private string userStr;

        [DataMember(Name = nameof(SysInfoProperty.StartName))]
        private string UserStr
        {
            get { return userStr; }
            set { SetString(value, ref userStr); }
        }

        /// <summary>
        /// User account the service runs as.
        /// </summary>
        public string User => userStr;

        #endregion
        #region StartMode

        private string startModeStr;

        [DataMember(Name = nameof(SysInfoProperty.StartMode))]
        private string StartModeStr
        {
            get { return startModeStr; }
            set { SetString(value, ref startModeStr); }
        }

        /// <summary>
        /// Startup mode of the service.
        /// </summary>
        public string StartMode => startModeStr;

        #endregion
        #region State

        private string stateStr;

        [DataMember(Name = nameof(SysInfoProperty.State))]
        private string StateStrStr
        {
            get { return descriptionStr; }
            set { SetString(CleanState(value), ref stateStr); }
        }

        /// <summary>
        /// Running state of the service.
        /// </summary>
        public string State => stateStr;

        #endregion
    }
}
