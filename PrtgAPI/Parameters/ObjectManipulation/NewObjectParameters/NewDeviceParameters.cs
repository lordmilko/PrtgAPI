using System.Collections.Generic;
using PrtgAPI.Attributes;
using PrtgAPI.Linq;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// <para type="description">Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for adding new <see cref="Device"/> objects.</para>
    /// </summary>
    public class NewDeviceParameters : NewObjectParameters
    {
        internal override CommandFunction Function => CommandFunction.AddDevice2;

        #region Device Specific

        /// <summary>
        /// Gets or sets the IP Address or HostName to use to connect to this device.
        /// </summary>
        [RequireValue(true)]
        public string Host
        {
            get
            {
                if (IPVersion == IPVersion.IPv4)
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
        /// Gets or sets the internet protocol version to use to connect to this device.
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
        /// Gets or sets how thoroughly PRTG should scan for compatible sensor types when performing an auto-discovery.
        /// </summary>
        public AutoDiscoveryMode AutoDiscoveryMode
        {
            get { return (AutoDiscoveryMode)GetCustomParameterEnumXml<AutoDiscoveryMode>(ObjectProperty.AutoDiscoveryMode); }
            set
            {
                SetCustomParameterEnumXml(ObjectProperty.AutoDiscoveryMode, value);

                if (value != AutoDiscoveryMode.AutomaticTemplate && DeviceTemplates != null)
                {
                    DeviceTemplates = null;
                    RemoveCustomParameter(ObjectPropertyInternal.HasDeviceTemplate);
                }
            }
        }

        /// <summary>
        /// Gets or sets how often auto-discovery operations should be performed to create new sensors.
        /// </summary>
        public AutoDiscoverySchedule AutoDiscoverySchedule
        {
            get { return (AutoDiscoverySchedule)GetCustomParameterEnumXml<IPVersion>(ObjectProperty.AutoDiscoverySchedule); }
            set { SetCustomParameterEnumXml(ObjectProperty.AutoDiscoverySchedule, value); }
        }

        /// <summary>
        /// Gets or sets the device templates to use when performing an auto-discovery. If <see cref="PrtgAPI.AutoDiscoveryMode.Automatic"/> or
        /// <see cref="PrtgAPI.AutoDiscoveryMode.AutomaticDetailed"/>  is specified, all templates will be used and this parameter will be ignored.<para/>
        /// If <see cref="PrtgAPI.AutoDiscoveryMode.AutomaticTemplate"/> is specified, at least one template must be specified.
        /// </summary>
        [DependentProperty(ObjectProperty.AutoDiscoveryMode, AutoDiscoveryMode.AutomaticTemplate)]
        public List<DeviceTemplate> DeviceTemplates
        {
            get { return (List<DeviceTemplate>) this[Parameter.DeviceTemplate]; }
            set
            {
                value = value?.WithoutNull();

                this[Parameter.DeviceTemplate] = value;

                if (value != null && value.Count > 0)
                {
                    AutoDiscoveryMode = AutoDiscoveryMode.AutomaticTemplate;
                    SetCustomParameterBool(ObjectPropertyInternal.HasDeviceTemplate, true);
                }
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="NewDeviceParameters"/> class.
        /// </summary>
        /// <param name="name">The name to use for this device.</param>
        /// <param name="host">The IP Address or HostName to use to connect to this device. If the host is not specified, the <paramref name="name"/> is used as the host.</param>
        public NewDeviceParameters(string name, string host = null) : base(name)
        {
            Host = host ?? name;
            IPVersion = IPVersion.IPv4;
            AutoDiscoveryMode = AutoDiscoveryMode.Manual;
            AutoDiscoverySchedule = AutoDiscoverySchedule.Once;
        }
    }
}
