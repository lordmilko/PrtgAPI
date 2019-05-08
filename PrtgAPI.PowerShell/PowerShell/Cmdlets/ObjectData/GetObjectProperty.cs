using System;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves properties and settings of a PRTG Object.</para>
    /// 
    /// <para type="description">The Get-ObjectProperty cmdlet retrieves properties and settings of a PRTG Object, as found on the "Settings"
    /// tab within the PRTG UI. Properties retrieved by Get-ObjectProperty may not necessarily be active depending on the value
    /// of their dependent property (e.g. the Interval will not take effect if InheritInterval is $true).</para>
    /// 
    /// <para type="description">Get-ObjectProperty supports retrieving type safe and raw values. These values can be
    /// retrieved as part of larger property collections (by specifying no parameters or -<see cref="Raw"/>) or as
    /// individual values (-<see cref="Property"/> or -<see cref="RawProperty"/>). When retrieving property collections
    /// Get-ObjectProperty will fail to retrieve the expected properties if the current user has read only access
    /// to the specified object. To retrieve properties from read only objects a specific -<see cref="Property"/> or -<see cref="RawProperty"/> must be specified.</para>
    /// 
    /// <para type="description">Properties that are not currently supported by Get-ObjectProperty can be retrieved by specifying
    /// the -RawProperty parameter. Raw property names can be found by inspecting the "name" attribute of  the &lt;input/&gt; tag
    /// under the object's Settings page in the PRTG UI. Unlike Set-ObjectProperty, raw property names do not need to include
    /// their trailing underscores when used with Get-ObjectProperty. If a trailing underscore is used, PrtgAPI will automatically truncate it.
    /// When retrieving raw properties please note that PRTG does not support the retrieval of Inheritance related properties via raw lookups.</para>
    /// 
    /// <para type="description">By default raw properties will display their values in their "raw" format, i.e. their literal string
    /// value or numeric representation (such as 0 or 1 for an "option" setting). If -<see cref="Text"/> is specified when retrieving an
    /// "option" property, the property's "label" in the PRTG UI will be returned instead of its numeric representation.</para> 
    /// 
    /// <para type="description">Properties can be retrieved by piping in a <see cref="PrtgObject"/> or by specifying a single object ID.
    /// When retrieving by -<see cref="Id"/> individual properties of sub objects (such as Channels and Notification Triggers) can be retrieved by specifying
    /// a -<see cref="SubId"/> as well as -<see cref="RawSubType"/>. As sub objects typically include all properties known to them on their regular objects,
    /// retrieval of individual sub object properties is generally not necessary.</para>
    /// 
    /// <para type="descrption">When retrieving individual properties, Get-ObjectProperty will throw a <see cref="PrtgRequestException"/> if the specified
    /// property is not present on the target object. If the PRTG Server is not in English however, Get-ObjectProperty will return
    /// "(Property not found)" in the PRTG Server's language when a property cannot be found.</para>
    /// 
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
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Id 1001 | Get-ObjectProperty -RawProperty query_ -Text</code>
    ///     <para>Retrieve the "query" field of a REST Custom sensor.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-ObjectProperty -Id 1001 -Raw</code>
    ///     <para>Retrieve all raw properties of the object with ID 1001</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-ObjectProperty -Id 1001 -SubId 1 -RawSubType channel -RawProperty limitmaxerror</code>
    ///     <para>Retrieve the upper error limit property of the channel with ID 1 on the object with ID 1001</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Property-Manipulation#get-1">Online version:</para>
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
        /// <para type="description">The object to retrieve properties for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Default)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Property)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.RawProperty)]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Raw)]
        public PrtgObject Object { get; set; }

        /// <summary>
        /// <para type="description">The ID of the object to retrieve properties for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.PropertyManual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawPropertyManual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawSubPropertyManual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawManual)]
        public int Id { get; set; }

        /// <summary>
        /// <para type="description">The sub ID of the object to retrieve properties for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawSubPropertyManual)]
        public int SubId { get; set; }

        /// <summary>
        /// <para type="description">The type of the sub object to retrieve properties for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawSubPropertyManual)]
        public string RawSubType { get; set; }

        /// <summary>
        /// <para type="description">The name of one or more properties to retrieve. Note: PRTG does not support retrieving inheritance settings in via direct API calls.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = ParameterSet.Property)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.PropertyManual)]
        public ObjectProperty[] Property { get; set; }

        /// <summary>
        /// <para type="description">The raw name of one or more properties to retrieve. This can be typically discovered by inspecting the "name" attribute of the properties' &lt;input/&gt; tag on the Settings page of PRTG.<para/>
        /// Note: PRTG does not support retrieving raw section inheritance settings.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawProperty)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawPropertyManual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawSubPropertyManual)]
        public string[] RawProperty { get; set; }

        /// <summary>
        /// <para type="description">Retrieve all raw properties from the target object.</para>
        /// <para type="description">Note: objects may have additional properties that cannot be retrieved via this method.
        /// For more information, see Get-Help about_ObjectProperty</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Raw)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.RawManual)]
        public SwitchParameter Raw { get; set; }

        /// <summary>
        /// <para type="description">Specifies whether to display option properties using their label names instead their internal numeric values.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.RawProperty)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.RawPropertyManual)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.RawSubPropertyManual)]
        public SwitchParameter Text { get; set; }

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
            switch (ParameterSetName)
            {
                case ParameterSet.Default:              //Object
                    ProcessDefault();
                    break;
                case ParameterSet.Property:             //Object
                case ParameterSet.PropertyManual:       //Id
                    ProcessProperty();
                    break;
                case ParameterSet.Raw:                  //Object
                case ParameterSet.RawManual:            //Id
                    ProcessRaw();
                    break;
                case ParameterSet.RawProperty:          //Object
                case ParameterSet.RawPropertyManual:    //Id
                    ProcessRawProperty(p => client.GetObjectPropertyRaw(GetId(), p, Text));
                    break;
                case ParameterSet.RawSubPropertyManual: //Id
                    ProcessRawProperty(p => client.GetObjectPropertyRaw(GetId(), SubId, RawSubType, p, Text));
                    break;
                default:
                    throw new UnknownParameterSetException(ParameterSetName);
            }
        }

        #region Parameter Sets

        private void ProcessProperty()
        {
            WriteProperties(Property, v => client.GetObjectProperty(GetId(), v));
        }

        private void ProcessRawProperty(Func<string, object> getValue)
        {
            WriteProperties(RawProperty, getValue, p => p.ToLower().Replace("_", ""));
        }

        private void ProcessRaw()
        {
            var type = TypeFromBase();

            var dictionary = type != null
                ? client.GetObjectPropertiesRaw(GetId(), type.Value)
                : client.GetObjectPropertiesRaw(GetId());

            var obj = new PSObject();

            foreach (var val in dictionary)
            {
                obj.Properties.Add(new PSNoteProperty(val.Key, val.Value));
            }

            WriteObject(obj);
        }

        private void ProcessDefault()
        {
            var knownObj = Object as SensorOrDeviceOrGroupOrProbe;

            if (knownObj != null)
            {
                TypeDescription = $"{knownObj.BaseType} Properties";

                switch (knownObj.BaseType)
                {
                    case BaseType.Sensor:
                        WriteObjectWithProgress(() => WarnReadOnly(client.GetSensorProperties(Object.Id)));
                        break;
                    case BaseType.Device:
                        WriteObjectWithProgress(() => WarnReadOnly(client.GetDeviceProperties(Object.Id)));
                        break;
                    case BaseType.Group:
                        WriteObjectWithProgress(() => WarnReadOnly(client.GetGroupProperties(Object.Id)));
                        break;
                    case BaseType.Probe:
                        WriteObjectWithProgress(() => WarnReadOnly(client.GetProbeProperties(Object.Id)));
                        break;
                    default:
                        throw new NotImplementedException($"Property handler not implemented for base type {knownObj.BaseType}.");
                }
            }
            else
            {
                throw new NotSupportedException($"Typed property handler not implemented for object type {Object.DisplayType}.");
            }
        }

        private object WarnReadOnly(object value)
        {
            if (value == null)
                WriteError(new ErrorRecord(
                    new InvalidOperationException($"Cannot retrieve properties for read only object '{Object}' (ID: {Object.Id}). Consider requesting a specific -{nameof(Property)}."),
                    nameof(InvalidOperationException),
                    ErrorCategory.InvalidOperation, null
                ));

            return value;
        }

        void WriteObjectWithProgressSafe(Func<object> obj)
        {
            WriteObjectWithProgress(() =>
            {
                try
                {
                    return obj();
                }
                catch(Exception ex) when (ex is PrtgRequestException)
                {
                    WriteError(new ErrorRecord(
                        ex,
                        nameof(PrtgRequestException),
                        ErrorCategory.InvalidOperation,
                        null
                    ));

                    return null;
                }
            });
        }

        #endregion

        private void WriteProperties<TProperty>(TProperty[] properties, Func<TProperty, object> getValue, Func<TProperty, TProperty> getPropertyName = null)
        {
            if (properties.Length == 1)
                WriteObjectWithProgressSafe(() => getValue(properties[0]));
            else
                WriteObjectWithProgressSafe(() => GetMultipleProperties(properties, getValue, getPropertyName));
        }

        private PSObject GetMultipleProperties<TProperty>(TProperty[] properties, Func<TProperty, object> getValue, Func<TProperty, TProperty> getPropertyName)
        {
            var obj = new PSObject();

            foreach (var prop in properties)
            {
                var name = getPropertyName != null ? getPropertyName(prop) : prop;
                var val = getValue(prop);

                obj.Properties.Add(new PSNoteProperty(name.ToString(), val));
            }

            return obj;
        }

        private int GetId()
        {
            return Object?.Id ?? Id;
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
                        throw new NotImplementedException($"Unable to resolve {nameof(ObjectType)} from base type {obj.BaseType}.");
                }
            }

            return null;
        }
    }
}
