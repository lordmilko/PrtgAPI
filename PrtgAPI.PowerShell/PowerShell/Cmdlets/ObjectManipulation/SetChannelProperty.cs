using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Parameters;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Utilities;
using IDynamicParameters = System.Management.Automation.IDynamicParameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Modifies the value of a PRTG channel property.</para>
    /// 
    /// <para type="description">The Set-ChannelProperty cmdlet modifies properties and settings of PRTG Channels.
    /// All supported properties that can be modified are typesafe, using the type of the property on the Channel object
    /// returned from Get-Channel.</para>
    /// 
    /// <para type="description">When a value is specified, Set-ChannelProperty will attempt to parse the value into its expected type.
    /// If the type cannot be parsed, an exception will be thrown indicating the type of the object specified and the type that was expected.
    /// In the case of enums, Set-ChannelProperty will list all valid values of the target type so that you may know exactly how to interface
    /// with the specified property. In the event you wish to modify multiple properties in a single request, Set-ChannelProperty provides dynamically
    /// generated parameters for each property supported by PrtgAPI.</para>
    /// 
    /// <para type="description">In the event that a property is specified that has a dependency on another property ebing set (such as UpperErrorLimit
    /// requiring LimitsEnabled be $true) Set-ChannelProperty will automatically assign the required values such that the original property may be
    /// correctly enabled. If the parent of a property is set to the opposite of a child property's required value PrtgAPI will automatically remove
    /// the value specified on the child as well. For example, if LimitsEnabled is set to $false, all error and warning limit related properties
    /// will be set to $null. If LimitsEnabled is later set to $true, you will need to repopulate the values of all error and warning limit properties.</para>
    /// 
    /// <para type="description">For a list of all settings currently supported by Set-ChannelProperty, see Get-Help about_ChannelSettings.</para>
    /// 
    /// <para type="description">By default, Set-ChannelProperty will operate in Batch Mode. In Batch Mode, Set-ChannelProperty
    /// will not execute a request for each individual object, but will rather store each item in a queue to modify channel properties
    /// for all objects at once, via a single request. This allows PrtgAPI to be extremely performant in performing operations
    /// against a large number of objects.</para>
    /// 
    /// <para type="description">If the pipeline is cancelled (either due to a cmdlet throwing an exception
    /// or the user pressing Ctrl-C) before fully completing, Set-ChannelProperty will not generate a request against PRTG.
    /// If you wish to disable Batch Mode and fully process objects individually one at a time, this can be achieved
    /// by specifying -Batch:$false.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Tags wmicpuloadsensor | Get-Channel Total | Set-ChannelProperty UpperErrorLimit 90</code>
    ///     <para>Set the upper error limit of the "Total" channel of all WMI CPU Load sensors to 90. Will also set LimitsEnabled to $true</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $channels = Get-Sensor -Tags wmicpuloadsensor | Get-Channel Total
    ///
    ///         C:\> $channels | Set-ChannelProperty -UpperErrorLimit 80 -LowerErrorLimit 20
    ///     </code>
    ///     <para>Set the value of both the UpperErrorLimit and LowerErrorLimit in a single request.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Property-Manipulation#modifying-properties-1">Online version:</para>
    /// <para type="link">Get-Help ChannelSettings</para>
    /// <para type="link">Get-Channel</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Set-ObjectProperty</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ChannelProperty", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class SetChannelProperty : PrtgMultiOperationCmdlet, IDynamicParameters
    {
        /// <summary>
        /// <para type="description">Channel to set the properties of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Dynamic)]
        public Channel Channel { get; set; }

        /// <summary>
        /// <para type="description">ID of the channel's parent sensor.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.DynamicManual)]
        public int SensorId { get; set; }

        /// <summary>
        /// <para type="description">ID of the channel to set the properties of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.DynamicManual)]
        public int ChannelId { get; set; }

        /// <summary>
        /// <para type="description">Property of the channel to set.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = ParameterSet.Manual)]
        public ChannelProperty Property { get; set; }

        /// <summary>
        /// <para type="description">Value to set the property to.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = false, Position = 3, ParameterSetName = ParameterSet.Manual)]
        [AllowEmptyString]
        public object Value { get; set; }

        internal override string ProgressActivity => "Modify PRTG Channel Settings";

        private PropertyDynamicParameterSet<ChannelProperty> dynamicParams;

        private List<ChannelParameter> dynamicParameters;

        /// <summary>
        /// Provides an enhanced one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessingEx()
        {
            Value = PSObjectUtilities.CleanPSObject(Value);

            if (DynamicSet())
                dynamicParameters = dynamicParams.GetBoundParameters(this, (p, v) => new ChannelParameter(p, PSObjectUtilities.CleanPSObject(v)));

            base.BeginProcessingEx();
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            var str = Channel != null ? $"'{Channel.Name}' (Channel ID: {Channel.Id}, Sensor ID: {Channel.SensorId})" : $"Channel ID: {ChannelId} (Sensor ID: {SensorId})";

            if (!MyInvocation.BoundParameters.ContainsKey("Value") && !DynamicSet())
                throw new ParameterBindingException("Value parameter is mandatory, however a value was not specified. If Value should be empty, specify $null.");

            if (Channel != null)
            {
                SensorId = Channel.SensorId;
                ChannelId = Channel.Id;
            }

            if (ShouldProcess(str, $"Set-ChannelProperty {GetShouldProcessMessage()}"))
            {
                var desc = Channel != null ? Channel.Name : $"ID {ChannelId}";

                //Can't batch something if there's no pipeline input
                if (Channel == null)
                    Batch = false;

                ExecuteOrQueue(Channel, $"Queuing channel '{desc}'");
            }
        }

        private string GetShouldProcessMessage()
        {
            if (DynamicSet())
            {
                var strActions = dynamicParameters.Select(p => $"{p.Property} = '{p.Value}'");
                var str = string.Join(", ", strActions);

                return str;
            }

            return $"{Property} = '{Value}'";
        }

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected override void PerformSingleOperation()
        {
            string message;

            Action action;

            if (ParameterSetName == ParameterSet.Default)
            {
                message = $"Setting channel '{Channel.Name}' (Sensor ID: {Channel.SensorId}) setting '{Property}' to '{Value}'";

                action = () => client.SetChannelProperty(Channel, Property, Value);
            }
            else if (DynamicSet())
            {
                var strActions = dynamicParameters.Select(p => $"'{p.Property}' to '{p.Value}'");
                var str = string.Join(", ", strActions);

                var name = Channel != null ? $"channel '{Channel.Name}'" : $"ID {ChannelId}";

                message = $"Setting channel {name} (Sensor ID: {SensorId}) setting {str}";

                switch (ParameterSetName)
                {
                    case ParameterSet.Dynamic:
                        action = () => client.SetChannelProperty(Channel, dynamicParameters.ToArray());
                        break;
                    case ParameterSet.DynamicManual:
                        action = () => client.SetChannelProperty(SensorId, ChannelId, dynamicParameters.ToArray());
                        break;
                    default:
                        throw new UnknownParameterSetException(ParameterSetName);
                }
            }
            else
            {
                message = $"Setting channel ID {ChannelId} (Sensor ID: {SensorId} setting {Property} to '{Value}'";

                action = () => client.SetChannelProperty(SensorId, ChannelId, Property, Value);
            }

            ExecuteOperation(action, message);
        }

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected override void PerformMultiOperation(int[] ids)
        {
            var groups = objects.Cast<Channel>().GroupBy(o => o.Id).ToList();

            for (int i = 0; i < groups.Count; i++)
            {
                var complete = groups.Count == i + 1;

                var sensorIds = groups[i].Select(j => j.SensorId).ToArray();

                var nameGroups = groups[i].GroupBy(g => g.Name).ToList();

                var summary = GetListSummary(nameGroups, g =>
                {
                    var t = "Sensor ID";

                    if (g.Count() > 1)
                        t += "s";

                    var strId = g.Select(a => a.SensorId.ToString());

                    return $"'{g.Key}' ({t}: {string.Join(", ", strId)})";
                });

                var type = "channel";

                if (nameGroups.Count() > 1)
                    type += "s";

                string message;

                if (DynamicSet())
                {
                    var strActions = dynamicParameters.Select(p => $"'{p.Property}' to '{p.Value}'");
                    var str = string.Join(", ", strActions);
                    message = $"Setting {type} {summary} setting {str}";
                }
                else
                {
                    message = $"Setting {type} {summary} setting '{Property}' to '{Value}'";
                    dynamicParameters = new[] {new ChannelParameter(Property, Value)}.ToList();
                }

                ExecuteMultiOperation(() => client.SetChannelProperty(groups[i].ToList(), dynamicParameters.ToArray()), message, complete);
            }
        }

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Channel;

        /// <summary>
        /// Retrieves an object that defines the dynamic parameters of this cmdlet.
        /// </summary>
        /// <returns>An object that defines the dynamic parameters of this cmdlet.</returns>
        public object GetDynamicParameters()
        {
            if (dynamicParams == null)
                dynamicParams = new PropertyDynamicParameterSet<ChannelProperty>(
                    new[] { ParameterSet.Dynamic, ParameterSet.DynamicManual },
                    e => ObjectPropertyParser.GetPropertyInfoViaPropertyParameter<Channel>(e).Property
                );

            return dynamicParams.Parameters;
        }

        private bool DynamicSet()
        {
            return ParameterSetName == ParameterSet.Dynamic || ParameterSetName == ParameterSet.DynamicManual;
        }
    }
}
