using PrtgAPI.Attributes;
using PrtgAPI.Request.Serialization.FilterHandlers;
using PrtgAPI.Request.Serialization.ValueConverters;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace PrtgAPI
{
    /// <summary>
    /// <para type="description">Specifies properties that can be retrieved for and filtered by on PRTG Objects.</para>
    /// </summary>
    public enum Property
    {
        /// <summary>
        /// Unique identifier of the object.
        /// </summary>
        [Description("objid")]
        Id,

        /// <summary>
        /// <see cref="ObjectType"/> of the object. When filtering for sensors, specific sensor types must be specified.
        /// </summary>
        [FilterHandler(typeof(TypeFilterHandler))]
        Type,

        /// <summary>
        /// Name of the object or the object an <see cref="IEventObject"/> applies to.
        /// </summary>
        [FilterHandler(typeof(StringFilterHandler))]
        Name,

        /// <summary>
        /// Tags defined on the object or inherited from all parent objects.
        /// </summary>
        [FilterHandler(typeof(StringFilterHandler))]
        Tags,

        /// <summary>
        /// Whether an object is currently active or has been <see cref="PrtgAPI.Status.Paused"/>.
        /// </summary>
        [XmlBool(false)]
        Active,

        /// <summary>
        /// Percentage of time an object has been in a down state.
        /// </summary>
        [ValueConverter(typeof(UpDownTimeConverter))]
        Downtime,

        /// <summary>
        /// Total amount of time an object has been in a down state.
        /// </summary>
        [Description("downtimetime")]
        [ValueConverter(typeof(TimeSpanConverter))]
        TotalDowntime,

        /// <summary>
        /// Total amount of time an object has been down since it was last up.
        /// </summary>
        [Description("downtimesince")]
        [ValueConverter(typeof(TimeSpanConverter))]
        DownDuration,

        /// <summary>
        /// Percentage of time an object has been in an up state.
        /// </summary>
        [ValueConverter(typeof(UpDownTimeConverter))]
        Uptime,

        /// <summary>
        /// Total amount of time an object has been in an up state.
        /// </summary>
        [Description("uptimetime")]
        [ValueConverter(typeof(TimeSpanConverter))]
        TotalUptime,

        /// <summary>
        /// Total amount of time an object has been up since it was last down.
        /// </summary>
        [Description("uptimesince")]
        [ValueConverter(typeof(TimeSpanConverter))]
        UpDuration,

        /// <summary>
        /// Total amount of time an object has been monitored.
        /// </summary>
        [Description("knowntime")]
        [ValueConverter(typeof(TimeSpanConverter))]
        TotalMonitorTime,

        /// <summary>
        /// Datetime when object first received data.
        /// </summary>
        [Description("cumsince")]
        [ValueConverter(typeof(DateTimeConverter))]
        DataCollectedSince,

        /// <summary>
        /// Name of the sensor.
        /// </summary>
        Sensor,

        /// <summary>
        /// Monitoring interval of an object.
        /// </summary>
        [Description("intervalx")]
        [ValueConverter(typeof(ZeroPaddingConverter))]
        Interval,

        /// <summary>
        /// Date/time object last refreshed results.
        /// </summary>
        [ValueConverter(typeof(DateTimeConverter))]
        LastCheck,

        /// <summary>
        /// Date/time object was last in an up status.
        /// </summary>
        [ValueConverter(typeof(DateTimeConverter))]
        LastUp,

        /// <summary>
        /// Date/time object was last in a down status.
        /// </summary>
        [ValueConverter(typeof(DateTimeConverter))]
        LastDown,

        /// <summary>
        /// Name of the parent device.
        /// </summary>
        [FilterHandler(typeof(StringFilterHandler))]
        Device,

        /// <summary>
        /// Name of the parent group.
        /// </summary>
        [FilterHandler(typeof(StringFilterHandler))]
        Group,

        /// <summary>
        /// Name of the parent probe.
        /// </summary>
        [FilterHandler(typeof(StringFilterHandler))]
        Probe,

        /// <summary>
        /// Name of parent device and group seperated by slash.
        /// </summary>
        GrpDev,

        /// <summary>
        /// Number of each trigger type defined for an object.
        /// </summary>
        [Description("notifiesx")]
        [FilterHandler(typeof(NotificationTypesHandler))]
        NotificationTypes,

        /// <summary>
        /// Access rights for the current user on an object.
        /// </summary>
        Access,

        /// <summary>
        /// Displays the name the object this object is dependent on.
        /// </summary>
        [FilterHandler(typeof(StringFilterHandler))]
        Dependency,

        /// <summary>
        /// <see cref="PrtgAPI.Status"/> or <see cref="LogStatus"/> of the object.
        /// </summary>
        Status,

        /// <summary>
        /// Message (such as "OK") displayed on an object.
        /// </summary>
        [FilterHandler(typeof(StringFilterHandler))]
        Message,

        /// <summary>
        /// Priority of the object.
        /// </summary>
        Priority,

        /// <summary>
        /// Last value of a sensor's primary channel.
        /// </summary>
        [ValueConverter(typeof(LastValueConverter))]
        LastValue,

        /// <summary>
        /// Number of sensors currently in an <see cref="PrtgAPI.Status.Up"/> state.
        /// </summary>
        [Description("upsens")]
        [ValueConverter(typeof(ZeroPaddingConverter))]
        UpSensors,

        /// <summary>
        /// Number of sensors currently in a <see cref="PrtgAPI.Status.Down"/> state.
        /// </summary>
        [Description("downsens")]
        [ValueConverter(typeof(ZeroPaddingConverter))]
        DownSensors,

        /// <summary>
        /// Number of sensors currently in a <see cref="PrtgAPI.Status.DownAcknowledged"/> state.
        /// </summary>
        [Description("downacksens")]
        [ValueConverter(typeof(ZeroPaddingConverter))]
        DownAcknowledgedSensors,

        /// <summary>
        /// Number of sensors currently in a <see cref="PrtgAPI.Status.DownPartial"/> state.
        /// </summary>
        [Description("partialdownsens")]
        [ValueConverter(typeof(ZeroPaddingConverter))]
        PartialDownSensors,

        /// <summary>
        /// Number of sensors currently in a <see cref="PrtgAPI.Status.Warning"/> state.
        /// </summary>
        [Description("warnsens")]
        [ValueConverter(typeof(ZeroPaddingConverter))]
        WarningSensors,

        /// <summary>
        /// Number of sensors currently in any <see cref="PrtgAPI.Status.Paused"/> state.
        /// </summary>
        [Description("pausedsens")]
        [ValueConverter(typeof(ZeroPaddingConverter))]
        PausedSensors,

        /// <summary>
        /// Number of sensors currently in a <see cref="PrtgAPI.Status.Unusual"/> state.
        /// </summary>
        [Description("unusualsens")]
        [ValueConverter(typeof(ZeroPaddingConverter))]
        UnusualSensors,

        /// <summary>
        /// Number of sensors currently in a <see cref="PrtgAPI.Status.Unknown"/> state.
        /// </summary>
        [Description("undefinedsens")]
        [ValueConverter(typeof(ZeroPaddingConverter))]
        UnknownSensors,

        /// <summary>
        /// Number of sensors present under an object (including all child objects).
        /// </summary>
        [Description("totalsens")]
        [ValueConverter(typeof(ZeroPaddingConverter))]
        TotalSensors,

        /// <summary>
        /// Value of an object.
        /// Used in: Values, TopData
        /// </summary>
        [Description("value_")]
        Value,

        /// <summary>
        /// Displays the sensor coverage of a time span in a value table.
        /// Used in: Values
        /// </summary>
        Coverage,

        /// <summary>
        /// Displays an exclamation mark when the sensor tree object is marked as favorite.<para/>
        /// Note: when filtering by this property, objects will not have a Favorite of False unless they were previously set to True.
        /// </summary>
        [XmlBool]
        [FilterHandler(typeof(FavoriteFilterHandler))]
        Favorite,

        /// <summary>
        /// Displays the user responsible for a history entry or the user (or user group) a ticket is assigned to.
        /// Used in: History, Tickets, TicketData
        /// </summary>
        [Description("user")]
        UserName,

        /// <summary>
        /// Name of the parent object of the associated object of a log message.
        /// Used in: Messages
        /// </summary>
        Parent,

        //todo: what is topidx?------------------------------------------------------------------------------
        /// <summary>
        /// Timestamp or timespan of an object (for tickets: last modification).
        /// Used in: Messages, Tickets, TicketData, Values, History, StoredReports, TopIDX
        /// </summary>
        DateTime,

        /// <summary>
        /// Like 'datetime' but only the date part.
        /// Used in: Messages, Tickets, History, Values
        /// </summary>
        DateOnly,

        /// <summary>
        /// Like 'datetime' but only the date part.
        /// Used in: Messages, Tickets, History, Values
        /// </summary>
        TimeOnly,

        /// <summary>
        /// For sensor tree objects this displays the name of an associated schedule, for reports this displays the report generation schedule.
        /// Used in: Sensors, Devices, Groups, Probes, Reports
        /// </summary>
        [FilterHandler(typeof(StringFilterHandler))]
        Schedule,

        /// <summary>
        /// Displays the period of a report (day, week etc.).
        /// Used in: Reports
        /// </summary>
        Period,

        /// <summary>
        /// Displays the email address of a report.
        /// Used in: Reports
        /// </summary>
        Email,

        /// <summary>
        /// Displays the template used by a report.
        /// Used in: Reports
        /// </summary>
        Template,

        /// <summary>
        /// Timestamp of the last generation of a report.
        /// Used in: Reports
        /// </summary>
        LastRun,

        /// <summary>
        /// Timestamp of the next generation of a report.
        /// Used in: Reports
        /// </summary>
        NextRun,

        /// <summary>
        /// Size of a stored report.
        /// Used in: StoredReports
        /// </summary>
        Size,

        /// <summary>
        /// Numerical data for minigraphs. Numbers are 5 minute averages for the last 24 hours (must be scaled to the maximum of the series). There are two datasets: "|" separates measured value series and error series.
        /// Used in: Sensors
        /// </summary>
        [FilterHandler(typeof(StringFilterHandler))]
        MiniGraph,

        /// <summary>
        /// Device Icon.
        /// Used in: Devices
        /// </summary>
        DeviceIcon,

        /// <summary>
        /// Object comments (for tickets: related object).
        /// Used in: All Objects
        /// </summary>
        [FilterHandler(typeof(StringFilterHandler))]
        Comments,

        /// <summary>
        /// Hostname or IP address.
        /// Used in: Devices
        /// </summary>
        [FilterHandler(typeof(StringFilterHandler))]
        Host,

        /// <summary>
        /// Probe status for probes, auto discovery status for groups.
        /// Used in: Groups, Probes
        /// </summary>
        [FilterHandler(typeof(StringOrNumericFilterHandler))]
        Condition,

        /// <summary>
        /// Probe connection status. Equivalent to <see cref="Condition"/>.
        /// </summary>
        [Description("condition")]
        ProbeStatus,

        /// <summary>
        /// Object type (string).
        /// Used in: All Tree Objects
        /// </summary>
        [FilterHandler(typeof(StringFilterHandler))]
        BaseType,

        /// <summary>
        /// URL of the object.
        /// Used in: All Tree Objects
        /// </summary>
        [Description("baselink")]
        [ValueConverter(typeof(UrlConverter))]
        Url,

        /// <summary>
        /// URL of the device icon.
        /// Used in: Devices
        /// </summary>
        Icon,

        /// <summary>
        /// ID of the parent object or ID of a ticket (e.g. the device of sensor, the probe of a group, etc).
        /// Used in: All Tree Objects, Tickets
        /// </summary>
        ParentId,

        /// <summary>
        /// Location property (used in Geo Maps).
        /// Used in: Groups, Devices
        /// </summary>
        [FilterHandler(typeof(StringFilterHandler))]
        Location,

        /// <summary>
        /// Subobjects are folded up (true) or down (false); tickets: user (or user group) to which ticket is assinged read it since last change.
        /// Used in: Groups, Probes, Tickets
        /// </summary>
        [XmlBool(false)]
        [Description("fold")]
        Collapsed,

        /// <summary>
        /// Number of groups in a probe/group node.
        /// Used in: Groups, Probes
        /// </summary>
        [Description("groupnum")]
        [ValueConverter(typeof(ZeroPaddingConverter))]
        TotalGroups,

        /// <summary>
        /// Number of devices in a probe/group node.
        /// Used in: Groups, Probes
        /// </summary>
        [Description("devicenum")]
        [ValueConverter(typeof(ZeroPaddingConverter))]
        TotalDevices,

        /// <summary>
        /// Type of the ticket: user, notification, todo.
        /// Used in: Tickets
        /// </summary>
        TicketType,

        /// <summary>
        /// User who edited the ticket most recently.
        /// Used in: Tickets, TicketData
        /// </summary>
        ModifiedBy,

        /// <summary>
        /// Types of all ticket edits.
        /// Used in: TicketData
        /// </summary>
        Actions,

        /// <summary>
        /// The text of the ticket that was added with the last edit, or the properties of a notification trigger.
        /// Used in: TicketData, Triggers
        /// </summary>
        Content,

        /// <summary>
        /// Position of the object in PRTG Tables. Internally, this value is represented as position = pos * 10
        /// </summary>
        [ValueConverter(typeof(PositionConverter))]
        Position,

        /// <summary>
        /// Start date to retrieve records from. This is the point in time furthest away from now.
        /// Used in: Messages (Logs)
        /// </summary>
        [Description("dstart")]
        StartDate,

        /// <summary>
        /// End date to retrieve records to. This is the point in time closest to now.
        /// Used in: Messages (Logs)
        /// </summary>
        [Description("dend")]
        EndDate,

        /// <summary>
        /// Age of records to retrieve.
        /// Used in: Messages (Logs)
        /// </summary>
        [Description("drel")]
        RecordAge,
    }
}
