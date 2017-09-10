using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Modifies the value of an object property.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ObjectProperty", SupportsShouldProcess = true)]
    public class SetObjectProperty : PrtgOperationCmdlet
    {
        /// <summary>
        /// <para type="description">The object to modify the properties of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">The property to modify.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Default")]
        public ObjectProperty Property { get; set; }

        /// <summary>
        /// <para type="description">The value to set for the specified property.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = "Default")]
        public object Value { get; set; }

        /// <summary>
        /// <para type="description">The raw name of the property to modify. This can be typically discovered by inspecting the 'name' attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.</para>
        /// This value typically ends in an underscore.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Unsafe")]
        public string RawProperty { get; set; }

        /// <summary>
        /// <para type="description">The value to set the object's property to. For radio buttons and dropdown lists, this is the integer found in the 'value' attribute.<para/>
        /// WARNING: If an invalid value is set for a property, minor corruption may occur.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Unsafe")]
        public string RawValue { get; set; }

        /// <summary>
        /// Sets an unsafe object property without prompting for confirmation. WARNING: Setting an invalid value for a property can cause minor corruption. Only use if you know what you are doing.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "Unsafe")]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ParameterSetName == "Default")
            {
                //Value is not required, but is required in that we need to explicitly say null
                if (!MyInvocation.BoundParameters.ContainsKey("Value"))
                    throw new ParameterBindingException("Value parameter is mandatory, however a value was not specified. If Value should be empty, specify $null");

                var prop = BaseSetObjectPropertyParameters<ObjectProperty>.GetPropertyInfoViaTypeLookup(Property);

                Value = ParseValueIfRequired(prop, Value);

                if (ShouldProcess($"{Object.Name} (ID: {Object.Id})", $"Set-ObjectProperty {Property} = '{Value}'"))
                    ExecuteOperation(() => client.SetObjectProperty(Object.Id, Property, Value), "Modify PRTG Object Settings", $"Setting object {Object.Name} (ID: {Object.Id}) setting {Property} to '{Value}'");
            }
            else
            {
                if (Force || ShouldContinue($"Are you sure you want to set raw object property '{RawProperty}' to value '{RawValue}'? This may cause minor corruption if the specified value is not valid for the target property. Only proceed if you know what you are doing.", "WARNING!"))
                {
                    if (ShouldProcess($"{Object.Name} (ID: {Object.Id})", $"Set-ObjectProperty {Property} = '{Value}'"))
                        ExecuteOperation(() => client.SetObjectPropertyRaw(Object.Id, RawProperty, RawValue), "Modify PRTG Object Settings", $"Setting object {Object.Name} (ID: {Object.Id}) setting {RawProperty} to '{RawValue}'");
                }
            }
        }
    }
}
