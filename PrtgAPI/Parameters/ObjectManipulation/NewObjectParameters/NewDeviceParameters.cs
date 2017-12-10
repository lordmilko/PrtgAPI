using System;
using PrtgAPI.Attributes;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// <para type="description">Represents parameters used to construct a <see cref="PrtgUrl"/> for adding new <see cref="Device"/> objects.</para>
    /// </summary>
    public class NewDeviceParameters : NewObjectParameters
    {
        #region Device Specific

        /// <summary>
        /// The IP Address or HostName to use to connect to this device.
        /// </summary>
        [RequireValue(true)]
        public string Host
        {
            get
            {
                if(IPVersion == IPVersion.IPv4)
                    return (string)GetCustomParameter(ObjectProperty.Hostv4);

                return (string)GetCustomParameter(ObjectProperty.Hostv6);
            }
            set
            {
                var ipVersion = (IPVersion?) GetCustomParameterEnumXml<IPVersion>(ObjectPropertyInternal.IPVersion);

                if (ipVersion == IPVersion.IPv4 || ipVersion == null)
                    SetCustomParameter(ObjectProperty.Hostv4, value);
                else
                    SetCustomParameter(ObjectProperty.Hostv6, value);
            }
        }

        /// <summary>
        /// The internet protocol version to use to connect to this device.
        /// </summary>
        public IPVersion IPVersion
        {
            get { return (IPVersion)GetCustomParameterEnumXml<IPVersion>(ObjectPropertyInternal.IPVersion); }
            set
            {
                if (value == IPVersion.IPv4)
                {
                    var hostv6 = GetCustomParameter(ObjectProperty.Hostv6);

                    if (!string.IsNullOrEmpty(hostv6?.ToString()))
                    {
                        SetCustomParameter(ObjectProperty.Hostv4, hostv6);
                        SetCustomParameter(ObjectProperty.Hostv6, null);
                    }
                }
                else
                {
                    var hostv4 = GetCustomParameter(ObjectProperty.Hostv4);

                    if (!string.IsNullOrEmpty(hostv4?.ToString()))
                    {
                        SetCustomParameter(ObjectProperty.Hostv6, hostv4);
                        SetCustomParameter(ObjectProperty.Hostv4, null);
                    }
                }

                SetCustomParameterEnumXml(ObjectPropertyInternal.IPVersion, value);

            }
        }

        //todo: device icon

        /// <summary>
        /// How thoroughly PRTG should scan for compatible sensor types when performing an auto-discovery.
        /// </summary>
        public AutoDiscoveryMode AutoDiscoveryMode
        {
            get { return (AutoDiscoveryMode)GetCustomParameterEnumXml<AutoDiscoveryMode>(ObjectProperty.AutoDiscoveryMode); }
            set { SetCustomParameterEnumXml(ObjectProperty.AutoDiscoveryMode, value); }
        }

        /// <summary>
        /// How often auto-discovery operations should be performed to create new sensors.
        /// </summary>
        public AutoDiscoverySchedule AutoDiscoverySchedule
        {
            get { return (AutoDiscoverySchedule)GetCustomParameterEnumXml<IPVersion>(ObjectProperty.AutoDiscoverySchedule); }
            set { SetCustomParameterEnumXml(ObjectProperty.AutoDiscoverySchedule, value); }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="NewDeviceParameters"/> class.
        /// </summary>
        /// <param name="deviceName">The name to use for this device.</param>
        /// <param name="host">The IP Address or HostName to use to connect to this device.</param>
        public NewDeviceParameters(string deviceName, string host) : base(deviceName)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentException($"{nameof(host)} cannot be null or empty", nameof(host));

            Host = host;
            IPVersion = IPVersion.IPv4;
            AutoDiscoveryMode = AutoDiscoveryMode.Manual;
            AutoDiscoverySchedule = AutoDiscoverySchedule.Once;
        }
    }
}
