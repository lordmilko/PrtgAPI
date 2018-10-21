using System;
using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI
{
    /// <summary>
    /// Settings that are found within container objects in PRTG, including Devices, Groups and Probes.
    /// </summary>
    public class ContainerSettings : TableSettings
    {
        internal ContainerSettings()
        {
        }

        #region Basic Settings

        /// <summary>
        /// The name of the PRTG Object.<para/>
        /// Corresponds to Basic Settings -> Name.
        /// </summary>
        [XmlElement("injected_name")]
        public string Name { get; set; }

        /// <summary>
        /// Tags that have been applied to this object.<para/>
        /// Corresponds to Basic Settings -> Tags.
        /// </summary>
        [XmlElement("injected_tags")]
        [SplittableString(' ')]
        public string[] Tags { get; set; }

        /// <summary>
        /// Indicates whether this object is active. If an object is inative, all objects under it are paused.<para/>
        /// Corresponds to Basic Settings -> Status.
        /// </summary>
        [XmlElement("injected_active")]
        public bool Active { get; set; }

        /// <summary>
        /// The priority of the object.<para/>
        /// Corresponds to Basic Settings -> Priority.
        /// </summary>
        [XmlElement("injected_priority")]
        public Priority? Priority { get; set; }

        #endregion
        #region Location

        /// <summary>
        /// Whether this object's location is inherited from its parent.<para/>
        /// Corresponds to Location -> Inherit Location.
        /// </summary>
        [XmlElement("injected_locationgroup")]
        public bool? InheritLocation { get; set; }

        /// <summary>
        /// The location of this object.<para/>
        /// Corresponds to Location -> Location (for Geo Maps).
        /// </summary>
        [XmlElement("injected_location")]
        public string Location { get; set; }

        [XmlElement("injected_lonlat")]
        internal string coordinates { get; set; }

        /// <summary>
        /// The latitudinal and longitudinal coordinates of this object's <see cref="Location"/>.
        /// </summary>
        public Coordinates Coordinates
        {
            get
            {
                if (string.IsNullOrEmpty(coordinates))
                    return null;

                var coords = coordinates.Split(',');

                if (coords.Length != 2)
                    return null;

                var lon = Convert.ToDouble(coords[0]);
                var lat = Convert.ToDouble(coords[1]);

                if (lon == 0 && lat == 0)
                    return null;

                return new Coordinates
                {
                    Latitude = lat,
                    Longitude = lon
                };
            }
        }

        #endregion
        #region Credentials for Windows Systems

        /// <summary>
        /// Whether this object's Windows Credentials are inherited from its parent.<para/>
        /// Corresponds to Credentials for Windows Systems -> Inherit Windows Credentials.
        /// </summary>
        [XmlElement("injected_windowsconnection")]
        public bool? InheritWindowsCredentials { get; set; }

        /// <summary>
        /// The domain or local hostname used for Windows Authentication.<para/>
        /// Corresponds to Credentials for Windows Systems -> Domain or Computer Name.
        /// </summary>
        [XmlElement("injected_windowslogindomain")]
        public string WindowsDomain { get; set; }

        /// <summary>
        /// The username used for Windows Authentication.<para/>
        /// Corresponds to Credentials for Windows Systems -> User.
        /// </summary>
        [XmlElement("injected_windowsloginusername")]
        public string WindowsUserName { get; set; }

        /// <summary>
        /// The hidden Windows Password of this object.
        /// </summary>
        [XmlElement("injected_windowsloginpassword")]
        internal string windowsPassword { get; set; }

        /// <summary>
        /// Whether a Windows Password is set.<para/>
        /// Corresponds to Credentials for Windows Systems -> Password.
        /// </summary>
        public bool HasWindowsPassword => !string.IsNullOrEmpty(windowsPassword);

        #endregion
        #region Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems

        /// <summary>
        /// Whether this object's Linux Credentials are inherited from its parent.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> Inherit Linux Credentials.
        /// </summary>
        [XmlElement("injected_linuxconnection")]
        public bool? InheritLinuxCredentials { get; set; }

        /// <summary>
        /// The username used for Linux Authentication.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> User.
        /// </summary>
        [XmlElement("injected_linuxloginusername")]
        public string LinuxUserName { get; set; }

        /// <summary>
        /// The login/authentication mode to use for Linux Authentication.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> Login.
        /// </summary>
        [XmlElement("injected_linuxloginmode")]
        public LinuxLoginMode LinuxLoginMode { get; set; }

        /// <summary>
        /// The hidden Linux Password of this object.
        /// </summary>
        [XmlElement("injected_linuxloginpassword")]
        internal string linuxPassword { get; set; }

        /// <summary>
        /// Whether a Linux Password is set.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> Password.
        /// </summary>
        public bool HasLinuxPassword => !string.IsNullOrEmpty(linuxPassword);

        /// <summary>
        /// The hidden Linux Private Key of this object.
        /// </summary>
        [XmlElement("injected_privatekey")]
        internal string linuxPrivateKey { get; set; }

        /// <summary>
        /// Whether a Linux Private Key is set.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> Private Key.
        /// </summary>
        public bool HasLinuxPrivateKey => !string.IsNullOrEmpty(linuxPrivateKey);

        /// <summary>
        /// The protocol that is used to communicate with WBEM.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> For WBEM Use Protocol.
        /// </summary>
        [XmlElement("injected_wbemprotocol")]
        [TypeLookup(typeof(XmlEnumAlternateName))]
        public HttpMode WbemProtocolMode { get; set; }

        /// <summary>
        /// Indicates the port to use for WBEM communications. If automatic is specified, the port will be 5988 or 5989.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> For WBEM Use Port.
        /// </summary>
        [XmlElement("injected_wbemportmode")]
        public AutoMode WbemPortMode { get; set; }

        /// <summary>
        /// The custom port to use for WBEM.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> WBEM Port.
        /// </summary>
        [XmlElement("injected_wbemport")]
        public string WbemPort { get; set; }

        /// <summary>
        /// The port to use for SSH communications.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> SSH Port.
        /// </summary>
        [XmlElement("injected_sshport")]
        public int SSHPort { get; set; }

        /// <summary>
        /// Specifies whether to execute commands as the specified user or elevate rights using su or sudo.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> SSH Rights Elevation.
        /// </summary>
        [XmlElement("injected_sshelevatedrights")]
        public SSHElevationMode SSHElevationMode { get; set; }

        /// <summary>
        /// The user to use for SSH Elevation with su. If no user is specified, root will be used.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> Target User.
        /// </summary>
        [XmlElement("injected_elevationnamesu")]
        public string SSHElevationSuUser { get; set; }

        /// <summary>
        /// The user to use for SSH Elevation with sudo. If no user is specified, root will be used.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> Target User.
        /// </summary>
        [XmlElement("injected_elevationnamesudo")]
        public string SSHElevationSudoUser { get; set; }

        /// <summary>
        /// The hidden SSH Elevation Password of this object.
        /// </summary>
        [XmlElement("injected_elevationpass")]
        internal string sshElevationPassword { get; set; }

        /// <summary>
        /// Whether a SSH Elevation Password is set.<para/>
        /// Corresponds to Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> SSH Rights Elevation -> Password.
        /// </summary>
        public bool HasSSHElevationPassword => !string.IsNullOrEmpty(sshElevationPassword);

        /// <summary>
        /// The engine to use for SSH Requests.<para/>
        /// Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems -> SSH Engine.
        /// </summary>
        [XmlElement("injected_sshversion_devicegroup")]
        public SSHEngine? SSHEngine { get; set; }

        #endregion
        #region Credentials for VMware/XenServer

        /// <summary>
        /// Whether this object's VMware/XenServer credentials are inherited from its parent.<para/>
        /// Corresponds to Credentials for VMware/XenServer -> Inherit VMware Credentials.
        /// </summary>
        [XmlElement("injected_vmwareconnection")]
        public bool? InheritVMwareCredentials { get; set; }

        /// <summary>
        /// The username to use for VMware/XenServer authentication.<para/>
        /// Corresponds to Credentials for VMware/XenServer -> User.
        /// </summary>
        [XmlElement("injected_esxuser")]
        public string VMwareUserName { get; set; }

        /// <summary>
        /// The hidden VMware Password of this object.
        /// </summary>
        [XmlElement("injected_esxpassword")]
        internal string vmwarePassword { get; set; }

        /// <summary>
        /// Whether a VMware Password is set.<para/>
        /// Corresponds to Credentials for VMware/XenServer -> Password.
        /// </summary>
        public bool HasVMwarePassword => !string.IsNullOrEmpty(vmwarePassword);

        /// <summary>
        /// The protocol to use when connecting to VMware/XenServer systems.<para/>
        /// Corresponds to Credentials for VMware/XenServer -> VMware Protocol.
        /// </summary>
        [XmlElement("injected_esxprotocol")]
        public HttpMode VMwareProtocol { get; set; }

        /// <summary>
        /// Whether to reuse sessions for multiple sensor scans.<para/>
        /// Corresponds to Credentials for VMware/XenServer -> Session Pool.
        /// </summary>
        [XmlElement("injected_vmwaresessionpool")]
        public VMwareSessionMode? VMwareSessionMode { get; set; }

        #endregion
        #region Credentials for SNMP Devices

        /// <summary>
        /// Whether this object's SNMP credentials are inherited from its parent.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Inherit SNMP Credentials.
        /// </summary>
        [XmlElement("injected_snmpversiongroup")]
        public bool? InheritSNMPCredentials { get; set; }

        /// <summary>
        /// The version to use for SNMP.<para/>
        /// Corresponds to Credentials for SNMP Devices -> SNMP Version.
        /// </summary>
        [XmlElement("injected_snmpversion")]
        public SNMPVersion SNMPVersion { get; set; }

        /// <summary>
        /// The community string to use when using SNMP v1. The default value is 'public'.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Community String (SNMP v1).
        /// </summary>
        [XmlElement("injected_snmpcommv1")]
        public string SNMPCommunityStringV1 { get; set; }

        /// <summary>
        /// The community string to use when using SNMP v2c. The default value is 'public'.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Community String (SNMP v2c).
        /// </summary>
        [XmlElement("injected_snmpcommv2")]
        public string SNMPCommunityStringV2 { get; set; }

        /// <summary>
        /// The authentication type to use for SNMPv3.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Authentication Type.
        /// </summary>
        [XmlElement("injected_snmpauthmode")]
        public HashType SNMPv3AuthType { get; set; }

        /// <summary>
        /// The username to use for SNMPv3.<para/>
        /// Corresponds to Credentials for SNMP Devices -> User.
        /// </summary>
        [XmlElement("injected_snmpuser")]
        public string SNMPv3UserName { get; set; }

        /// <summary>
        /// The hidden SNMPv3 Password of this object.
        /// </summary>
        [XmlElement("injected_snmpauthpass")]
        internal string snmpv3Password { get; set; }

        /// <summary>
        /// Whether a SNMPv3 Password is set.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Password.
        /// </summary>
        public bool HasSNMPv3Password => !string.IsNullOrEmpty(snmpv3Password);

        /// <summary>
        /// The encryption type to use for SNMPv3.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Encryption Type.
        /// </summary>
        [XmlElement("injected_snmpencmode")]
        public EncryptionType SNMPv3EncryptionType { get; set; }

        /// <summary>
        /// The hidden SNMPv3 Encryption Key of this object.
        /// </summary>
        [XmlElement("injected_snmpencpass")]
        internal string snmpv3EncryptionKey { get; set; }

        /// <summary>
        /// Whether a SNMPv3 Encryption Key is set.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Data Encryption Key.
        /// </summary>
        public bool HasSNMPv3EncryptionKey => !string.IsNullOrEmpty(snmpv3EncryptionKey);

        /// <summary>
        /// The context name to use for SNMPv3. A context name is required only if specified by the target device.<para/>
        /// Corresponds to Credentials for SNMP Devices -> Context Name.
        /// </summary>
        [XmlElement("injected_snmpcontext")]
        public string SNMPv3Context { get; set; }

        /// <summary>
        /// The port to use for SNMP. The default value is 161.<para/>
        /// Corresponds to Credentials for SNMP Devices -> SNMP Port.
        /// </summary>
        [XmlElement("injected_snmpport")]
        public int SNMPPort { get; set; }

        /// <summary>
        /// The length of time (in seconds) before a SNMP request times out.<para/>
        /// Corresponds to Credentials for SNMP Devices -> SNMP Timeout.
        /// </summary>
        [XmlElement("injected_snmptimeout")]
        public int SNMPTimeout { get; set; }

        #endregion
        #region Credentials for Database Management Systems

        /// <summary>
        /// Whether this object's DBMS credentials are inherited from its parent.<para/>
        /// Corresponds to Credentials for Database Management Systems -> Inherit Database Credentials.
        /// </summary>
        [XmlElement("injected_dbcredentials")]
        public bool? InheritDBCredentials { get; set; }

        /// <summary>
        /// Indicates whether PRTG automatically determines the port of the DBMS or the port is set manually.<para/>
        /// Corresponds to Credentials for Database Management Systems -> Port for Databases.
        /// </summary>
        [XmlElement("injected_usedbcustomport")]
        public AutoMode DBPortMode { get; set; }

        /// <summary>
        /// The port to use for all database sensors. This only applies if the <see cref="DBPortMode"/> is <see cref="AutoMode.Manual"/>.<para/>
        /// Corresponds to Credentials for Database Management Systems -> Custom Database Port.
        /// </summary>
        [XmlElement("injected_dbport")]
        public int? DBPort { get; set; }

        /// <summary>
        /// The authentication mode PRTG uses to connect to a database server.<para/>
        /// Corresponds to Credentials for Database Management Systems -> Authentication Mode.
        /// </summary>
        [XmlElement("injected_dbauth")]
        public DBAuthMode DBAuthMode { get; set; }

        /// <summary>
        /// The username to use when <see cref="DBAuthMode"/> is <see cref="PrtgAPI.DBAuthMode.SQL"/>.<para/>
        /// Corresponds to Credentials for Database Management Systems -> User.
        /// </summary>
        [XmlElement("injected_dbuser")]
        public string DBUserName { get; set; }

        /// <summary>
        /// The hidden Database Password of this object.
        /// </summary>
        [XmlElement("injected_dbpassword")]
        internal string dbPassword { get; set; }

        /// <summary>
        /// Whether a database password is set.<para/>
        /// Corresponds to Credentials for Database Management Systems -> Password.
        /// </summary>
        public bool HasDBPassword => !string.IsNullOrEmpty(dbPassword);

        /// <summary>
        /// The length of time (in seconds) before a database request times out.<para/>
        /// Corresponds to Credentials for Database Management Systems -> Timeout.
        /// </summary>
        [XmlElement("injected_dbtimeout")]
        public int DBTimeout { get; set; }

        #endregion
        #region  Credentials for Amazon Cloudwatch

        /// <summary>
        /// Whether this object's Amazon Cloudwatch credentials are inherited from its parent.<para/>
        /// Corresponds to Credentials for Amazon Cloudwatch -> Inherit Amazon Credentials.
        /// </summary>
        [XmlElement("injected_cloudcredentials")]
        public bool? InheritAmazonCredentials { get; set; }

        /// <summary>
        /// The access key to use for Amazon Web Services.<para/>
        /// Corresponds to Credentials for Amazon Cloudwatch -> Access Key.
        /// </summary>
        [XmlElement("injected_awsak")]
        public string AmazonAccessKey { get; set; }

        /// <summary>
        /// The hidden Amazon Secret Key of this object.
        /// </summary>
        [XmlElement("injected_awssk")]
        internal string amazonSecretKey { get; set; }

        /// <summary>
        /// Whether an Amazon Secret Key is set.<para/>
        /// Corresponds to Credentials for Amazon Cloudwatch -> Secret Key.
        /// </summary>
        public bool HasAmazonSecretKey => !string.IsNullOrEmpty(amazonSecretKey);

        #endregion
        #region Windows Compatibility Options

        /// <summary>
        /// Whether this object's Windows Compatibility settings are inherited from its parent.<para/>
        /// Corresponds to Windows Compatibility Options -> Inherit Windows Compatibility Options.
        /// </summary>
        [XmlElement("injected_wmicompatibility")]
        public bool? InheritWindowsCompatibility { get; set; }

        /// <summary>
        /// The data source to use for performing WMI queries.<para/>
        /// Corresponds to Windows Compatibility Options -> Preferred Data Source.
        /// </summary>
        [XmlElement("injected_wmiorpc")]
        public WmiDataSource WmiDataSource { get; set; }

        /// <summary>
        /// The method to use for determining the timeout of WMI requests.<para/>
        /// Corresponds to Windows Compatibility Options -> Timeout Method.
        /// </summary>
        [XmlElement("injected_wmitimeoutmethod")]
        public WmiTimeoutMethod WmiTimeoutMethod { get; set; }

        /// <summary>
        /// The length of time (in seconds) before a WMI request times out. This only applies if <see cref="WmiTimeoutMethod"/> is <see cref="PrtgAPI.WmiTimeoutMethod.Manual"/>.<para/>
        /// Corresponds to Windows Compatibility Options -> Timeout Value.
        /// </summary>
        [XmlElement("injected_wmitimeout")]
        public string WmiTimeout { get; set; }

        #endregion
        #region SNMP Compatibility Options

        /// <summary>
        /// Whether this object's SNMP Compatibility settings are inherited from its parent.<para/>
        /// Corresponds to SNMP Compatibility Options -> Inherit SNMP Compatibility Options.
        /// </summary>
        [XmlElement("injected_snmpcompatibility")]
        public bool? InheritSNMPCompatibility { get; set; }

        /// <summary>
        /// The delay (in ms) PRTG should wait between multiple SNMP requests to a single device. This value must be 0-100ms. Higher values are not supported.<para/>
        /// Corresponds to SNMP Compatibility Options -> SNMP Delay.
        /// </summary>
        [XmlElement("injected_snmpdelay")] //todo: need to add some sort of validation to setting the snmp delay. what happens if you try and set a value higher than 100?
        public int SNMPDelay { get; set; }

        /// <summary>
        /// Whether PRTG should retry failed requests.<para/>
        /// Corresponds to SNMP Compatibility Options -> Failed Requests.
        /// </summary>
        [XmlElement("injected_retrysnmp")]
        public RetryMode SNMPRetryMode { get; set; }

        /// <summary>
        /// Whether PRTG treats overflow values as valid results.<para/>
        /// Corresponds to SNMP Compatibility Options -> Overflow Values.
        /// </summary>
        [XmlElement("injected_ignoreoverflow")]
        public SNMPOverflowMode SNMPOverflowMode { get; set; }

        /// <summary>
        /// Whether spurious "0" values should be ignored for delta (difference) sensors.<para/>
        /// Corresponds to SNMP Compatibility Options -> Zero Values.
        /// </summary>
        [XmlElement("injected_ignorezero")]
        public SNMPZeroValueMode SNMPZeroValueMode { get; set; }

        /// <summary>
        /// Whether to use force using 32-bit counters even when a device reports 64-bit counters are available.<para/>
        /// Corresponds to SNMP Compatibility Options -> 32-bit/64-bit Counters.
        /// </summary>
        [XmlElement("injected_force32")]
        public SNMPCounterMode SNMPCounterMode { get; set; }

        /// <summary>
        /// Whether to include a single or multiple SNMP requests in each request.<para/>
        /// Corresponds to SNMP Compatibility Options -> Request Mode.
        /// </summary>
        [XmlElement("injected_usesingleget")]
        public SNMPRequestMode SNMPRequestMode { get; set; }

        /// <summary>
        /// Name template to define how the sensor name should be displayed. Template variables include [port], [ifalias], [ifname], [ifdescr], [ifspeed], [ifsensor] and custom OIDs.<para/>
        /// Corresponds to SNMP Compatibility Options -> Port Name Template.
        /// </summary>
        [XmlElement("injected_trafficportname")]
        public string SNMPPortNameTemplate { get; set; }

        /// <summary>
        /// Whether PRTG should automatically update SNMP port sensor names when those names change in the device.<para/>
        /// Corresponds to SNMP Compatibility Options -> Port Name Update.
        /// </summary>
        [XmlElement("injected_updateportname")]
        public SNMPPortNameUpdateMode SNMPPortNameUpdateMode { get; set; }

        /// <summary>
        /// Specifies the field to use to identify SNMP interfaces in the event the interface port order changes on reboot.<para/>
        /// Corresponds to SNMP Compatibility Options -> Port Identification.
        /// </summary>
        [XmlElement("injected_portupdateoid")]
        public SNMPPortIdentification SNMPPortIdMode { get; set; }

        /// <summary>
        /// The start index for the interface range SNMP Traffic sensors can query during sensor creation. If this value is 0, PRTG will use "automatic mode".<para/>
        /// Corresponds to SNMP Compatibility Options -> Start Interface Index.
        /// </summary>
        [XmlElement("injected_portstart")]
        public int SNMPInterfaceStartIndex { get; set; }

        /// <summary>
        /// The end index for the interface range SNMP Traffic sensors can query during sensor creation. If this value is 0, PRTG will use "automatic mode".<para/>
        /// Corresponds to SNMP Compatibility Options -> End Interface Index.
        /// </summary>
        [XmlElement("injected_portend")]
        public int SNMPInterfaceEndIndex { get; set; }

        #endregion

        //todo: the rest, including proxy settings for http sensors automatic monitoring data analysis

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}