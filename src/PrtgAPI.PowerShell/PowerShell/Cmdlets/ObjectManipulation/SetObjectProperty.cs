using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Utilities;
using IDynamicParameters = System.Management.Automation.IDynamicParameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Modifies the value of a PRTG object property.</para>
    /// 
    /// <para type="description">The Set-ObjectProperty cmdlet modifies properties and settings of PRTG Objects including
    /// Sensors, Devices, Groups Probes, Notification Actions and Schedules.  All supported properties that can be modified
    /// are typesafe, using the type of the property on the Settings object returned from Get-ObjectProperty.</para>
    /// 
    /// <para type="description">When a value is specified, Set-ObjectProperty will attempt to parse the value into its expected type. If the
    /// type cannot be parsed, an exception will be thrown indicating the type of the object specified and the type of value that was expected.
    /// In the case of enums, Set-ObjectProperty will list all valid values of the target type so that you may know how exactly to interface with the
    /// specified property. In the event you wish to modify multiple properties in a single request, Set-ObjectProperty provides dynamically
    /// generated parameters for each property supported by PrtgAPI.</para>
    /// 
    /// <para type="description">In the event a property is specified that has a dependency on another property being set (such as Interval requiring
    /// InheritInterval be $false, DBPort require DBPortMode be manual, etc) Set-ObjectProperty will automatically assign the required values such that
    /// the original property may be correctly enabled.</para>
    /// 
    /// <para type="description">For properties that are not currently supported by PrtgAPI, these settings can still be interfaced with via
    /// the -RawProperty and -RawValue parameters. When -Raw* parameters are specified, PrtgAPI will not perform any validation on the specified values.
    /// The -RawProperty parameter must contain the raw property name, as typically found on the "name" attribute of  the &lt;input/&gt; tag
    /// under the object's Settings page in the PRTG UI. All properties that end in an underscore should include this in their names. All properties
    /// typically end in an underscore, with the exception of section inheritance properties (Inherit Windows Credentials, etc). Multiple raw
    /// properties can be modified at once by specifying a hashtable to the -RawParameters parameter.</para>
    /// 
    /// <para type="description">For a list of settings currently supported by Set-ObjectProperty, see Get-Help about_ObjectSettings and about_SensorSettings.</para>
    /// 
    /// <para type="description">By default, Set-ObjectProperty will operate in Batch Mode. In Batch Mode, Set-ObjectProperty
    /// will not execute a request for each individual object, but will rather store each item in a queue to modify object properties
    /// for all objects at once, via a single request. This allows PrtgAPI to be extremely performant in performing operations
    /// against a large number of objects.</para>
    /// 
    /// <para type="description">If the pipeline is cancelled (either due to a cmdlet throwing an exception
    /// or the user pressing Ctrl-C) before fully completing, Set-ObjectProperty will not generate a request against PRTG.
    /// If you wish to disable Batch Mode and fully process objects individually one at a time, this can be achieved
    /// by specifying -Batch:$false.</para>
    /// 
    /// <example>
    ///     <code>
    ///         C:\> Get-Sensor -Id 1001 | Set-ObjectProperty Interval "00:00:30"
    ///         C:\> Get-Sensor -Id 1001 | Set-ObjectProperty Interval ([TimeSpan]"00:00:30")
    ///         C:\> Get-Sensor -Id 1001 | Set-ObjectProperty Interval ThirtySeconds
    ///     </code>
    ///     <para>Set the Scanning Interval of the sensor with ID 1001 to 30 seconds three different ways. Type "ScanningInterval"
    /// expected by property Interval will attempt to coerce strings, TimeSpans and enums into a ScanningInterval object. If the
    /// cmdlet succeeds, InheritInterval will also be set to false.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Id 2002 | Set-ObjectProperty -VMwareUserName root -VMwarePassword password</code>
    ///     <para>Set the value of both the VMwareUserName and VMwarePassword in a single request.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1001 | Set-ObjectProperty -RawProperty name_ -RawValue newName</code>
    ///     <para>Set raw property "name" to value "newName"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $schedule = Get-PrtgSchedule -Id 621
    ///
    ///         C:\> Get-Sensor -Id 2024 | Set-ObjectProperty -RawParameters @{
    ///         >>      scheduledependency = 0
    ///         >>      schedule_ = $schedule
    ///         >>   }
    ///     </code>
    ///     <para>Apply the schedule with ID 621 to the sensor with ID 2024</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Set-ObjectProperty -Id 1001 Name "New Name"</code>
    ///     <para>Set the name of the object with ID 1001 to "New Name"</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Property-Manipulation#set-1">Online version:</para>
    /// <para type="link">Get-Help ObjectSettings</para>
    /// <para type="link">Get-Help SensorSettings</para>
    /// <para type="link">Get-ObjectProperty</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Probe</para>
    /// <para type="link">Get-NotificationAction</para>
    /// <para type="link">Get-PrtgSchedule</para> 
    /// <para type="link">Set-ChannelProperty</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ObjectProperty", SupportsShouldProcess = true, DefaultParameterSetName = ParameterSet.Default)]
    public class SetObjectProperty : PrtgMultiOperationCmdlet, IDynamicParameters
    {
        /// <summary>
        /// <para type="description">The object to modify the properties of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.RawProperty)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Raw)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Dynamic)]
        public PrtgObject Object { get; set; }

        /// <summary>
        /// <para type="description">ID of the object to modify properties of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Manual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawPropertyManual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawManual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.DynamicManual)]
        public int[] Id { get; set; }

        /// <summary>
        /// <para type="description">The property to modify.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Manual)]
        public ObjectProperty Property { get; set; }

        /// <summary>
        /// <para type="description">The value to set for the specified property.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = ParameterSet.Manual)]
        public object Value { get; set; }

        /// <summary>
        /// <para type="description">The raw name of the property to modify. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.</para>
        /// This value typically ends in an underscore.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawProperty)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawPropertyManual)]
        public string RawProperty { get; set; }

        /// <summary>
        /// <para type="description">The one or more values to set the object's property to. For radio buttons and dropdown lists, this is the integer found in the 'value' attribute.<para/>
        /// WARNING: If an invalid value is set for a property, minor corruption may occur.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawProperty)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawPropertyManual)]
        public object[] RawValue { get; set; }

        /// <summary>
        /// <para type="description">A collection of parameters for modifying multiple raw properties.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Raw)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawManual)]
        public Hashtable RawParameters { get; set; }

        /// <summary>
        /// <para type="description">Sets an unsafe object property without prompting for confirmation. WARNING: Setting an invalid value for a property can cause minor corruption. Only use if you know what you are doing.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Raw)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.RawManual)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.RawProperty)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.RawPropertyManual)]
        public SwitchParameter Force { get; set; }

        internal override string ProgressActivity => "Modify PRTG Object Settings";

        private PropertyDynamicParameterSet<ObjectProperty> dynamicParams;

        private PropertyParameter[] dynamicParameters;

        private CustomParameter[] parameters;

        private bool IsNormalParameterSet => IsAnyParameterSet(ParameterSet.Default, ParameterSet.Manual);
        private bool IsDynamicParameterSet => IsAnyParameterSet(ParameterSet.Dynamic, ParameterSet.DynamicManual);
        private bool IsRawPropertyParameterSet => IsAnyParameterSet(ParameterSet.RawProperty, ParameterSet.RawPropertyManual);
        private bool IsRawParameterSet => IsAnyParameterSet(ParameterSet.Raw, ParameterSet.RawManual);

        private string BasicShouldProcessMessage => GetShouldProcessMessage(Object, Id);

        /// <summary>
        /// Provides an enhanced one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessingEx()
        {
            if (IsNormalParameterSet)
            {
                //Value is not required, but is required in that we need to explicitly say null
                if (!MyInvocation.BoundParameters.ContainsKey("Value"))
                    throw new ParameterBindingException("Value parameter is mandatory, however a value was not specified. If Value should be empty, specify $null.");

                ParseValue();
            }
            else if (IsDynamicParameterSet)
            {
                dynamicParameters = dynamicParams.GetBoundParameters(this, (p, v) => new PropertyParameter(p, PSObjectUtilities.CleanPSObject(v))).ToArray();

                if (dynamicParameters.Length == 0)
                    throw new ParameterBindingException($"At least one dynamic property or -{nameof(Property)} and -{nameof(Value)} must be specified.");
            }
            else if (IsRawPropertyParameterSet)
            {
                WarnSuspiciousPropertyName(RawProperty);
                RawValue = PSObjectUtilities.CleanPSObject(RawValue);
            }
            else if (IsRawParameterSet)
            {
                parameters = RawParameters.Keys.Cast<object>()
                    .Select(k => new CustomParameter(k.ToString(), PSObjectUtilities.CleanPSObject(RawParameters[k]), ParameterType.MultiParameter))
                    .ToArray();
            }

            base.BeginProcessingEx();
        }

        private void WarnSuspiciousPropertyName(string name)
        {
            if (name.EndsWith("_"))
                return;

            var inherited = Enum.GetValues(typeof(ObjectProperty)).Cast<ObjectProperty>().Where(v => v.GetEnumAttribute<LiteralValueAttribute>() != null).ToArray();
            var raw = inherited.Select(i => ObjectPropertyParser.GetObjectPropertyName(i)).ToArray();

            if (raw.Contains(name))
                return;

            WriteWarning($"Property '{name}' does not look correct. If request does not work try with '{name}_' instead. To suppress this message specify -WarningAction SilentlyContinue.");
        }

        private bool IsAnyParameterSet(params string[] sets)
        {
            return sets.Any(s => ParameterSetName == s);
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (IsNormalParameterSet)
            {
                if (ShouldProcess(BasicShouldProcessMessage, $"{MyInvocation.MyCommand} {Property} = {Value.ToQuotedList()}"))
                    ExecuteOrQueue(Object);
            }
            else if (IsDynamicParameterSet)
            {
                var strActions = dynamicParameters.Select(p => $"{p.Property} = {p.Value.ToQuotedList()}");
                var str = string.Join(", ", strActions);

                if (ShouldProcess(BasicShouldProcessMessage, $"{MyInvocation.MyCommand} {str}"))
                    ExecuteOrQueue(Object);
            }
            else
                ProcessUnsafeRecord();
        }

        private void ProcessUnsafeRecord()
        {
            string continueStr;
            string whatIfStr;

            if (IsRawPropertyParameterSet)
            {
                //RawValue is Mandatory and thus cannot be null (can only be string.Empty)
                var plural = RawValue.Length > 1 ? "s" : "";

                continueStr = $"property '{RawProperty}' to value{plural} {RawValue.ToQuotedList()}";
                whatIfStr = $"{RawProperty} = {RawValue.ToQuotedList()}";
            }
            else if (IsRawParameterSet)
            {
                var vals = parameters.Select(p => $"{p.Name} = {p.Value.ToQuotedList()}");
                whatIfStr = $"{string.Join(", ", vals)}";
                continueStr = $"properties {whatIfStr}";
            }
            else
                throw new NotImplementedException($"Don't know how to handle parameter set '{ParameterSetName}'.");

            var objectNameStr = Object != null ? $"'{Object.Name}'" : $"{("ID".Plural(Id))} {(string.Join(", ", Id))}";

            if (Force || ShouldContinue($"Are you sure you want to set raw object {continueStr} on {TypeDescriptionOrDefault(Object)} {objectNameStr}? This may cause minor corruption if the specified value is not valid for the target property. Only proceed if you know what you are doing.", "WARNING!"))
            {
                if (ShouldProcess(BasicShouldProcessMessage, $"{MyInvocation.MyCommand} {whatIfStr}"))
                    ExecuteOrQueue(Object);
            }
        }

        private void ParseValue()
        {
            var prop = ObjectPropertyParser.GetPropertyInfoViaTypeLookup(Property);
            Value = ParseValueIfRequired(prop.Property, Value);
        }

        /// <summary>
        /// Invokes this cmdlet's action against the current object in the pipeline.
        /// </summary>
        protected override void PerformSingleOperation()
        {
            var ids = GetSingleOperationId(Object, Id);

            if (IsNormalParameterSet)
                ExecuteOperation(() => client.SetObjectProperty(ids, Property, Value), $"Setting object {BasicShouldProcessMessage} setting '{Property}' to {Value.ToQuotedList()}");
            else if (IsDynamicParameterSet)
            {
                var strActions = dynamicParameters.Select(p => $"'{p.Property}' to {p.Value.ToQuotedList()}");
                var str = string.Join(", ", strActions);

                ExecuteOperation(() => client.SetObjectProperty(ids, dynamicParameters), $"Setting object {BasicShouldProcessMessage} setting {str}");
            }
            else
            {
                if (IsRawPropertyParameterSet)
                {
                    var parameter = new CustomParameter(RawProperty, RawValue, ParameterType.MultiParameter);
                    ExecuteOperation(() => client.SetObjectPropertyRaw(ids, parameter), $"Setting object {BasicShouldProcessMessage} setting '{RawProperty}' to {RawValue.ToQuotedList()}");
                }
                else
                {
                    var settingsStr = string.Join(", ", parameters.Select(p => $"{p.Name} = {p.Value.ToQuotedList()}"));

                    ExecuteOperation(() => client.SetObjectPropertyRaw(ids, parameters), $"Setting object {BasicShouldProcessMessage} settings {settingsStr}");
                }
            }
        }

        /// <summary>
        /// Invokes this cmdlet's action against all queued items from the pipeline.
        /// </summary>
        /// <param name="ids">The Object IDs of all queued items.</param>
        protected override void PerformMultiOperation(int[] ids)
        {
            if (IsNormalParameterSet)
                ExecuteMultiOperation(() => client.SetObjectProperty(ids, Property, Value), $"Setting {GetMultiTypeListSummary()} setting '{Property}' to {Value.ToQuotedList()}");
            else if (IsDynamicParameterSet)
            {
                var strActions = dynamicParameters.Select(p => $"'{p.Property}' to {p.Value.ToQuotedList()}");
                var str = string.Join(", ", strActions);

                ExecuteMultiOperation(() => client.SetObjectProperty(ids, dynamicParameters), $"Setting {GetMultiTypeListSummary()} setting {str}");
            }
            else
            {
                if (IsRawPropertyParameterSet)
                {
                    var parameter = new CustomParameter(RawProperty, RawValue, ParameterType.MultiParameter);
                    ExecuteMultiOperation(() => client.SetObjectPropertyRaw(ids, parameter), $"Setting {GetMultiTypeListSummary()} setting '{RawProperty}' to {RawValue.ToQuotedList()}");
                }
                else
                {
                    var settingsStr = string.Join(", ", parameters.Select(p => $"'{p.Name}' = {p.Value.ToQuotedList()}"));

                    ExecuteMultiOperation(() => client.SetObjectPropertyRaw(ids, parameters.ToArray()), $"Setting {GetMultiTypeListSummary()} settings {settingsStr}");
                }
            }
        }

        /// <summary>
        /// Returns the current object that should be passed through this cmdlet.
        /// </summary>
        public override object PassThruObject => Object;

        /// <summary>
        /// Retrieves an object that defines the dynamic parameters of this cmdlet.
        /// </summary>
        /// <returns>An object that defines the dynamic parameters of this cmdlet.</returns>
        public object GetDynamicParameters()
        {
            if (dynamicParams == null)
                dynamicParams = new PropertyDynamicParameterSet<ObjectProperty>(
                    new[] {ParameterSet.Dynamic, ParameterSet.DynamicManual },
                    e => ObjectPropertyParser.GetPropertyInfoViaTypeLookup(e).Property.PropertyType
                );

            return dynamicParams.Parameters;
        }
    }
}
