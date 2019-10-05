using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Adds a new device to a PRTG Group or Probe.</para>
    /// 
    /// <para type="description">The Add-Device cmdlet adds a new device to a PRTG Group or Probe. When adding a new
    /// device, Add-Device supports two methods of specifying the parameters required to create the object. For basic scenarios
    /// where you wish to inherit all settings from the parent object, the <see cref="Name"/>, <see cref="Host"/> and auto-discovery method
    /// can all be specified as arguments directly to Add-Device. If a -<see cref="Host"/> is not specified, Add-Device will automatically
    /// use the -<see cref="Name"/> as the device's hostname. If -<see cref="AutoDiscover"/> is specified, Add-Device will perform an
    /// <see cref="AutoDiscoveryMode.Automatic"/> auto-discovery.</para>
    /// 
    /// <para type="description">For more advanced scenarios where you wish to specify more advanced parameters (such as the Internet Protocol
    /// version used to communicate with the device) a <see cref="NewDeviceParameters"/> object can instead be created with the New-DeviceParameters cmdlet.
    /// When the parameters object is passed to Add-Device, PrtgAPI will validate that all mandatory parameter fields contain values.
    /// If a mandatory field is missing a value, Add-Sensor will throw an <see cref="InvalidOperationException"/>, listing the field whose value was missing.</para>
    /// 
    /// <para type="description">By default, Add-Device will attempt to resolve the created device to a <see cref="Device"/> object.
    /// As PRTG does not return the ID of the created object, PrtgAPI identifies the newly created device by comparing the devices
    /// under the parent object before and after the new device is created. While this is generally very reliable, in the event
    /// something or someone else creates another new device directly under the target object with the same Name, that object
    /// will also be returned in the objects resolved by Add-Device. If you do not wish to resolve the created device, this behavior can be
    /// disabled by specifying -Resolve:$false.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Probe contoso | Add-Device dc-1</code>
    ///     <para>Add a new device named "dc-1" to the Contoso probe, using "dc-1" as its hostname, without performing an automatic auto-discovery.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Group -Id 2305 | Add-Device exch-1 192.168.0.2 -AutoDiscover</code>
    ///     <para>Add a device named "exch-1" to the group with ID 2305, using 192.168.0.2 as its IP Address and performing an automatic auto-discovery after the device is created.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = New-DeviceParameters sql-1 "2001:db8::ff00:42:8329"
    ///         C:\> $params.IPVersion = "IPv6"
    ///
    ///         C:\> Get-Probe contoso | Add-Device $params
    ///     </code>
    ///     <para>Add a device named sql-1 using an IPv6 Address to the probe Contoso probe.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Object-Creation#devices-1">Online version:</para>
    /// <para type="link">New-DeviceParameters</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// 
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "Device", SupportsShouldProcess = true)]
    public class AddDevice : AddParametersObject<NewDeviceParameters, Device, GroupOrProbe>
    {
        /// <summary>
        /// <para type="description">The parent object to create an object under.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Basic)]
        public override GroupOrProbe Destination
        {
            get { return DestinationInternal; }
            set { DestinationInternal = value; }
        }

        /// <summary>
        /// <para type="description">A set of parameters whose properties describe the type of object to add, with what settings.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Default)]
        public override NewDeviceParameters Parameters
        {
            get { return ParametersInternal; }
            set { ParametersInternal = value; }
        }

        /// <summary>
        /// <para type="description">The name to use for the device. If a <see cref="Host"/> is not specified, this value will be used as the hostname as well.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Basic)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The IPv4 Address/HostName to use for monitoring this device. If this value is not specified, the <see cref="Name"/> will be used as the hostname.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = ParameterSet.Basic)]
        public new string Host { get; set; }

        /// <summary>
        /// <para type="description">Whether to perform an <see cref="AutoDiscoveryMode.Automatic"/> auto-discovery on the newly created device.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Basic)]
        public SwitchParameter AutoDiscover { get; set; }

        /// <summary>
        /// <para type="description">One or more wildcards specifying the device templates to used when performing the auto-discovery. If no templates are specified,
        /// all templates will be used.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Basic)]
        public string[] Template { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddDevice"/> 
        /// </summary>
        public AddDevice() : base(BaseType.Device)
        {
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ParameterSetName == ParameterSet.Basic)
            {
                Parameters = new NewDeviceParameters(Name, GetHost());

                if (AutoDiscover)
                {
                    if (Template != null && Template.Length > 0)
                    {
                        Parameters.AutoDiscoveryMode = AutoDiscoveryMode.AutomaticTemplate;

                        var templates = client.GetDeviceTemplates();

                        templates = templates.Where(template => Template
                            .Select(name => new WildcardPattern(name, WildcardOptions.IgnoreCase))
                            .Any(filter => filter.IsMatch(template.Name) || filter.IsMatch(template.Value))
                        ).ToList();

                        if (templates.Count == 0)
                            throw new ArgumentException($"No device templates could be found that match the specified template names {Template.ToQuotedList()}.");

                        Parameters.DeviceTemplates = templates;
                    }
                    else
                        Parameters.AutoDiscoveryMode = AutoDiscoveryMode.Automatic;
                }
            }

            base.ProcessRecordEx();
        }

        private string GetHost()
        {
            return string.IsNullOrEmpty(Host) ? Name : Host;
        }

        internal override string WhatIfDescription()
        {
            return $"(Host: {GetHost()}) ";
        }

        /// <summary>
        /// Resolves the children of the destination object that match the new object's name.
        /// </summary>
        /// <param name="filters">An array of search filters used to retrieve all children of the destination with the specified name.</param>
        /// <returns>All objects under the parent object that match the new object's name.</returns>
        protected override List<Device> GetObjects(SearchFilter[] filters) => client.GetDevices(filters);
    }
}
