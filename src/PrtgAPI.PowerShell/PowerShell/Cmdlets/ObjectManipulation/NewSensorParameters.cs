using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Targets;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Creates a new set of sensor parameters for creating a brand new sensor under a device.</para>
    /// 
    /// <para type="description">The New-SensorParameters cmdlet creates a set of parameters for adding a brand
    /// new sensor to PRTG. All sensor types supported by PRTG can be added with New-SensorParameters, however certain
    /// types are "natively" supported. For types that are not are natively supported, New-SensorParameters is capable of
    /// either dynamically generating the required set of parameters, or creating a completely custom set of parameters from
    /// a collection of values manually crafted by hand.</para>
    /// 
    /// <para type="description">Natively supported sensor types allow interfacing with strongly typed properties
    /// of a well known object deriving from NewSensorParameters. When a supported type is created, the name to give the
    /// sensor can be optionally specified. If a name is not specified, New-SensorParameters will automatically assign
    /// the sensor the default name PRTG would assign a sensor of the specified type (e.g. EXE/Script Advanced sensors
    /// by default are named "XML Custom EXE/Script Sensor").</para>
    /// 
    /// <para type="description">In addition to the sensor name, certain sensor types contain additional mandatory fields
    /// that must be populated before attempting to add the sensor (such as the ExeFile of an EXE/Script Advanced sensor).
    /// New-SensorParameters optionally allows you to specify the value of the primary mandatory field of the specified
    /// type using the -Value parameter. Fields that require values contain a value of $null by default, however not all
    /// fields that are $null are necessarily mandatory. If you attempt to add a natively supported type with missing
    /// mandatory fields, PrtgAPI will catch this and alert you that the value that was missing.</para>
    /// 
    /// <para type="description">Beyond type safety, PrtgAPI does not perform any validation
    /// that the values you specify to fields are "correct" (e.g. for an EXE/Script Advanced sensor, that the specified
    /// file exists). In the event invalid values are specified, PRTG will usually handle the error gracefully, however you
    /// are responsible for confirming that any values that are used to create a new sensor as are as correct as possible.</para>
    /// 
    /// <para type="description">For sensor types that are not natively supported, PrtgAPI provides the ability to dynamically generate
    /// the set of parameters required to add the specified sensor type. Dynamically generated sensor parameters operate as a hybrid of a
    /// both a Dictionary and a PSObject, allowing you to interface with these types as if they are a native object. For sensor types that require
    /// additional information be provided before retrieving their sensor parameters, a -<see cref="QueryTarget"/> or a set of -<see cref="QueryParameters"/>
    /// must be specified. New-SensorParameters will automatically advise you what should be provided for these parameters if it determines these
    /// values are required by the specified sensor type.</para>
    ///
    /// <para type="description">For sensor types that require Sensor Targets, a dictionary of all identified sensor targets can be found
    /// under the dynamic sensor parameter's Targets property. Parameters that appear to contain sensor targets will automatically be wrapped
    /// as a <see cref="GenericSensorTarget"/>, and by default will contain the first target from the list of available candidates.</para>
    /// 
    /// <para type="description">By default, dynamically sensor parameters are "locked", as to prevent additional parameters from being
    /// added to the object in the event a typo is made. If you do wish to add additional parameters however, this can be performed by calling the
    /// Unlock method on the specified <see cref="DynamicSensorParameters"/> and then setting the value via the parameter's indexer or via
    /// the dynamic property name. Note that <see cref="DynamicSensorParameters"/> dynamic properties will always show parameters as not
    /// containing a trailing underscore as to provide a "cleaner" interface. The raw <see cref="CustomParameter"/> objects of the parameters
    /// can be viewed however by specifying <see cref="Parameter.Custom"/> to the object's indexer.</para> 
    /// 
    /// <para type="description">If you wish to create your parameters yourself, this can either be done by creating an -Empty set of parameters
    /// for you to manually populate, or by defining a hashtable listing all the parameters that are required to create the specified sensor
    /// along with their raw associated values. The parameters for creating a specific type of sensor can be discovered via a web debugger
    /// such as Fiddler or by inspecting the underlying parameters generated by a set of <see cref="DynamicSensorParameters"/>.</para>
    /// 
    /// <para type="description">When accessing <see cref="RawSensorParameters"/> and <see cref="DynamicSensorParameters"/> via their indexers,
    /// by default PowerShell will suppress any exceptions thrown when trying to access non-existent properties, instead simply returning $null.
    /// This is due to the default Strict Mode the PowerShell engine runs under. To enable exceptions on accessing invalid paramters, the strict
    /// mode must be set to version 3 or higher. For more information, see Get-Help Set-StrictMode.</para>
    /// 
    /// <para type="description">Great care should be taken when adding sensors using raw parameters. As there is no type safety,
    /// the possibility of making errors is high. As most raw parameter names end in an underscore, it is critical to ensure
    /// these parameters have been named properly. In the event a sensor is added improperly, it can easily be corrected or
    /// deleted in the PRTG UI. When specifying a hashtable parameter set to New-SensorParameters, PrtgAPI will validate
    /// that at a minimum the 'name_' and 'sensortype' parameters are specified. If either of these two are missing,
    /// New-SensorParameters will generate an exception.</para>
    /// 
    /// <para type="description">When sensor parameters are created via the Add-Sensor cmdlet, if the -Resolve parameter is specified
    /// PrtgAPI will attempt to resolve the resultant sensors based on the sensor type specified in the sensor parameters. If the resulting sensor
    /// type is capable of changing based on the specified parameters, you can instruct PrtgAPI to broaden its sensor resolution search by specifying
    /// the -<see cref="DynamicType"/> parameter when creating your parameters object. <see cref="DynamicType"/> property can also be modified on the resulting
    /// <see cref="NewSensorParameters"/> object that is returned from the New-SensorParameters cmdlet.
    /// </para>
    /// 
    /// <para type="description">All sensor parameter types support specifying common sensor parameters (Inherit Triggers, Interval, Priority, etc)
    /// via well typed properties. If these properties are not set, PRTG will automatically use the default values for these fields based on the
    /// type of sensor being created.</para>
    /// 
    /// <example>
    ///     <code>
    ///         C:\> $params = New-SensorParameters ExeXml "Custom Script" "CustomScript.ps1"
    ///
    ///         C:\> Get-Device dc-1 | Add-Sensor $params
    ///     </code>
    ///     <para>Create a new EXE/Script Advanced sensor on the device dc-1 using the name "Custom Script", that executes the file "CustomScript.ps1", specifying the script name in the optional -Value parameter</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = New-SensorParameters ExeXml "Custom Script"
    ///         C:\> $params.ExeFile = "CustomScript.ps1"
    ///
    ///         C:\> Get-Device dc-1 | Add-Sensor $params
    ///     </code>
    ///     <para>Create a new EXE/Script Advanced sensor on the device dc-1 using the name "Custom Script", that executes the file "CustomScript.ps1", specifying the script name after the object has been created</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = New-SensorParameters ExeXml
    ///         C:\> $params.ExeFile = "CheckStatus.ps1"
    ///
    ///         C:\> Get-Device -Id 1001 | Add-Sensor $params
    ///     </code>
    ///     <para>Create a new EXE/Script Advanced sensor on the device with ID 1001 using the name "XML Custom EXE/Script Sensor" that executes the file "CheckStatus.ps1"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = Get-Device esxi-1 | New-SensorParameters -RawType vmwaredatastoreextern
    ///         C:\> $params.datafieldlist__check = $params.Targets["datafieldlist__check"]
    ///
    ///         C:\> $params | Add-Sensor
    ///     </code>
    ///     <para>Dynamically create a new set of VMware Datastore credentials for the device named esxi-1 targeting all datastores that exist on the device.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = Get-Device esxi-1 | New-SensorParameters -RawType vmwaredatastoreextern
    ///         C:\> $params[[PrtgAPI.Parameter]::Custom]
    ///     </code>
    ///     <para>View the raw set of CustomParameters defined on an object.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = Get-Device esxi-1 | New-SensorParameters -RawType exchangepsdatabase
    ///         C:\> $params.Unlock()
    /// 
    ///         C:\> $params.customparam_ = "some value"
    ///     </code>
    ///     <para>Create a new parameter named "customparam" on a set of Exchange Database sensor parameters.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $params = New-SensorParameters -Empty
    ///         C:\> $params["name_"] = "My Sensor"
    ///         C:\> $params["sensortype"] = "exexml"
    ///     </code>
    ///     <para>Create an empty set of sensor parameters to manually insert all raw parameters.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $raw = @{
    ///         >>      name_ = "my raw sensor"
    ///         >>      tags_ = "xmlexesensor"
    ///         >>      priority_ = 4
    ///         >>      exefile_ = "CustomScript.ps1|CustomScript.ps1||
    ///         >>      exeparams_ = "arg1 arg2 arg3"
    ///         >>      environment_ = 1
    ///         >>      usewindowsauthentication_ = 1
    ///         >>      mutexname_ = "testMutex"
    ///         >>      timeout_ = 70
    ///         >>      writeresult_ = 1
    ///         >>      intervalgroup = 0
    ///         >>      interval_ = "30|30 seconds"
    ///         >>      errorintervalsdown_ = 2
    ///         >>      sensortype = "exexml"
    ///         >> }
    /// 
    ///         C:\> $params = New-SensorParameters $raw
    /// 
    ///         C:\> Get-Device dc-1 | Add-Sensor $params
    ///     </code>
    ///     <para>Add a new EXE/Script Advanced sensor to the device named dc-1 using a hashtable containing its raw parameters.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $params = New-SensorParameters $raw -DynamicType</code>
    ///     <para>Create a set of parameters for creating a sensor with a dynamic type (such as snmplibrary).</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Set-StrictMode -Version 3</code>
    ///     <para>Set the Strict Mode to version 3 for the current PowerShell session.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $params = Get-Device -Id 1001 | New-SensorParameters -RawType snmplibrary -qt *ups*</code>
    ///     <para>Create a set of parameters for creating a SNMP Library sensor utilizing a wildcard expression that matches the sensor query target "APC UPS.oidlib".</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $target = (Get-SensorType snmplibrary -Id 1001).QueryTargets | where Value -like *ups*
    ///         C:\> $params = Get-Device -Id 1001 | New-SensorParameters -RawType snmplibrary -qt $target
    ///     </code>
    ///     <para>Create a set of parameters for creating a SNMP Library sensor utilizing the sensor query target "APC UPS.oidlib".</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> $params = Get-Device -Id 1001 | New-SensorParameters -RawType ipmisensor -qp @{ username = "admin"; password = "password" }</code>
    ///     <para>Create a set of parameters for creating an IPMI Sensor specifying the query target parameters required to authenticate to IPMI.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Object-Creation#parameters">Online version:</para>
    /// <para type="link">Get-Help SensorParameters</para>
    /// <para type="link">Add-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="linl">New-Sensor</para>
    /// <para type="link">Set-StrictMode</para>
    /// </summary>
    [Cmdlet(VerbsCommon.New, "SensorParameters", DefaultParameterSetName = ParameterSet.Default)]
    public class NewSensorParametersCommand : PrtgProgressCmdlet
    {
        /// <summary>
        /// <para type="description">The type of sensor to create.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Default, Position = 0)]
        public SensorType Type { get; set; }

        /// <summary>
        /// <para type="description">The name to give the new sensor. If no value is specified, the default name of the specified sensor type will be used.
        /// If the specified sensor type does not support specifying a name, this field is used for any mandatory values required by the sensor type.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Default, Position = 1)]
        public object First { get; set; }

        /// <summary>
        /// <para type="description">A mandatory value required by the specified sensor type.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Default, Position = 2)]
        public object Second { get; set; }

        /// <summary>
        /// <para type="description">A collection of raw parameters for adding an unsupported sensor type.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Raw, Position = 0)]
        public Hashtable RawParameters { get; set; }

        /// <summary>
        /// <para type="description">The device to create a set of <see cref="DynamicSensorParameters"/> from.</para> 
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Dynamic, ValueFromPipeline = true)]
        public Device Device { get; set; }

        /// <summary>
        /// <para type="description">The raw name of the sensor type to create.</para> 
        /// </summary>
        [Alias("rt")]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Dynamic)]
        public string RawType { get; set; }

        /// <summary>
        /// <para type="description">Wildcard used to specify the sensor targets to assign to a set of <see cref="DynamicSensorParameters"/>.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Dynamic)]
        public string Target { get; set; }

        /// <summary>
        /// <para type="description">Duration (in seconds) to wait for dynamic sensor parameters to resolve.</para> 
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Dynamic)]
        public int Timeout { get; set; } = 60;

        /// <summary>
        /// <para type="description">A sensor query target to use when retrieving dynamic sensor parameters. Can include wildcards.</para>
        /// </summary>
        [Alias("qt")]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Dynamic)]
        public SensorQueryTarget QueryTarget { get; set; }

        /// <summary>
        /// <para type="description">A set of sensor query target parameters to use when retrieving dynamic sensor parameters.</para>
        /// </summary>
        [Alias("qp")]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Dynamic)]
        public Hashtable QueryParameters { get; set; }

        /// <summary>
        /// <para type="description">Specifies that an empty set of <see cref="RawSensorParameters"/> should be returned to allow constructing
        /// a parameter set manually.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Empty)]
        public SwitchParameter Empty { get; set; }

        /// <summary>
        /// <para type="description">Specifies whether the resulting sensor type is dynamically determined by the parameters included in the request.
        /// If this property is true, PrtgAPI will relax its sensor resolution mechanism to ensure the resultant object is retrieved.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Raw)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Dynamic)]
        public SwitchParameter DynamicType { get; set; }

        private const string NameParameter = "name_";
        private const string SensorTypeParameter = "sensortype";

        /// <summary>
        /// Initializes a new instance of the <see cref="NewSensorParametersCommand"/> class.
        /// </summary>
        public NewSensorParametersCommand() : base("Sensor Parameters")
        {
        }

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            First = PSObjectUtilities.CleanPSObject(First);
            Second = PSObjectUtilities.CleanPSObject(Second);

            if (ParameterSetName == ParameterSet.Dynamic)
                base.BeginProcessing();
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            NewSensorParameters parameters;

            if (ParameterSetName == ParameterSet.Dynamic)
            {
                WriteProcessProgressRecords(f => CreateDynamicParameters(i => f(i, $"Probing target device ({i}%)")));

                return;
            }
            if (ParameterSetName == ParameterSet.Empty)
            {
                parameters = new PSRawSensorParameters("fake_name", "fake_sensortype")
                {
                    Parameters = new List<CustomParameter>()
                };

                parameters.GetParameters().Remove(Parameter.SensorType);
            }
            else
            {
                parameters = ParameterSetName == ParameterSet.Raw ? CreateRawParameters() : CreateTypedParameters();

                var attrib = Type.GetEnumAttribute<NewSensorAttribute>() ?? new NewSensorAttribute();

                if (!string.IsNullOrWhiteSpace(First?.ToString()) && !attrib.DynamicName)
                    parameters.Name = First.ToString();
            }

            WriteObject(parameters);
        }

        private NewSensorParameters CreateRawParameters()
        {
            if (!RawParameters.ContainsKey(NameParameter))
                throw new InvalidOperationException($"Hashtable record '{NameParameter}' is mandatory, however a value was not specified.");

            if (!RawParameters.ContainsKey(SensorTypeParameter))
                throw new InvalidOperationException($"Hashtable record '{SensorTypeParameter}' is mandatory, however a value was not specified.");

            var parameters = new PSRawSensorParameters(RawParameters[NameParameter]?.ToString(), RawParameters[SensorTypeParameter]?.ToString());

            var toAdd = RawParameters.Keys.Cast<object>()
                    .Where(k => k.ToString() != SensorTypeParameter)
                    .Select(k => new CustomParameter(k.ToString(), PSObjectUtilities.CleanPSObject(RawParameters[k]), ParameterType.MultiParameter))
                    .ToList();

            foreach (var param in toAdd)
            {
                parameters[param.Name] = param.Value;
            }

            if (DynamicType)
                parameters.DynamicType = true;

            return parameters;
        }

        internal static SensorMultiQueryTargetParameters GetQueryTargetParameters(PrtgClient client, int deviceId, string sensorType, SensorQueryTarget queryTarget, Hashtable queryParametersRaw)
        {
            queryTarget = GetQueryTarget(client, deviceId, sensorType, queryTarget);
            var queryParameters = GetQueryParameters(queryParametersRaw);

            if (queryTarget != null || queryParameters != null)
                return new SensorMultiQueryTargetParameters(queryTarget, queryParameters);

            return null;
        }

        private static SensorQueryTarget GetQueryTarget(PrtgClient client, int deviceId, string sensorType, SensorQueryTarget queryTarget)
        {
            if (queryTarget != null && queryTarget.Value.Contains("*"))
            {
                var allTypes = client.GetSensorTypes(deviceId);
                var desiredType = allTypes.FirstOrDefault(t => string.Equals(t.Id, sensorType, StringComparison.OrdinalIgnoreCase));

                if (desiredType != null)
                {
                    if (desiredType.QueryTargets == null)
                        return queryTarget; //Type does not support query targets; leave it to internal engine to throw exception

                    var wildcard = new WildcardPattern(queryTarget.Value, WildcardOptions.IgnoreCase);
                    var candidates = desiredType.QueryTargets.Where(a => wildcard.IsMatch(a.Value)).ToList();

                    if (candidates.Count == 1)
                        return candidates.Single();
                    else if (candidates.Count > 1)
                        throw new NonTerminatingException($"Query target wildcard '{queryTarget}' is ambiguous between the following parameters: {candidates.ToQuotedList()}. Please specify a more specific identifier.");
                    else
                        throw new NonTerminatingException($"Could not find a query target matching the wildcard expression '{queryTarget}'. Please specify one of the following parameters: {desiredType.QueryTargets.ToQuotedList()}.");
                }
            }

            return queryTarget;
        }

        private static SensorQueryTargetParameters GetQueryParameters(Hashtable queryParametersRaw)
        {
            if (queryParametersRaw == null)
                return null;

            var queryParameters = new SensorQueryTargetParameters();

            foreach (var key in queryParametersRaw.Keys.Cast<object>())
                queryParameters[key.ToString()] = PSObjectUtilities.CleanPSObject(queryParametersRaw[key]);

            return queryParameters;
        }

        private DynamicSensorParameters CreateDynamicParameters(Func<int, bool> progressCallback)
        {
            var multiQuery = GetQueryTargetParameters(client, Device.Id, RawType, QueryTarget, QueryParameters);

            var dynamicParamters = client.GetDynamicSensorParameters(Device, RawType, progressCallback, Timeout, multiQuery, CancellationToken);
            dynamicParamters.Source = Device;

            if (!string.IsNullOrEmpty(Target) && dynamicParamters.Targets.Count > 0)
            {
                if (dynamicParamters.Targets.Count == 1)
                {
                    var wildcard = new WildcardPattern(Target, WildcardOptions.IgnoreCase);

                    var target = dynamicParamters.Targets.First();

                    var targets = target.Value.Where(t => wildcard.IsMatch(t.Name)).ToArray();

                    if (targets.Length == 1)
                        dynamicParamters[target.Key] = targets.First();
                    else
                        dynamicParamters[target.Key] = targets;
                }
                else
                {
                    throw new NonTerminatingException("Cannot filter targets as multiple target fields are present. Please filter targets manually after parameter creation.");
                }
            }

            if (DynamicType)
                dynamicParamters.DynamicType = true;

            return dynamicParamters;
        }

        private NewSensorParameters CreateTypedParameters()
        {
            NewSensorParameters parameters;

            switch (Type)
            {
                case SensorType.ExeXml:
                    parameters = new ExeXmlSensorParameters("FAKE_VALUE")
                    {
                        ExeFile = GetImplicit<ExeFileTarget>(Second)
                    };
                    break;
                case SensorType.Http:
                    var httpParameters = new HttpSensorParameters();
                    MaybeSet(Second, v => httpParameters.Url = v?.ToString());
                    parameters = httpParameters;
                    break;
                case SensorType.WmiService:
                    parameters = new WmiServiceSensorParameters(new List<WmiServiceTarget>())
                    {
                        Services = GetList<WmiServiceTarget>(First)
                    };
                    break;
                case SensorType.Factory:
                    parameters = new FactorySensorParameters(Enumerable.Empty<string>())
                    {
                        ChannelDefinition = GetList<string>(Second)?.ToArray()
                    };
                    break;
                default:
                    throw new NotImplementedException($"Sensor type '{Type}' is currently not supported.");
            }

            return parameters;
        }

        [ExcludeFromCodeCoverage]
        private T GetImplicit<T>(object val)
        {
            if (val == null)
                return default(T);

            if (val is T)
                return (T) val;

            var implicitOp = typeof (T).GetMethod("op_Implicit", new[] {typeof (string)});

            if (implicitOp == null)
                throw new InvalidOperationException($"Object type {typeof (T)} does not contain an implicit operator for objects of type string.");

            return (T) implicitOp.Invoke(null, new object[] {val.ToString()});
        }

        [ExcludeFromCodeCoverage]
        private List<T> GetList<T>(object val)
        {
            if (val == null)
                return null;

            if (val is PSObject)
                val = ((PSObject) val).BaseObject;

            if (val is T)
                return new List<T> {(T)val };

            if (val is List<T>)
                return (List<T>)val;

            if (val.IsIEnumerable())
            {
                var objList = val.ToIEnumerable().Where(o => o != null);

                var list = new List<T>();

                foreach (var obj in objList)
                {
                    var obj1 = obj;

                    if (obj1 is PSObject)
                        obj1 = ((PSObject) obj).BaseObject;

                    if (obj1 is T)
                        list.Add((T)obj1);
                    else
                        throw new ArgumentException($"Expected one or more items of type {typeof (T)}, however an item of type {obj1.GetType()} was specified.");
                }

                return list;
            }

            throw new ArgumentException($"Expected one or more items of type {typeof (T)}, however an item of type {val.GetType()} was specified.");
        }

        private void MaybeSet(object v, Action<object> set)
        {
            if (!string.IsNullOrEmpty(v?.ToString()))
                set(v);
        }
    }
}
