using System;
using System.Runtime.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Represents a process running on a device.
    /// </summary>
    [DataContract]
    [Category(SystemInfoType.Processes)]
    public class DeviceProcessInfo : DeviceInfo
    {
        /// <summary>
        /// Type of system information represented by this object.
        /// </summary>
        public override DeviceInfoType Type => DeviceInfoType.Process;

        #region ProcessId

        /// <summary>
        /// Process ID of the process.
        /// </summary>
        [DataMember(Name = nameof(SysInfoProperty.ProcessId))]
        public int ProcessId { get; set; }

        #endregion
        #region Caption

        private string captionStr;

        [DataMember(Name = nameof(SysInfoProperty.Caption))]
        private string CaptionStr
        {
            get { return captionStr; }
            set { SetString(value, ref captionStr); }
        }

        /// <summary>
        /// Caption of the process.
        /// </summary>
        public string Caption => captionStr;

        #endregion
        #region CreationDate

        private string creationDateStr;
        private DateTime? creationdate;

        [DataMember(Name = nameof(SysInfoProperty.CreationDate))]
        private string CreationDateStr
        {
            get { return creationDateStr; }
            set { SetDateColon(value, ref creationDateStr, ref creationdate); }
        }

        /// <summary>
        /// DateTime the process was created.
        /// </summary>
        public DateTime? CreationDate => creationdate;

        #endregion

        /// <summary>
        /// Name of the process.
        /// </summary>
        public override string Name => DisplayName;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Name} (PID: {ProcessId})";
        }
    }
}
