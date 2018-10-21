using System.ComponentModel;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies properties that can be retrieved on System Information Objects.
    /// </summary>
    enum SysInfoProperty
    {
        [Description("_receivetime")]
        ReceiveTime,

        /// <summary>
        /// Object display name. Applies to: all <see cref="DeviceInfoType"/> types.
        /// </summary>
        [Description("_displayname")]
        DisplayName,

        /// <summary>
        /// Short object name. Applies to: <see cref="DeviceInfoType.Hardware"/>, <see cref="DeviceInfoType.Service"/>,
        /// <see cref="DeviceInfoType.Software"/>.
        /// </summary>
        [Description("_name")]
        Name,

        /// <summary>
        /// Object description. Applies to: <see cref="DeviceInfoType.Hardware"/>, <see cref="DeviceInfoType.Service"/>.
        /// </summary>
        [Description("_description")]
        Description,

        /// <summary>
        /// Object serial number. Applies to: <see cref="DeviceInfoType.Hardware"/>.
        /// </summary>
        [Description("_serialnumber")]
        SerialNumber,

        /// <summary>
        /// Object capacity. Applies to: <see cref="DeviceInfoType.Hardware"/>.
        /// </summary>
        [Description("_capacity")]
        Capacity,

        /// <summary>
        /// Object class. Applies to: <see cref="DeviceInfoType.Hardware"/>.
        /// </summary>
        [Description("_class")]
        Class,

        /// <summary>
        /// Object caption. Applies to: <see cref="DeviceInfoType.Hardware"/>, <see cref="DeviceInfoType.Process"/>.
        /// </summary>
        [Description("_caption")]
        Caption,

        /// <summary>
        /// Object state. Applies to: <see cref="DeviceInfoType.Hardware"/>, <see cref="DeviceInfoType.Service"/>.
        /// </summary>
        [Description("_state")]
        State,

        /// <summary>
        /// user domain. Applies to: <see cref="DeviceInfoType.User"/>.
        /// </summary>
        [Description("_domain")]
        Domain,

        /// <summary>
        /// Username. Applies to: <see cref="DeviceInfoType.User"/>.
        /// </summary>
        [Description("_user")]
        User,

        /// <summary>
        /// Process ID. Applies to: <see cref="DeviceInfoType.Process"/>.
        /// </summary>
        [Description("_processid")]
        ProcessId,

        [Description("_creationdate")]
        CreationDate,

        //services
        [Description("_startname")]
        StartName,

        //services
        [Description("_startmode")]
        StartMode,

        //software
        [Description("_vendor")]
        Vendor,

        //software
        [Description("_version")]
        Version,

        /// <summary>
        /// Property name. Applies to: <see cref="DeviceInfoType.System"/>.
        /// </summary>
        [Description("_key")]
        Key,

        //system
        [Description("_id")]
        Id,

        //system
        [Description("_adapter")]
        Adapter,

        /// <summary>
        /// Property value. Applies to: <see cref="DeviceInfoType.System"/>.
        /// </summary>
        [Description("_value")]
        Value,

        //software
        [Description("_date")]
        Date,

        //software
        [Description("_size")]
        Size,
    }
}
