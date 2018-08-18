using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves properties and settings of a PRTG Object.</para>
    /// 
    /// <para type="description">The Get-ObjectProperty cmdlet retrieves properties and settings of a PRTG Object, as found on the "Settings"
    /// tab within the PRTG UI. Properties retrieved by Get-ObjectProperty may not necessarily be active depending on the value
    /// of their dependent property (e.g. the Interval will not take effect if InheritInterval is $true).</para>
    /// <para type="description">Properties that are not currently supported by Get-ObjectProperty can be retrieved by specifying
    /// the -RawProperty parameter. Raw property names can be found by inspecting the "name" attribute of  the &lt;input/&gt; tag
    /// under the object's Settings page in the PRTG UI. Unlike Set-ObjectProperty, raw property names do not need to include
    /// their trailing underscores when used with Get-ObjectProperty. If a trailing underscore is used, PrtgAPI will automatically truncate it.
    /// When retrieving raw properties please note that PRTG does not support the retrieval of Inheritance related properties via raw lookups.</para>
    /// <para type="description">In order to provide type safety when modifying properties, all properties supported by Set-ObjectProperty
    /// perform a lookup against their corresponding property in Get-ObjectProperty. If the type of value passed to Set-ObjectProperty
    /// does not match the property's expected type, PrtgAPI will attempt to parse the value into the expected type. For more information,
    /// see Set-ObjectProperty.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Device -Id 1001 | Get-ObjectProperty</code>
    ///     <para>Retrieve all properties and settings of the device with ID 1001</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Id 1001 | Get-ObjectProperty -RawProperty name_</code>
    ///     <para>Retrieve the value of raw property "name"</para>
    /// </example>
    /// 
    /// <para type="link">Set-ObjectProperty</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ObjectProperty", DefaultParameterSetName = ParameterSet.Default)]
    public class GetObjectProperty : PrtgProgressCmdlet
    {
        /// <summary>
        /// <para type="description">The object to retrieve properties of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Property)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.RawProperty)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Raw)]
        public PrtgObject Object { get; set; }

        /// <summary>
        /// <para type="description">The name of one or more properties to retrieve. Note: PRTG does not support retrieving inheritance settings in via direct API calls.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Property)]
        public ObjectProperty[] Property { get; set; }

        /// <summary>
        /// <para type="description">The raw name of one or more properties to retrieve. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// Note: PRTG does not support retrieving raw section inheritance settings.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawProperty)]
        public string[] RawProperty { get; set; }

        /// <summary>
        /// <para type="description">Retrieve all raw properties from the target object.</para>
        /// <para type="description">Note: objects may have additional properties that cannot be retrieved via this method.
        /// For more information, see Get-Help about_ObjectProperty</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Raw)]
        public SwitchParameter Raw { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetObjectProperty"/> class.
        /// </summary>
        public GetObjectProperty() : base("Object Properties")
        {
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (ParameterSetName == ParameterSet.Property)
            {
                WriteProperties(Property, client.GetObjectProperty);

                //todo: need tests for this
            }
            else if (ParameterSetName == ParameterSet.Raw)
            {
                var type = TypeFromBase();

                var dictionary = type != null
                    ? client.GetObjectPropertiesRaw(Object.Id, type.Value)
                    : client.GetObjectPropertiesRaw(Object.Id);

                var obj = new PSObject();

                foreach (var val in dictionary)
                {
                    obj.Properties.Add(new PSNoteProperty(val.Key, val.Value));
                }

                WriteObject(obj);
            }
            else if (ParameterSetName == ParameterSet.RawProperty)
            {
                WriteProperties(RawProperty, client.GetObjectPropertyRaw, p => p.ToLower().Replace("_", ""));

                //todo: unit/integration test this
            }
            else
            {
                var knownObj = Object as SensorOrDeviceOrGroupOrProbe;

                if (knownObj != null)
                {
                    TypeDescription = $"{knownObj.BaseType} Properties";

                    switch (knownObj.BaseType)
                    {
                        case BaseType.Sensor:
                            WriteObjectWithProgress(() => client.GetSensorProperties(Object.Id));
                            break;
                        case BaseType.Device:
                            WriteObjectWithProgress(() => client.GetDeviceProperties(Object.Id));
                            break;
                        case BaseType.Group:
                            WriteObjectWithProgress(() => client.GetGroupProperties(Object.Id));
                            break;
                        case BaseType.Probe:
                            WriteObjectWithProgress(() => client.GetProbeProperties(Object.Id));
                            break;
                        default:
                            throw new NotImplementedException($"Property handler not implemented for base type {knownObj.BaseType}");
                    }
                }
                else
                {
                    throw new NotSupportedException($"Typed property handler not implemented for object type {Object.DisplayType}");
                }
            }
        }

        private void WriteProperties<TProperty>(TProperty[] properties, Func<int, TProperty, object> getValue, Func<TProperty, TProperty> getPropertyName = null)
        {
            if (properties.Length == 1)
                WriteObjectWithProgress(() => getValue(Object.Id, properties[0]));
            else
                WriteObjectWithProgress(() => GetMultipleProperties(properties, getValue, getPropertyName));
        }

        private PSObject GetMultipleProperties<TProperty>(TProperty[] properties, Func<int, TProperty, object> getValue, Func<TProperty, TProperty> getPropertyName)
        {
            var obj = new PSObject();

            foreach (var prop in properties)
            {
                var name = getPropertyName != null ? getPropertyName(prop) : prop;
                var val = getValue(Object.Id, prop);

                obj.Properties.Add(new PSNoteProperty(name.ToString(), val));
            }

            return obj;
        }

        [ExcludeFromCodeCoverage]
        private ObjectType? TypeFromBase()
        {
            var obj = Object as SensorOrDeviceOrGroupOrProbe;

            if (obj != null)
            {
                switch (obj.BaseType)
                {
                    case BaseType.Sensor:
                        return ObjectType.Sensor;
                    case BaseType.Device:
                        return ObjectType.Device;
                    case BaseType.Group:
                        return ObjectType.Group;
                    case BaseType.Probe:
                        return ObjectType.Probe;
                    default:
                        throw new NotImplementedException($"Unable to resolve {nameof(ObjectType)} from base type {obj.BaseType}");
                }
            }

            return null;
        }
    }
}
