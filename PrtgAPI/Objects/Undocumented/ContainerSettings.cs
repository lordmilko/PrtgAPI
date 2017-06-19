using System.Xml.Serialization;
using PrtgAPI.Attributes;

namespace PrtgAPI.Objects.Undocumented
{
    public abstract class ContainerSettings : ObjectSettings
    {
        //Basic Settings

        [XmlElement("injected_name")]
        public string Name { get; set; }

        [XmlElement("injected_tags")]
        [SplittableString(' ')]
        public string[] Tags { get; set; }

        [XmlElement("injected_active")]
        public bool Active { get; set; }

        [XmlElement("injected_priority")]
        public Priority Priority { get; set; }

        //Location

        [XmlElement("injected_locationgroup")]
        public bool InheritLocation { get; set; }

        [XmlElement("injected_location")]
        public string Location { get; set; }

        //Credentials for Windows Systems

        [XmlElement("injected_windowsconnection")]
        public bool InheritWindowsCredentials { get; set; }

        [XmlElement("injected_windowslogindomain")]
        public string WindowsDomain { get; set; }

        [XmlElement("injected_windowsloginusername")]
        public string WindowsUserName { get; set; }

        //Credentials for Linux/Solaris/Mac OS (SSH/WBEM) Systems

        [XmlElement("injected_linuxconnection")]
        public bool InheritLinuxCredentials { get; set; }

        [XmlElement("injected_linuxloginusername")]
        public string LinuxUserName { get; set; }

        [XmlElement("injected_linuxloginmode")]
        public LinuxLoginMode LinuxLoginMode { get; set; }

        [XmlElement("injected_wbemportmode")]
        public AutoMode WbemPort { get; set; }

        [XmlElement("injected_sshport")]
        public int SSHPort { get; set; }

        [XmlElement("injected_sshelevatedrights")]
        public SSHElevationMode SSHElevationMode { get; set; }

        [XmlElement("injected_elevationnamesu")]
        [XmlElement("injected_elevationnamesudo")]
        public string SSHElevationUser { get; set; }

        [XmlElement("injected_sshversion_devicegroup")]
        public SSHEngine SSHEngine { get; set; }

        //Credentials for VMware/XenServer

        [XmlElement("injected_vmwareconnection")]
        public bool InheritVMwareCredentials { get; set; }

        [XmlElement("injected_esxuser")]
        public string VMwareUserName { get; set; }

        [XmlElement("injected_esxprotocol")]
        public HTTPMode VMWareProtocol { get; set; }

        [XmlElement("injected_vmwaresessionpool")]
        public VMwareSessionMode SessionPool { get; set; }

        //Credentials for SNMP Devices

        [XmlElement("injected_snmpversiongroup")]
        public bool InheritSNMPCredentials { get; set; }

        [XmlElement("injected_snmpversion")]
        public SNMPVersion SNMPVersion { get; set; }

        [XmlElement("injected_snmpcommv2")]
        [XmlElement("injected_snmpcommv1")]
        public string SNMPCommunityString { get; set; }

        [XmlElement("injected_snmpauthmode")]
        public HashType SNMPv3AuthType { get; set; }

        [XmlElement("injected_snmpuser")]
        public string SNMPv3UserName { get; set; }

        [XmlElement("injected_snmpencmode")]
        public EncryptionType SNMPv3EncryptionType { get; set; }

        [XmlElement("injected_snmpencpass")]
        public string SNMPv3EncryptionKey { get; set; }

        [XmlElement("injected_snmpcontext")]
        public string SNMPv3Context { get; set; }

        [XmlElement("injected_snmpport")]
        public int SNMPPort { get; set; }

        [XmlElement("injected_snmptimeout")]
        public int SNMPTimeout { get; set; }

        //Credentials for Database Management Systems

        [XmlElement("injected_dbcredentials")]
        public bool InheritDBCredentials { get; set; }

        [XmlElement("injected_usedbcustomport")]
        public AutoMode DBPortMode { get; set; }

        [XmlElement("injected_dbport")]
        public int? DBPort { get; set; }

        [XmlElement("injected_dbauth")]
        public DBAuthMode DBAuthMode { get; set; }

        [XmlElement("injected_dbuser")]
        public string DBUser { get; set; }

        [XmlElement("injected_dbtimeout")]
        public int DBTimeout { get; set; }

        //Credentials for Amazon Cloudwatch

        [XmlElement("injected_cloudcredentials")]
        public bool InheritAmazonCredentials { get; set; }

        [XmlElement("injected_awsak")]
        public string AmazonAccessKey { get; set; }

        [XmlElement("injected_awssk")]
        public string AmazonSecretKey { get; set; }

        //Windows Compatibility Options

        [XmlElement("injected_wmicompatibility")]
        public bool InheritWindowsCompatibility { get; set; }

        [XmlElement("injected_wmiorpc")]
        public WmiDataSource WmiDataSource { get; set; }

        [XmlElement("injected_wmitimeoutmethod")]
        public WmiTimeoutMethod WmiTimeoutMethod { get; set; }

        [XmlElement("injected_wmitimeout")]
        public string WmiTimeout { get; set; }

        //SNMP Compatibility Options

        [XmlElement("injected_snmpcompatibility")]
        public bool InheritSNMPCompatibility { get; set; }

        [XmlElement("injected_snmpdelay")]
        public int SNMPDelay { get; set; }

        [XmlElement("injected_retrysnmp")]
        public RetryMode SNMPRetryMode { get; set; }

        [XmlElement("injected_ignoreoverflow")]
        public SNMPOverflowMode SNMPOverflowMode { get; set; }

        [XmlElement("injected_ignorezero")]
        public SNMPZeroValueMode SNMPZeroValueMode { get; set; }

        [XmlElement("injected_force32")]
        public SNMPCounterMode SNMPCounterMode { get; set; }

        [XmlElement("injected_usesingleget")]
        public SNMPRequestMode SNMPRequestMode { get; set; }

        [XmlElement("injected_trafficportname")]
        public string SNMPPortNameTemplate { get; set; }

        [XmlElement("injected_updateportname")]
        public SNMPPortNameUpdateMode SNMPPortNameUpdateMode { get; set; }

        [XmlElement("injected_portupdateoid")]
        public SNMPPortIdentification SNMPPortIdentification { get; set; }

        [XmlElement("injected_portstart")]
        public int SNMPStartInterfaceIndex { get; set; }

        [XmlElement("injected_portend")]
        public int SNMPEndInterfaceIndex { get; set; }

        //todo: the rest, including automatic monitoring data analysis
    }
}