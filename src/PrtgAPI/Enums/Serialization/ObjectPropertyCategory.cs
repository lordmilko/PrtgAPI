namespace PrtgAPI
{
    /// <summary>
    /// Specifies the settings category an <see cref="ObjectProperty"/> belongs to.
    /// </summary>
    enum ObjectPropertyCategory
    {
        #region Containers

        /// <summary>
        /// Properties found under the Basic Settings section of an object.
        /// </summary>
        BasicSettings,

        /// <summary>
        /// Properties found under the Location section of an object.
        /// </summary>
        Location,

        /// <summary>
        /// Properties found under the Credentials for Windows Systems section of an object.
        /// </summary>
        CredentialsForWindows,

        /// <summary>
        /// Properties found under the Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems section of an object.
        /// </summary>
        CredentialsForLinux,

        /// <summary>
        /// Properties found under the Credentials for VMware/XenServer section of an object.
        /// </summary>
        CredentialsForVMware,

        /// <summary>
        /// Properties found under the Credentials for SNMP Devices section of an object.
        /// </summary>
        CredentialsForSNMP,

        /// <summary>
        /// Properties found under the Credentials for Database Management Systems section of an object.
        /// </summary>
        CredentialsForDatabases,

        /// <summary>
        /// Properties found under the Credentials for Amazon Cloudwatch section of an object.
        /// </summary>
        CredentialsForAmazon,

        /// <summary>
        /// Properties found under the Windows Compatibility Options section of an object.
        /// </summary>
        WindowsCompatibility,

        /// <summary>
        /// Properties found under the SNMP Compatibility Options section of an object.
        /// </summary>
        SNMPCompatibility,

        /// <summary>
        /// Properties found under the Channel Unit Configuration section of an object.
        /// </summary>
        ChannelUnit,

        #endregion
        #region Sensors

        /// <summary>
        /// Properties found under the Access Rights section of an object.
        /// </summary>
        AccessRights,

        /// <summary>
        /// Properties found under the Basic Sensor Settings section of an object.
        /// </summary>
        BasicSensorSettings,

        /// <summary>
        /// Properties found under the Debug Options section of an object.
        /// </summary>
        DebugOptions,

        /// <summary>
        /// Properties found under the HTTP Specific section of an object.
        /// </summary>
        HttpSpecific,

        /// <summary>
        /// Properties found under the Proxy Settings for HTTP Sensors section of an object.
        /// </summary>
        ProxySettingsForHttp,

        /// <summary>
        /// Properties found under the Ping Settings section of an object.
        /// </summary>
        PingSettings,

        /// <summary>
        /// Properties found under the Scanning Interval section of an object.
        /// </summary>
        ScanningInterval,

        /// <summary>
        /// Properties found under the Schedules, Dependencies and Maintenance Window section of an object.
        /// </summary>
        SchedulesDependenciesAndMaintenance,

        /// <summary>
        /// Properties found under the Sensor Display section of an object.
        /// </summary>
        SensorDisplay,

        /// <summary>
        /// Properties found under the Sensor Settings (EXE/XML) section of an object.
        /// </summary>
        SensorSettingsExeXml,

        /// <summary>
        /// Properties found under the WMI Alternative Query section of an object.
        /// </summary>
        WmiAlternativeQuery,

        /// <summary>
        /// Properties found under the WMI Remote Ping Configuration section of an object.
        /// </summary>
        WmiRemotePing,

        /// <summary>
        /// Properties found under the Sensor Factory Specific Settings section of an object.
        /// </summary>
        SensorFactorySpecific,

        /// <summary>
        /// Properties found under the WMI Service Monitor section of an object.
        /// </summary>
        WmiServiceMonitor,

        /// <summary>
        /// Properties found under the Database Specific section of an object.
        /// </summary>
        DatabaseSpecific,

        /// <summary>
        /// Properties found under the Data section of an object.
        /// </summary>
        Data,

        #endregion

        /// <summary>
        /// Properties found under the Devices section of an object.
        /// </summary>
        Devices,

        /// <summary>
        /// Special properties not visible in the Settings page of the PRTG UI.
        /// </summary>
        Special
    }
}
