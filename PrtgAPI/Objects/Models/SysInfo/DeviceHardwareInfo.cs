using System.Runtime.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Describes a hardware component found on a device.
    /// </summary>
    [DataContract]
    [Category(SystemInfoType.Hardware)]
    public class DeviceHardwareInfo : DeviceInfo
    {
        /// <summary>
        /// Type of system information represented by this object.
        /// </summary>
        public override DeviceInfoType Type => DeviceInfoType.Hardware;

        #region Name

        private string nameStr;

        [DataMember(Name = nameof(SysInfoProperty.Name))]
        private string NameStr
        {
            get { return nameStr; }
            set { SetString(value, ref nameStr); }
        }

        /// <summary>
        /// Name of the component.
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
        /// Description of the component.
        /// </summary>
        public string Description => descriptionStr;

        #endregion
        #region Class

        private string classStr;

        [DataMember(Name = nameof(SysInfoProperty.Class))]
        private string ClassStr
        {
            get { return classStr; }
            set { SetString(value, ref classStr); }
        }

        /// <summary>
        /// Hardware category of the component.
        /// </summary>
        public string Class => classStr;

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
        /// Caption of the component.
        /// </summary>
        public string Caption => captionStr;

        #endregion
        #region State

        private string stateStr;

        [DataMember(Name = nameof(SysInfoProperty.State))]
        private string StateStr
        {
            get { return stateStr; }
            set { SetString(CleanState(value), ref stateStr); }
        }

        /// <summary>
        /// Health state of the component.
        /// </summary>
        public string State => stateStr;

        #endregion
        #region SerialNumber

        private string serialNumberStr;

        [DataMember(Name = nameof(SysInfoProperty.SerialNumber))]
        private string SerialNumberStr
        {
            get { return serialNumberStr; }
            set { SetString(value, ref serialNumberStr); }
        }

        /// <summary>
        /// Serial number of the component.
        /// </summary>
        public string SerialNumber => serialNumberStr;

        #endregion
        #region Capacity

        private string capacityStr;
        private long? capacity;

        [DataMember(Name = nameof(SysInfoProperty.Capacity))]
        private string CapacityStr
        {
            get { return capacityStr; }
            set { SetLong(value, ref capacityStr, ref capacity); }
        }

        /// <summary>
        /// Capacity of the component. For example, total RAM capacity. Typically measured in bytes.
        /// </summary>
        public long? Capacity => capacity;

        #endregion
    }
}
