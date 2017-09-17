using System.ComponentModel;
using PrtgAPI.Attributes;
using PrtgAPI.Objects.Undocumented;

namespace PrtgAPI
{
    internal enum ObjectPropertyInternal
    {
        LonLat,
    }

    /// <summary>
    /// <para type="description">Specifies the settings of objects that can be interfaced with using the PRTG HTTP API.</para>
    /// </summary>
    public enum ObjectProperty
    {
    #region Containers

        #region Basic Settings

        /// <summary>
        /// Indicates whether this object is active. If an object is inative, all objects under it are paused.<para/>
        /// Corresponds to Basic Settings -> Status.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        Active,

        #endregion
        #region Credentials for Windows Systems

        /// <summary>
        /// Whether this object's Windows Credentials are inherited from its parent.<para/>
        /// Corresponds to Credentials for Windows Systems -> Inherit Windows Credentials.
        /// </summary>
        [LiteralValue]
        [TypeLookup(typeof(ContainerSettings))]
        InheritWindowsCredentials,

        /// <summary>
        /// The domain or local hostname used for Windows Authentication.<para/>
        /// Corresponds to Credentials for Windows Systems -> Domain or Computer Name.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritWindowsCredentials), false)]
        WindowsDomain,

        /// <summary>
        /// The username used for Windows Authentication.<para/>
        /// Corresponds to Credentials for Windows Systems -> User.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritWindowsCredentials), false)]
        WindowsUserName,

        //[TypeLookup(typeof(ContainerSettings))]
        //[DependentProperty(nameof(InheritWindowsCredentials), false)]
        //WindowsPassword,

        #endregion
        #region Location

        /// <summary>
        /// Whether this object's location is inherited from its parent.<para/>
        /// Corresponds to Location -> Inherit Location.
        /// </summary>
        [LiteralValue]
        [TypeLookup(typeof(ContainerSettings))]
        InheritLocation,

        /// <summary>
        /// The location of this object.<para/>
        /// Corresponds to Location -> Location (for Geo Maps).
        /// </summary>
        [Type(typeof(Location))]
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritLocation), false)]
        [SecondaryProperty(nameof(ObjectPropertyInternal.LonLat))]
        Location,

        #endregion
        #region Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems

        /// <summary>
        /// Whether this object's Linux Credentials are inherited from its parent.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> Inherit Linux Credentials.
        /// </summary>
        [LiteralValue]
        [TypeLookup(typeof(ContainerSettings))]
        InheritLinuxCredentials,

        /// <summary>
        /// The username used for Linux Authentication.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> User.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritLinuxCredentials), false)]
        LinuxUserName,

        /// <summary>
        /// The login/authentication mode to use for Linux Authentication.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> Login.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritLinuxCredentials), false)]
        LinuxLoginMode,

        /// <summary>
        /// The protocol that is used to communicate with WBEM.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> For WBEM Use Protocol.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritLinuxCredentials), false)]
        WbemProtocolMode,

        /// <summary>
        /// Indicates the port to use for WBEM communications. If automatic is specified, the port will be 5988 or 5989.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> For WBEM Use Port.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritLinuxCredentials), false)]
        WbemPortMode,

        /// <summary>
        /// The custom port to use for WBEM.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> WBEM Port.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(WbemPortMode), AutoMode.Manual, true)]
        WbemPort,

        /// <summary>
        /// The port to use for SSH communications.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> SSH Port.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritLinuxCredentials), false)]
        SSHPort,

        /// <summary>
        /// Specifies whether to execute commands as the specified user or elevate rights using su or sudo.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> SSH Rights Elevation.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritLinuxCredentials), false)]
        SSHElevationMode,

        //SSHElevationUser

        /// <summary>
        /// The engine to use for SSH Requests.<para/>
        /// Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> SSH Engine.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritLinuxCredentials), false)]
        SSHEngine,

        #endregion
        #region Credentials for VMware/XenServer

        /// <summary>
        /// Whether this object's VMware/XenServer credentials are inherited from its parent.<para/>
        /// Corresponds to Credentials for VMware/XenServer -> Inherit VMware Credentials.
        /// </summary>
        [LiteralValue]
        [TypeLookup(typeof(ContainerSettings))]
        InheritVMwareCredentials,

        /// <summary>
        /// The username to use for VMware/XenServer authentication.<para/>
        /// Corresponds to Credentials for VMware/XenServer -> User.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritVMwareCredentials), false)]
        VMwareUserName,

        //VMwarePassword

        /// <summary>
        /// The protocol to use when connecting to VMware/XenServer systems.<para/>
        /// Corresponds to Credentials for VMware/XenServer -> VMware Protocol.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritVMwareCredentials), false)]
        VMwareProtocol,

        /// <summary>
        /// Whether to reuse sessions for multiple sensor scans.<para/>
        /// Corresponds to Credentials for VMware/XenServer -> Session Pool.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritVMwareCredentials), false)]
        VMwareSessionMode,

        #endregion
        #region Credentials for SNMP Devices

        /// <summary>
        /// Whether this object's SNMP credentials are inherited from its parent.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Inherit SNMP Credentials.
        /// </summary>
        [LiteralValue]
        [TypeLookup(typeof(ContainerSettings))]
        InheritSNMPCredentials,

        /// <summary>
        /// The version to use for SNMP.<para/>
        /// Corresponds to Credentials for SNMP Devices -> SNMP Version.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCredentials), false)]
        SNMPVersion,

        /// <summary>
        /// The community string to use when using SNMP v1. The default value is 'public'.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Community String (SNMP v1).
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCredentials), false)]
        SNMPCommunityStringV1,

        /// <summary>
        /// The community string to use when using SNMP v2c. The default value is 'public'.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Community String (SNMP v2c).
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCredentials), false)]
        SNMPCommunityStringV2,

        /// <summary>
        /// The authentication type to use for SNMPv3.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Authentication Type.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCredentials), false)]
        SNMPv3AuthType,

        /// <summary>
        /// The username to use for SNMPv3.<para/>
        /// Corresponds to Credentials for SNMP Devices -> User.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCredentials), false)]
        SNMPv3UserName,

        /// <summary>
        /// The encryption type to use for SNMPv3.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Encryption Type.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCredentials), false)]
        SNMPv3EncryptionType,

        /// <summary>
        /// The context name to use for SNMPv3. A context name is required only if specified by the target device.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Context Name.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCredentials), false)]
        SNMPv3Context,

        /// <summary>
        /// The port to use for SNMP. The default value is 161.<para/>
        /// Corresponds to Credentials for SNMP Devices -> SNMP Port.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCredentials), false)]
        SNMPPort,

        /// <summary>
        /// The length of time (in seconds) before a SNMP request times out.<para/>
        /// Corresponds to Credentials for SNMP Devices -> SNMP Timeout.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCredentials), false)]
        SNMPTimeout,

        #endregion
        #region Credentials for Database Management Systems

        /// <summary>
        /// Whether this object's DBMS credentials are inherited from its parent.<para/>
        /// Corresponds to Credentials for Database Management Systems -> Inherit Database Credentials.
        /// </summary>
        [LiteralValue]
        [TypeLookup(typeof(ContainerSettings))]
        InheritDBCredentials,

        /// <summary>
        /// Indicates whether PRTG automatically determines the port of the DBMS or the port is set manually.<para/>
        /// Corresponds to Credentials for Database Management Systems -> Port for Databases.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritDBCredentials), false)]
        DBPortMode,

        /// <summary>
        /// The port to use for all database sensors. This only applies if the <see cref="DBPortMode"/> is <see cref="AutoMode.Manual"/>.<para/>
        /// Corresponds to Credentials for Database Management Systems -> Custom Database Port.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(DBPortMode), AutoMode.Manual, true)]
        DBPort,

        /// <summary>
        /// The authentication mode PRTG uses to connect to a database server.<para/>
        /// Corresponds to Credentials for Database Management Systems -> Authentication Mode.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritDBCredentials), false)]
        DBAuthMode,

        /// <summary>
        /// The username to use when <see cref="DBAuthMode"/> is <see cref="PrtgAPI.DBAuthMode.SQL"/>.<para/>
        /// Corresponds to Credentials for Database Management Systems -> User.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(DBAuthMode), PrtgAPI.DBAuthMode.SQL, true)]
        DBUserName,

        /// <summary>
        /// The length of time (in seconds) before a database request times out.<para/>
        /// Corresponds to Credentials for Database Management Systems -> Timeout.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritDBCredentials), false)]
        DBTimeout,

        #endregion
        #region  Credentials for Amazon Cloudwatch

        /// <summary>
        /// Whether this object's Amazon Cloudwatch credentials are inherited from its parent.<para/>
        /// Corresponds to Credentials for Amazon Cloudwatch -> Inherit Amazon Credentials.
        /// </summary>
        [LiteralValue]
        [TypeLookup(typeof(ContainerSettings))]
        InheritAmazonCredentials,

        /// <summary>
        /// The access key to use for Amazon Web Services.<para/>
        /// Corresponds to Credentials for Amazon Cloudwatch -> Access Key.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritAmazonCredentials), false)]
        AmazonAccessKey,

        #endregion
        #region Windows Compatibility Options

        /// <summary>
        /// Whether this object's Windows Compatibility settings are inherited from its parent.<para/>
        /// Corresponds to Windows Compatibility Options -> Inherit Windows Compatibility Options.
        /// </summary>
        [LiteralValue]
        [TypeLookup(typeof(ContainerSettings))]
        InheritWindowsCompatibility,

        /// <summary>
        /// The data source to use for performing WMI queries.<para/>
        /// Corresponds to Windows Compatibility Options -> Preferred Data Source.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritWindowsCompatibility), false)]
        WmiDataSource,

        /// <summary>
        /// The method to use for determining the timeout of WMI requests.<para/>
        /// Corresponds to Windows Compatibility Options -> Timeout Method.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritWindowsCompatibility), false)]
        WmiTimeoutMethod,

        /// <summary>
        /// The length of time (in seconds) before a WMI request times out. This only applies if <see cref="WmiTimeoutMethod"/> is <see cref="PrtgAPI.WmiTimeoutMethod.Manual"/>.<para/>
        /// Corresponds to Windows Compatibility Options -> Timeout Value.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(WmiTimeoutMethod), PrtgAPI.WmiTimeoutMethod.Manual, true)]
        WmiTimeout,

        #endregion
        #region SNMP Compatibility Options

        /// <summary>
        /// Whether this object's SNMP Compatibility settings are inherited from its parent.<para/>
        /// Corresponds to SNMP Compatibility Options -> Inherit SNMP Compatibility Options.
        /// </summary>
        [LiteralValue]
        [TypeLookup(typeof(ContainerSettings))]
        InheritSNMPCompatibility,

        /// <summary>
        /// The delay (in ms) PRTG should wait between multiple SNMP requests to a single device. This value must be 0-100ms. Higher values are not supported.<para/>
        /// Corresponds to SNMP Compatibility Options -> SNMP Delay.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCompatibility), false)]
        SNMPDelay,

        /// <summary>
        /// Whether PRTG should retry failed requests.<para/>
        /// Corresponds to SNMP Compatibility Options -> Failed Requests.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCompatibility), false)]
        SNMPRetryMode,

        /// <summary>
        /// Whether PRTG treats overflow values as valid results.<para/>
        /// Corresponds to SNMP Compatibility Options -> Overflow Values.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCompatibility), false)]
        SNMPOverflowMode,

        /// <summary>
        /// Whether spurious "0" values should be ignored for delta (difference) sensors.<para/>
        /// Corresponds to SNMP Compatibility Options -> Zero Values.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCompatibility), false)]
        SNMPZeroValueMode,

        /// <summary>
        /// Whether to use force using 32-bit counters even when a device reports 64-bit counters are available.<para/>
        /// Corresponds to SNMP Compatibility Options -> 32-bit/64-bit Counters.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCompatibility), false)]
        SNMPCounterMode,

        /// <summary>
        /// Whether to include a single or multiple SNMP requests in each request.<para/>
        /// Corresponds to SNMP Compatibility Options -> Request Mode.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCompatibility), false)]
        SNMPRequestMode,

        /// <summary>
        /// Name template to define how the sensor name should be displayed. Template variables include [port], [ifalias], [ifname], [ifdescr], [ifspeed], [ifsensor] and custom OIDs.<para/>
        /// Corresponds to SNMP Compatibility Options -> Port Name Template.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCompatibility), false)]
        SNMPPortNameTemplate,

        /// <summary>
        /// Whether PRTG should automatically update SNMP port sensor names when those names change in the device.<para/>
        /// Corresponds to SNMP Compatibility Options -> Port Name Update.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCompatibility), false)]
        SNMPPortNameUpdateMode,

        /// <summary>
        /// Specifies the field to use to identify SNMP interfaces in the event the interface port order changes on reboot.<para/>
        /// Corresponds to SNMP Compatibility Options -> Port Identification.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCompatibility), false)]
        SNMPPortIdMode,

        /// <summary>
        /// The start index for the interface range SNMP Traffic sensors can query during sensor creation. If this value is 0, PRTG will use "automatic mode".<para/>
        /// Corresponds to SNMP Compatibility Options -> Start Interface Index.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCompatibility), false)]
        SNMPInterfaceStartIndex,

        /// <summary>
        /// The end index for the interface range SNMP Traffic sensors can query during sensor creation. If this value is 0, PRTG will use "automatic mode".<para/>
        /// Corresponds to SNMP Compatibility Options -> End Interface Index.
        /// </summary>
        [TypeLookup(typeof(ContainerSettings))]
        [DependentProperty(nameof(InheritSNMPCompatibility), false)]
        SNMPInterfaceEndIndex,

        #endregion
    #endregion
    #region Sensors
        #region Access Rights

        /// <summary>
        /// Whether to inherit Access Rights settings from the parent object.<para/>
        /// Corresponds to Access Rights -> Inherit Access Rights.
        /// </summary>
        [LiteralValue]
        [TypeLookup(typeof(SensorSettings))]
        InheritAccess,

        #endregion
        #region Basic Sensor Settings

        /// <summary>
        /// The name of the PRTG Object.<para/>
        /// Corresponds to Basic Settings -> Name.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        Name,

        //[TypeLookup(typeof(SensorSettings))]
        //ParentTags,

        /// <summary>
        /// Tags that have been applied to this object.<para/>
        /// Corresponds to Basic Settings -> Tags.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        Tags,

        /// <summary>
        /// The priority of the object.<para/>
        /// Corresponds to Basic Settings -> Priority.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        Priority,

        #endregion
        #region Debug Options

        /// <summary>
        /// How raw sensor results should be stored for debugging purposes. Corresponds to:<para/>
        ///     Debug Options (WMI) -> Sensor Result<para/>
        ///     Sensor Settings (EXE/XML) -> EXE Result
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        DebugMode,

        #endregion
        #region HTTP Specific

        /// <summary>
        /// The URL to monitor. If a protocol is not specified, HTTP is used.<para/>
        /// Corresponds to HTTP Specific -> URL.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        Url,

        /// <summary>
        /// The HTTP Request Method to use for requesting the <see cref="Url"/>.<para/>
        /// Corresponds to HTTP Specific -> Request Method.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        HttpRequestMethod,

        /// <summary>
        /// Whether the Server Name Indication is inherited from the parent device, or derived from the specified <see cref="Url"/>.<para/>
        /// Corresponds to HTTP Specific -> SNI Inheritance.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        UseSNIFromUrl,

        #endregion
        #region Ping Settings

        /// <summary>
        /// The duration (in seconds) this sensor can run for before timing out. Based on the sensor type, the maximum valid value may be different. Corresponds to:<para/>
        ///     Ping Settings                 -> Timeout (Max 300)<para/>
        ///     WMI Remote Ping Configuration -> Timeout (Max 300)<para/>
        ///     HTTP Specific                 -> Timeout (Max 900)
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        Timeout,

        /// <summary>
        /// The packet size to use for Ping Requests. The default value is 32 bytes. This value must be between 1-10,000 bytes.<para/>
        /// Corresponds to Ping Settings -> Packet Size.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        PingPacketSize,

        /// <summary>
        /// Whether to send a single Ping or Multiple Pings. If <see cref="PrtgAPI.PingMode.MultiPing"/> is used, all packets must be most for the sensor to go <see cref="Status.Down"/>.<para/>
        /// Corresponds to Ping Settings -> Ping Method.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        PingMode,

        /// <summary>
        /// The number of Ping Requests to make, when using <see cref="PrtgAPI.PingMode.MultiPing"/>.<para/>
        /// Corresponds to Ping Settings -> Ping Count.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        PingCount,

        /// <summary>
        /// The delay between each Ping Request, when using <see cref="PrtgAPI.PingMode.MultiPing"/>.<para/>
        /// Corresponds to Ping Settings -> Ping Delay.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        PingDelay,

        /// <summary>
        /// Whether this sensor should auto acknowledge on entering a <see cref="Status.Down"/> state.<para/>
        /// Corresponds to Ping Settings -> Auto Acknowledge.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        AutoAcknowledge,

        #endregion
        #region Scanning Interval

        /// <summary>
        /// Whether to inherit this object's scanning interval settings from its parent.
        /// </summary>
        [LiteralValue]
        [TypeLookup(typeof(SensorSettings))]
        InheritInterval,

        /// <summary>
        /// The <see cref="PrtgAPI.ScanningInterval"/> with which an object refreshes its data.
        /// </summary>
        [Description("interval")]
        [TypeLookup(typeof(SensorSettings))]
        [DependentProperty(nameof(InheritInterval), false)]
        Interval,

        /// <summary>
        /// The <see cref="PrtgAPI.IntervalErrorMode"/> indicating the number of scanning intervals to wait before setting a sensor to <see cref="Status.Down"/> when an error is reported.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        [DependentProperty(nameof(InheritInterval), false)]
        IntervalErrorMode,

        #endregion
        #region Schedules, Dependencies and Maintenance Window

        //todo: make a note that if you set this WITHOUT setting another property, it does nothing. as such, maybe
        //we need to make this a dependent property. note that it would work on devices, just not sensors
        //todo: also note this on sensorsettings and whatever base class we end up using

        [LiteralValue]
        [TypeLookup(typeof(SensorSettings))]
        InheritDependency,

        /// <summary>
        /// Whether a one-time maintenance window has been defined.<para/>
        /// Corresponds to Schedules, Dependencies and Maintenance Window -> Maintenance Window.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        MaintenanceEnabled,

        #endregion
        #region Sensor Display

        //PrimaryChannel,

        [TypeLookup(typeof(SensorSettings))]
        GraphType,

        //StackUnit,

        #endregion
        #region Sensor Settings (EXE/XML)

        /// <summary>
        /// Parameters that will be passed to the specified EXE/Script file.<para/>
        /// Corresponds to Sensor Settings -> Parameters.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        ExeParameters,

        /// <summary>
        /// Whether PRTG Environment Variables (%host, %windowsusername, etc) will be available as System Environment Variables inside the EXE/Script.<para/>
        /// Corresponds to Sensor Settings -> Environment
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        SetExeEnvironmentVariables,

        /// <summary>
        /// Whether to use the Windows Credentials of the parent device to execute the specified EXE/Script file. If custom credentials are not used, the file will be executed under the credentials of the PRTG Probe Service.<para/>
        /// Corresponds to Sensor Settings -> Security Context.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        UseWindowsAuthentication,

        /// <summary>
        /// The mutex name to use. All sensors with the same mutex name will be executed sequentially, reducing resource utilization.<para/>
        /// Corresponds to Sensor Settings -> Mutex Name.
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        Mutex,

        [TypeLookup(typeof(SensorSettings))]
        EnableChangeTriggers,

        #endregion
        #region WMI Alternative Query

        [TypeLookup(typeof(SensorSettings))]
        WmiMode,

        #endregion
        #region WMI Remote Ping Configuration

        /// <summary>
        /// The DNS name or IP Address to target. Applies to: WMI Remote Ping
        /// </summary>
        [TypeLookup(typeof(SensorSettings))]
        Target,

        [TypeLookup(typeof(SensorSettings))]
        PingRemotePacketSize,

        #endregion
    #endregion
    }
}
