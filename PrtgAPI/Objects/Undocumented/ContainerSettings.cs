using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Objects.Undocumented
{
    internal abstract class ContainerSettings : ObjectSettings
    {
        //Basic Settings

        /// <summary>
        /// The name of the PRTG Object.
        /// </summary>
        [XmlElement("injected_name")]
        public string Name { get; set; }

        /// <summary>
        /// Tags that have been applied to this object.
        /// </summary>
        [XmlElement("injected_tags")]
        [SplittableString(' ')]
        public string[] Tags { get; set; }

        /// <summary>
        /// Tags that are inherited from this object's parent.
        /// </summary>
        [XmlElement("injected_parenttags")]
        public string[] ParentTags { get; set; }

        /// <summary>
        /// Indicates whether this object is active. If an object is inative, all objects under it are paused.
        /// </summary>
        [XmlElement("injected_active")]
        public bool Active { get; set; }

        /// <summary>
        /// The priority of the object.
        /// </summary>
        [XmlElement("injected_priority")]
        public Priority Priority { get; set; }

        //Location

        /// <summary>
        /// Whether this object's location is inherited from its parent.
        /// </summary>
        [XmlElement("injected_locationgroup")]
        public bool InheritLocation { get; set; }

        /// <summary>
        /// The location of this object.
        /// </summary>
        [XmlElement("injected_location")]
        public string Location { get; set; }

        #region Credentials for Windows Systems

        /// <summary>
        /// Whether this object's Windows Credentials are inherited from its parent.
        /// </summary>
        [XmlElement("injected_windowsconnection")]
        public bool InheritWindowsCredentials { get; set; }

        /// <summary>
        /// The domain or local hostname used for Windows Authentication.
        /// </summary>
        [XmlElement("injected_windowslogindomain")]
        public string WindowsDomain { get; set; }

        /// <summary>
        /// The username used for Windows Authentication.
        /// </summary>
        [XmlElement("injected_windowsloginusername")]
        public string WindowsUserName { get; set; }

        //todo: we need to be able to set the windows password, but not retrieve it

        #endregion
        #region Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems

        /// <summary>
        /// Whether this object's Linux Credentials are inherited from its parent.
        /// </summary>
        [XmlElement("injected_linuxconnection")]
        public bool InheritLinuxCredentials { get; set; }

        /// <summary>
        /// The username used for Linux Authentication.
        /// </summary>
        [XmlElement("injected_linuxloginusername")]
        public string LinuxUserName { get; set; }

        //todo: we need to be able to set the linux password/private key, but not retrieve it

        /// <summary>
        /// The login/authentication mode to use for Linux Authentication.
        /// </summary>
        [XmlElement("injected_linuxloginmode")]
        public LinuxLoginMode LinuxLoginMode { get; set; }

        /// <summary>
        /// Indicates the port to use for WBEM communications. If automatic is specified, the port will be 5988 or 5989.
        /// </summary>
        [XmlElement("injected_wbemportmode")]
        public AutoMode WbemPortMode { get; set; }

        /// <summary>
        /// The custom port to use for WBEM.
        /// </summary>
        [XmlElement("injected_wbemport")]
        public string WbemPort { get; set; }

        /// <summary>
        /// The port to use for SSH communications.
        /// </summary>
        [XmlElement("injected_sshport")]
        public int SSHPort { get; set; }

        /// <summary>
        /// Specifies whether to execute commands as the specified user or elevate rights using su or sudo.
        /// </summary>
        [XmlElement("injected_sshelevatedrights")]
        public SSHElevationMode SSHElevationMode { get; set; }

        /// <summary>
        /// The user to use for SSH Elevation with su or sudo. If no user is specified, root will be used.
        /// </summary>
        [XmlElement("injected_elevationnamesu")]
        [XmlElement("injected_elevationnamesudo")]
        public string SSHElevationUser { get; set; }

        //todo: need to allow setting the elevation user password

        [XmlElement("injected_sshversion_devicegroup")]
        public SSHEngine SSHEngine { get; set; }

        #endregion
        #region Credentials for VMware/XenServer

        /// <summary>
        /// Whether this object's VMware/XenServer credentials are inherited from its parent.
        /// </summary>
        [XmlElement("injected_vmwareconnection")]
        public bool InheritVMwareCredentials { get; set; }

        /// <summary>
        /// The username to use for VMware/XenServer authentication.
        /// </summary>
        [XmlElement("injected_esxuser")]
        public string VMwareUserName { get; set; }

        //todo: need to allow setting the vmware user password

        /// <summary>
        /// The protocol to use when connecting to VMware/XenServer systems.
        /// </summary>
        [XmlElement("injected_esxprotocol")]
        public HTTPMode VMWareProtocol { get; set; }

        /// <summary>
        /// Whether to reuse sessions for multiple sensor scans.
        /// </summary>
        [XmlElement("injected_vmwaresessionpool")]
        public VMwareSessionMode SessionPool { get; set; }

        #endregion
        #region Credentials for SNMP Devices

        /// <summary>
        /// Whether this object's SNMP credentials are inherited from its parent.
        /// </summary>
        [XmlElement("injected_snmpversiongroup")]
        public bool InheritSNMPCredentials { get; set; }

        /// <summary>
        /// The version to use for SNMP.
        /// </summary>
        [XmlElement("injected_snmpversion")]
        public SNMPVersion SNMPVersion { get; set; }

        /// <summary>
        /// The community string to use. Only applies when using SNMP v1 or v2c. The default value is 'public'.
        /// </summary>
        [XmlElement("injected_snmpcommv2")]
        [XmlElement("injected_snmpcommv1")]
        public string SNMPCommunityString { get; set; }

        /// <summary>
        /// The authentication type to use for SNMPv3.
        /// </summary>
        [XmlElement("injected_snmpauthmode")]
        public HashType SNMPv3AuthType { get; set; }

        /// <summary>
        /// The username to use for SNMPv3.
        /// </summary>
        [XmlElement("injected_snmpuser")]
        public string SNMPv3UserName { get; set; }

        //todo: allow setting the snmpv3 password

        /// <summary>
        /// The encryption type to use for SNMPv3.
        /// </summary>
        [XmlElement("injected_snmpencmode")]
        public EncryptionType SNMPv3EncryptionType { get; set; }

        /// <summary>
        /// The encryption key to use for SNMPv3.
        /// </summary>
        [XmlElement("injected_snmpencpass")]
        public string SNMPv3EncryptionKey { get; set; }

        //todo: allow setting the snmpv3 encryption key

        /// <summary>
        /// The context name to use for SNMPv3. A context name is required only if specified by the target device.
        /// </summary>
        [XmlElement("injected_snmpcontext")]
        public string SNMPv3Context { get; set; }

        /// <summary>
        /// The port to use for SNMP. The default value is 161.
        /// </summary>
        [XmlElement("injected_snmpport")]
        public int SNMPPort { get; set; }

        /// <summary>
        /// The length of time (in seconds) before a SNMP request times out.
        /// </summary>
        [XmlElement("injected_snmptimeout")]
        public int SNMPTimeout { get; set; }

        #endregion
        #region Credentials for Database Management Systems

        /// <summary>
        /// Whether this object's DBMS credentials are inherited from its parent.
        /// </summary>
        [XmlElement("injected_dbcredentials")]
        public bool InheritDBCredentials { get; set; }

        /// <summary>
        /// Indicates whether PRTG automatically determines the port of the DBMS or the port is set manually.
        /// </summary>
        [XmlElement("injected_usedbcustomport")]
        public AutoMode DBPortMode { get; set; }

        /// <summary>
        /// The port to use for all database sensors. This only applies if the <see cref="DBPortMode"/> is <see cref="AutoMode.Manual"/> .
        /// </summary>
        [XmlElement("injected_dbport")]
        public int? DBPort { get; set; }

        /// <summary>
        /// The authentication mode PRTG uses to connect to a database server.
        /// </summary>
        [XmlElement("injected_dbauth")]
        public DBAuthMode DBAuthMode { get; set; }

        /// <summary>
        /// The username to use when <see cref="DBAuthMode"/> is <see cref="PrtgAPI.DBAuthMode.SQL"/> 
        /// </summary>
        [XmlElement("injected_dbuser")]
        public string DBUser { get; set; }

        /// <summary>
        /// The length of time (in seconds) before a database request times out.
        /// </summary>
        [XmlElement("injected_dbtimeout")]
        public int DBTimeout { get; set; }

        #endregion
        #region  Credentials for Amazon Cloudwatch

        /// <summary>
        /// Whether this object's Amazon Cloudwatch credentials are inherited from its parent.
        /// </summary>
        [XmlElement("injected_cloudcredentials")]
        public bool InheritAmazonCredentials { get; set; }

        /// <summary>
        /// The access key to use for AWS.
        /// </summary>
        [XmlElement("injected_awsak")]
        public string AmazonAccessKey { get; set; }

        //todo: allow setting the amazon secret key

        #endregion
        #region Windows Compatibility Options

        /// <summary>
        /// Whether this object's Windows Compatibility settings are inherited from its parent.
        /// </summary>
        [XmlElement("injected_wmicompatibility")]
        public bool InheritWindowsCompatibility { get; set; }

        /// <summary>
        /// The data source to use for performing WMI queries.
        /// </summary>
        [XmlElement("injected_wmiorpc")]
        public WmiDataSource WmiDataSource { get; set; }

        /// <summary>
        /// The method to use for determining the timeout of WMI requests.
        /// </summary>
        [XmlElement("injected_wmitimeoutmethod")]
        public WmiTimeoutMethod WmiTimeoutMethod { get; set; }

        /// <summary>
        /// The length of time (in seconds) before a WMI request times out. This only applies if <see cref="WmiTimeoutMethod"/> is <see cref="PrtgAPI.WmiTimeoutMethod.Manual"/>.
        /// </summary>
        [XmlElement("injected_wmitimeout")]
        public string WmiTimeout { get; set; }

        #endregion
        #region SNMP Compatibility Options

        /// <summary>
        /// Whether this object's SNMP Compatibility settings are inherited from its parent.
        /// </summary>
        [XmlElement("injected_snmpcompatibility")]
        public bool InheritSNMPCompatibility { get; set; }

        /// <summary>
        /// The delay (in ms) PRTG should wait between multiple SNMP requests to a single device. This value must be 0-100ms. Higher values are not supported.
        /// </summary>
        [XmlElement("injected_snmpdelay")] //todo: need to add some sort of validation to setting the snmp delay. what happens if you try and set a value higher than 100?
        public int SNMPDelay { get; set; }

        /// <summary>
        /// Whether PRTG should retry failed requests.
        /// </summary>
        [XmlElement("injected_retrysnmp")]
        public RetryMode SNMPRetryMode { get; set; }

        /// <summary>
        /// Whether PRTG treats overflow values as valid results.
        /// </summary>
        [XmlElement("injected_ignoreoverflow")]
        public SNMPOverflowMode SNMPOverflowMode { get; set; }

        /// <summary>
        /// Whether spurious "0" values should be ignored for delta (difference) sensors.
        /// </summary>
        [XmlElement("injected_ignorezero")]
        public SNMPZeroValueMode SNMPZeroValueMode { get; set; }

        /// <summary>
        /// Whether to use force using 32-bit counters even when a device reports 64-bit counters are available.
        /// </summary>
        [XmlElement("injected_force32")]
        public SNMPCounterMode SNMPCounterMode { get; set; }

        /// <summary>
        /// Whether to include a single or multiple SNMP requests in each request.
        /// </summary>
        [XmlElement("injected_usesingleget")]
        public SNMPRequestMode SNMPRequestMode { get; set; }

        /// <summary>
        /// Name template to define how the sensor name should be displayed. Template variables include [port], [ifalias], [ifname], [ifdescr], [ifspeed], [ifsensor] and custom OIDs.
        /// </summary>
        [XmlElement("injected_trafficportname")]
        public string SNMPPortNameTemplate { get; set; }

        /// <summary>
        /// Whether PRTG should automatically update SNMP port sensor names when those names change in the device.
        /// </summary>
        [XmlElement("injected_updateportname")]
        public SNMPPortNameUpdateMode SNMPPortNameUpdateMode { get; set; }

        [XmlElement("injected_portupdateoid")]
        public SNMPPortIdentification SNMPPortIdentification { get; set; }

        /// <summary>
        /// The start index for the interface range SNMP Traffic sensors can query during sensor creation. If this value is 0, PRTG will use "automatic mode"
        /// </summary>
        [XmlElement("injected_portstart")]
        public int SNMPStartInterfaceIndex { get; set; }

        /// <summary>
        /// The end index for the interface range SNMP Traffic sensors can query during sensor creation. If this value is 0, PRTG will use "automatic mode"
        /// </summary>
        [XmlElement("injected_portend")]
        public int SNMPEndInterfaceIndex { get; set; }

        #endregion

        //todo: the rest, including proxy settings for http sensors automatic monitoring data analysis
    }
}