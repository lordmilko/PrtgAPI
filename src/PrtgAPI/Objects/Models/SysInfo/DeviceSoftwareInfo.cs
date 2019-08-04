using System;
using System.Runtime.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Represents an application installed on a device.
    /// </summary>
    [DataContract]
    [Category(SystemInfoType.Software)]
    public class DeviceSoftwareInfo : DeviceInfo
    {
        /// <summary>
        /// Type of system information represented by this object.
        /// </summary>
        public override DeviceInfoType Type => DeviceInfoType.Software;

        #region Name

        private string nameStr;

        [DataMember(Name = nameof(SysInfoProperty.Name))]
        private string NameStr
        {
            get { return nameStr; }
            set { SetString(value, ref nameStr); }
        }

        /// <summary>
        /// Name of the application.
        /// </summary>
        public override string Name => nameStr;

        #endregion
        #region Vendor

        private string vendorStr;

        [DataMember(Name = nameof(SysInfoProperty.Vendor))]
        private string VendorStr
        {
            get { return vendorStr; }
            set { SetString(value, ref vendorStr); }
        }

        /// <summary>
        /// Vendor that authored the application.
        /// </summary>
        public string Vendor => vendorStr;

        #endregion
        #region Version

        private string versionStr;
        private Version version;

        [DataMember(Name = nameof(SysInfoProperty.Version))]
        private string VersionStr
        {
            get { return versionStr; }
            set { SetVersion(value, ref versionStr, ref version); }
        }

        /// <summary>
        /// Version of the software that is installed.
        /// </summary>
        public Version Version => version;

        #endregion
        #region InstallDate

        private string installDateStr;
        private DateTime? installDate;

        [DataMember(Name = nameof(SysInfoProperty.Date))]
        private string InstallDateStr
        {
            get { return installDateStr; }
            set { SetDateDash(value, ref installDateStr, ref installDate); }
        }

        /// <summary>
        /// Date the software was installed.
        /// </summary>
        public DateTime? InstallDate => installDate;

        #endregion
        #region Size

        private string sizeStr;
        int? size;

        [DataMember(Name = nameof(SysInfoProperty.Size))]
        private string SizeStr
        {
            get { return sizeStr; }
            set { SetInt(value, ref sizeStr, ref size); }
        }

        /// <summary>
        /// Installed size of the application (in kilobytes).
        /// </summary>
        public int? Size => size;

        #endregion        
    }
}
