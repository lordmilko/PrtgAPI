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
        Username,

        /// <summary>
        /// Password to authenticate against PRTG.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Password,

        /// <summary>
        /// PassHash to authenticate against PRTG. Alternative to using <see cref="Password"/>
        /// </summary>
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
        /// Maximum number of records returned from a PRTG Request. Maximum: 50000
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Count,

        /// <summary>
        /// Record number to start with. Used in conjunction with <see cref="Count"/> to request data page by page.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Start,

        /// <summary>
        /// A <see cref="RecordAge"/> value limiting records returned to those within this time period.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        [Description("filter_drel")]
        FilterRecordAge,

        /// <summary>
        /// One or more <see cref="SensorStatus"/> values used to retrieve sensors in specified states.
        /// </summary>
        [ParameterType(ParameterType.MultiParameter)]
        [Description("filter_status")]
        FilterStatus,

        /// <summary>
        /// One or more string values limiting objects returned to only those with these tag attributes.
        /// </summary>
        [Description("filter_tags")] //todo: this needs a parametertype!
        FilterTags,

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
        SortBy,

        /// <summary>
        /// A <see cref="CustomNumberFormat"/> specifying how values should be displayed.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Show,

        //Object Manipulation

        /// <summary>
        /// ID of the object to operate upon.
        /// </summary>
        [ParameterType(ParameterType.SingleValue)]
        Id,

        /// <summary>
        /// The <see cref="Property"/> of <see cref="Id"/> to operate upon.
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
        /// Allows using custom parameters not known to PrtgAPI, including parameters whose names are dynamically generated. For use with <see cref="CustomParameter"/> 
        /// </summary>
        [ParameterType(ParameterType.MultiParameter)]
        Custom,

        //Undocumented

        [Undocumented]
        [ParameterType(ParameterType.SingleValue)]
        [Description("_hjax")]
        Hjax,

        [Undocumented]
        [ParameterType(ParameterType.SingleValue)]
        Channel,

        [Undocumented]
        [ParameterType(ParameterType.SingleValue)]
        ObjectType
    }
}
