using System.ComponentModel;

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
        [Attributes.ParameterType(ParameterType.SingleValue)]
        Username,

        /// <summary>
        /// Password to authenticate against PRTG.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        Password,

        /// <summary>
        /// PassHash to authenticate against PRTG. Alternative to using <see cref="Password"/>
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        PassHash,

        /// <summary>
        /// A <see cref="T:PrtgAPI.Content"/> value representing a PRTG object type.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        Content,

        /// <summary>
        /// One or more <see cref="T:PrtgAPI.Property"/> values specifying properties that will be retrieved for a PRTG Object.
        /// </summary>
        [Attributes.ParameterType(ParameterType.MultiValue)]
        Columns,

        /// <summary>
        /// A <see cref="T:PrtgAPI.Output"/> value specifying the format the PRTG Response will be stored in.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        Output,

        /// <summary>
        /// Maximum number of records returned from a PRTG Request. Maximum: 50000
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        Count,

        /// <summary>
        /// Record number to start with. Used in conjunction with <see cref="Count"/> to request data page by page.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        Start,

        /// <summary>
        /// A <see cref="T:PrtgAPI.RecordAge"/> value limiting records returned to those within this time period.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        [Description("filter_drel")]
        FilterRecordAge,

        /// <summary>
        /// One or more <see cref="T:PrtgAPI.SensorStatus"/> values used to retrieve sensors in specified states.
        /// </summary>
        [Attributes.ParameterType(ParameterType.MultiParameter)]
        [Description("filter_status")]
        FilterStatus,

        /// <summary>
        /// One or more string values limiting objects returned to only those with these tag attributes.
        /// </summary>
        [Description("filter_tags")]
        FilterTags,

        /// <summary>
        /// Used to filter objects returned based on the value of a specified <see cref="T:PrtgAPI.Property"/>.
        /// When submitting a PRTG Request, the name of the Property being filtered on should be added to the value of this parameter (e.g. filter_name).
        /// </summary>
        [Attributes.ParameterType(ParameterType.MultiParameter)]
        [Description("filter_")]
        FilterXyz,

        /// <summary>
        /// A <see cref="T:PrtgAPI.Property"/> used to specify which property to sort PRTG Response by.
        /// </summary>
        SortBy,

        /// <summary>
        /// A <see cref="T:PrtgAPI.CustomNumberFormat"/> specifying how values should be displayed.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        Show,

        //Object Manipulation

        /// <summary>
        /// ID of the object to operate upon.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        Id,

        /// <summary>
        /// The <see cref="Property"/> of <see cref="Id"/> to operate upon.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        Name,

        /// <summary>
        /// The value to store in the property referenced by <see cref="Name"/>.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        Value,

        //Prio,

        //Pausing/Resuming

        /// <summary>
        /// Message to display on a paused object.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        [Description("pausemsg")]
        PauseMessage,

        /// <summary>
        /// Action to perform for a PRTG Function. Meaning of value is dependent on function being excuted. Simulate an error: 1 (on simulate.htm). Pause indefinitely: 0 (on pause.htm). Resume monitoring of an object: 1 (on pause.htm)
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        Action,

        /// <summary>
        /// Duration to pause an object for, in minutes.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        [Description("duration")]
        PauseDuration,

        //Error Handling

        /// <summary>
        /// Message to display on an acknowledged sensor.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        [Description("ackmsg")]
        AcknowledgeMessage,



        //Reordering

        /// <summary>
        /// The new position of the object.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        [Description("newpos")]
        NewPos,
        /*
        //Report Related

        AddId,

        //Deletion

        Approve,

        //Addition

        TargetId,
        Host,

        //Location

        Location,
        LonLat
        */

        //Cloning

        /// <summary>
        /// The hostname of the new object.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        [Description("host")]
        Host,

        /// <summary>
        /// The object targeted by the API call.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        [Description("targetid")]
        TargetId,

        //Channel

        /// <summary>
        /// The ID of the Channel.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        [Description("subid")]
        SubId,

        /// <summary>
        /// The type of object to change properties for.
        /// </summary>
        [Attributes.ParameterType(ParameterType.SingleValue)]
        [Description("subtype")]
        SubType,
    }
}
