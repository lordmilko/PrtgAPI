using System.ComponentModel;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters;

namespace PrtgAPI
{
    /// <summary>
    /// Specifies fields to be included in a PRTG API Request.
    /// </summary>
    public enum Parameter
    {
        //Live Data

        /// <summary>
        /// Username to authenticate against PRTG. If PRTG has been configured to use Active Directory, this can be the username of a Domain User.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        UserName,

        /// <summary>
        /// Password to authenticate against PRTG.
        /// </summary>
        [MutuallyExclusive(nameof(PassHash))]
        [ParameterType(ParameterType.SingleValue)]
        Password,

        /// <summary>
        /// PassHash to authenticate against PRTG. Alternative to using <see cref="Password"/>
        /// </summary>
        [MutuallyExclusive(nameof(Password))]
        [ParameterType(ParameterType.SingleValue)]
        PassHash,

        /// <summary>
        /// A <see cref="PrtgAPI.Content"/> value representing a PRTG object type.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Content,

        /// <summary>
        /// One or more <see cref="Property"/> values specifying properties that will be retrieved for a PRTG Object.
        /// </summary>
        [ParameterType(ParameterType.MultiValue)]
        Columns,

        /// <summary>
        /// A <see cref="Output"/> value specifying the format the PRTG Response will be stored in.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Output,

        /// <summary>
        /// Maximum number of records returned from a PRTG Request.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Count,

        /// <summary>
        /// Record number to start with. Used in conjunction with <see cref="Count"/> to request data page by page.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Start,

        /// <summary>
        /// Used to filter objects returned based on the value of a specified <see cref="Property"/>.
        /// When submitting a PRTG Request, the name of the Property being filtered on should be added to the value of this parameter (e.g. filter_name).
        /// </summary>
        [ParameterType(ParameterType.MultiParameter)]
        [Description("filter_")]
        FilterXyz,

        /// <summary>
        /// A <see cref="Property"/> used to specify which property to sort PRTG Response by.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        SortBy,

        /// <summary>
        /// A <see cref="CustomValueFormat"/> specifying how values should be displayed.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Show,

        //Object Manipulation

        /// <summary>
        /// ID of the object to operate upon.
        /// </summary>
        [ParameterType(ParameterType.MultiValue)]
        Id,

        /// <summary>
        /// The <see cref="Property.Name"/> to use for <see cref="Id"/> or to operate upon.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Name,

        /// <summary>
        /// The value to store in the property referenced by <see cref="Name"/>.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Value,

        //Prio,

        //Pausing/Resuming

        /// <summary>
        /// Message to display on a paused object.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        [Description("pausemsg")]
        PauseMessage,

        /// <summary>
        /// Action to perform for a PRTG Function. Meaning of value is dependent on function being excuted. Simulate an error: 1 (on simulate.htm). Pause indefinitely: 0 (on pause.htm). Resume monitoring of an object: 1 (on pause.htm)
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Action,

        /// <summary>
        /// Duration to pause or acknowledge an object for, in minutes.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        [Description("duration")]
        Duration,

        //Error Handling

        /// <summary>
        /// Message to display on an acknowledged sensor.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        [Description("ackmsg")]
        AcknowledgeMessage,

        //Reordering

        /// <summary>
        /// The new position of the object.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        [Description("newpos")]
        NewPos,

        /*
        //Report Related

        AddId,

        //Location

        Location,
        LonLat
        */

        //Cloning

        /// <summary>
        /// The hostname of the new object.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        [Description("host")]
        Host,

        /// <summary>
        /// The object targeted by the API call.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        [Description("targetid")]
        TargetId,

        //Channel

        /// <summary>
        /// The ID of the object.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        [Description("subid")]
        SubId,

        /// <summary>
        /// The type of object to change properties for.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        [Description("subtype")]
        SubType,

        /// <summary>
        /// Whether or not approval is granted to perform an action.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Approve,

        /// <summary>
        /// The start date to retrieve data for.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        [Description("sdate")]
        StartDate,

        /// <summary>
        /// The end date to retrieve data for.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        [Description("edate")]
        EndDate,

        /// <summary>
        /// The value to average with.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        [Description("avg")]
        Average,

        /// <summary>
        /// Allows using custom parameters not known to PrtgAPI, including parameters whose names are dynamically generated. For use with <see cref="CustomParameter"/> 
        /// </summary>
        [ParameterType(ParameterType.MultiParameter)]
        Custom,

        //Undocumented

        /// <summary>
        /// ID of the channel to retrieve properties for.
        /// </summary>
        [Undocumented]
        [ParameterType(ParameterType.SingleValue)]
        Channel,

        /// <summary>
        /// The type of object to retrieve details for.
        /// </summary>
        [Undocumented]
        [ParameterType(ParameterType.SingleValue)]
        ObjectType,

        /// <summary>
        /// The type of sensor to create.
        /// </summary>
        [Undocumented]
        [ParameterType(ParameterType.SingleValue)]
        SensorType,

        /// <summary>
        /// The templates to use for performing an operation.
        /// </summary>
        [Undocumented]
        [ParameterType(ParameterType.MultiValue)]
        Template,

        /// <summary>
        /// Whether to collapse or show an item.
        /// </summary>
        [Undocumented]
        [ParameterType(ParameterType.SingleValue)]
        Fold,

        /// <summary>
        /// Object category request should apply to.
        /// </summary>
        [Undocumented]
        [ParameterType(ParameterType.SingleValue)]
        Category,

        /// <summary>
        /// Object kind request should apply to.
        /// </summary>
        [Undocumented]
        [ParameterType(ParameterType.SingleValue)]
        Kind,

        /// <summary>
        /// The details of a WMI Service to create.
        /// </summary>
        [Undocumented]
        [LengthLimit(30)]
        [ParameterType(ParameterType.MultiParameter)]
        [Description("service__check")]
        Service,

        /// <summary>
        /// The templates to use for performing an auto-discovery on a device.
        /// </summary>
        [Undocumented]
        [ParameterType(ParameterType.MultiParameter)]
        [Description("devicetemplate__check")]
        DeviceTemplate,

        /// <summary>
        /// Temp ID that identifies the session being used to create a new sensor.
        /// </summary>
        [Undocumented]
        [ParameterType(ParameterType.SingleValue)]
        [Description("tmpid")]
        TmpId
    }
}
