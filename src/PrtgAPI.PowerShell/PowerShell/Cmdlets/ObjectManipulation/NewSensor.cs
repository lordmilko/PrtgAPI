using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Management.Automation;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters;
using PrtgAPI.Parameters.Helpers;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;
using IDynamicParameters = System.Management.Automation.IDynamicParameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Adds a new sensor to a PRTG Device.</para>
    ///
    /// <para type="description">The New-Sensor cmdlet adds a new sensor to a PRTG Device. Unlike the similar Add-Sensor cmdlet,
    /// New-Sensor provides the ability to specify the properties of each sensor type as dynamic parameters directly to the cmdlet.
    /// As such, New-Sensor can only be used for creating sensor types that are properly supported by PrtgAPI.</para>
    ///
    /// <para type="description">All parameter sets found on New-Sensor adhere to the following basic rules:
    /// 1. The first parameter is always a SwitchParameter specifying the sensor type.
    /// 2. The second parameter is always the Name (if applicable).
    /// 3. The -Device parameter specifies where the sensor will be created (unless otherwise specified).
    /// 4. Required values are positional parameters, unless they would conflict with another parameter set.
    /// 5. Unless otherwise specified, all other parameters are named parameters.</para>
    ///
    /// <para type="description">Sensor types that require the use of complex sensor targets can either specify the target
    /// objects directly or specify one or more wildcard expressions, which New-Sensor will use to lookup these targets for you.
    /// Note that if you specify a wildcard expression when trying to create sensors on multiple objects, New-Sensor will resolve
    /// these targets on each individual target, which could be time consuming.</para>
    ///
    /// <para type="description">When creating sensor factory objects, New-Sensor incorporates the functionality of the New-SensorFactoryDefinition
    /// cmdlet, allowing you to generate a complex sensor factory definition from several specified sensors. Unlike other sensor types,
    /// the device the sensor factory should be created on is specified via the -DestinationId parameter, allowing the sensors to use
    /// for the factory to be piped into the cmdlet.</para>
    ///
    /// <para type="description">To view the parameters of your sensor without creating it, you can specify the -WhatIf parameter. This can be especially
    /// useful when creating sensor factories to confirm the channel definition was correctly generated as expected.</para>
    ///
    /// <example>
    ///     <code>C:\> Get-Device -Id 1001 | New-Sensor -ExeXml test test.ps1 -Mutex mutex1</code>
    ///     <para>Create a new EXE/Script Advanced sensor named "test" that executes the script "test.ps1" with
    /// property "Mutex" set to "mutex1" on the device with ID 1001.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Id 1001 | New-Sensor -Http</code>
    ///     <para>Create a new HTTP sensor on the device with ID 1001. The Name and Url of the sensor will use their default values ("HTTP" and "https://" respectively).</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device exch-1 | New-Sensor -WmiService *exchange*</code>
    ///     <para>Create WMI Service sensors for all Exchange services on the device named exch-1.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>
    ///         C:\> $targets = Get-Device exch-1 | Get-SensorTarget WmiService *exchange*
    ///         C:\> Get-Device *exch* | New-Sensor -WmiService $targets
    ///     </code>
    ///     <para>Create the same WMI Service sensors for all Exchange servers.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Tags wmicpu* | New-Sensor -Factory "CPU Overview" { $_.Device } -sn "Average CPU Usage" -se Average -DestinationId 1001</code>
    ///     <para>Create a sensor factory for showing the CPU Usage of all sensors on the device with ID 1001, including a summary channel showing the average CPU Usage of all devices.</para>
    /// </example>
    /// 
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Object-Creation#simple">Online version:</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Add-Sensor</para>
    /// <para type="link">New-SensorParameters</para>
    /// <para type="link">Get-SensorTarget</para>
    /// <para type="link">New-SensorFactoryDefinition</para>
    /// </summary>
    [Cmdlet(VerbsCommon.New, "Sensor", SupportsShouldProcess = true)]
    public class NewSensor : AddObject<NewSensorParameters, Sensor>, IDynamicParameters
    {
        private bool IsDeviceSensor
        {
            get
            {
                var hasDevice = MyInvocation.BoundParameters.ContainsKey(NewSensorDestinationType.Device.ToString());

                if (hasDevice)
                    return true;

                var hasDestinationId = MyInvocation.BoundParameters.ContainsKey(NewSensorDestinationType.DestinationId.ToString());

                if (hasDestinationId)
                    return false;

                //Someone piped an empty list of devices into the cmdlet

                //todo: unit test
                return true;
            }
        } 

        private bool IsDestinationIdSensor => MyInvocation.BoundParameters.ContainsKey(NewSensorDestinationType.DestinationId.ToString());

        private static ConcurrentDictionary<Type, Delegate> castSensorTargetCache = new ConcurrentDictionary<Type, Delegate>();

        private NewSensorDynamicParameterContainer dynamicParams;

        private List<Tuple<string, object>> modifiedSensorParameters = new List<Tuple<string, object>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NewSensor"/> class.
        /// </summary>
        public NewSensor() : base(BaseType.Sensor)
        {
        }

        #region Cmdlet Overrides

        /// <summary>
        /// Provides an enhanced one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessingEx()
        {
            dynamicParams.InvokeBeginProcessing(MyInvocation.BoundParameters);

            base.BeginProcessingEx();
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (IsDeviceSensor)
            {
                ExecuteOperation(() =>
                {
                    dynamicParams.InvokeProcessRecord();

                    CreateDeviceSensor(false);
                }, ProgressMessage, false, false);
            }
            else
            {
                dynamicParams.InvokeProcessRecord();
            }
        }

        /// <summary>
        /// Provides an enhanced one-time, postprocessing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessingEx()
        {
            if (!InvokeCommand.HasErrors && !IsDeviceSensor)
            {
                ExecuteWithCoreState(() =>
                {
                    dynamicParams.InvokeEndProcessing();

                    CreateDestinationIdSensor(true);
                });
            }

            base.EndProcessingEx();
        }

        /// <summary>
        /// Interrupts the currently running code to signal the cmdlet has been requested to stop.
        /// </summary>
        [ExcludeFromCodeCoverage]
        protected override void StopProcessingEx()
        {
            dynamicParams.InvokeStopProcessing();

            base.StopProcessingEx();
        }

        #endregion
        #region AddObject

        internal override int DestinationId
        {
            get
            {
                var type = dynamicParams.ActiveSensorType.DestinationType;

                switch (type)
                {
                    case NewSensorDestinationType.DestinationId:
                        return Convert.ToInt32(MyInvocation.BoundParameters[type.ToString()]);
                    case NewSensorDestinationType.Device:
                        return ((Device) MyInvocation.BoundParameters[type.ToString()]).Id;
                    default:
                        throw new NotImplementedException($"Don't know how to handle destination type '{type}'.");
                }
            }
        }

        internal override string ShouldProcessAction
        {
            get
            {
                switch (dynamicParams.ActiveSensorType.Type)
                {
                    case SensorType.Factory:
                        return GetParameterActionDescription(ObjectProperty.ChannelDefinition);
                    default:
                        return GetParameterActionDescription();
                }
            }
        }

        private string GetParameterActionDescription(params ObjectProperty[] additionalParameters)
        {
            var items = new List<string>();

            foreach (var prop in modifiedSensorParameters
                .Where(c => !additionalParameters.Any(p => p.ToString() == c.Item1)) //Exclude any additionalParameters; we'll print them specially below
                .OrderBy(c => c.Item1 != nameof(NewObjectParameters.Name)))          //Ensure property "Name" comes first (if applicable)
            {
                var val = prop.Item2.IsIEnumerable() ? string.Join(", ", prop.Item2.ToIEnumerable()) : prop.Item2?.ToString();

                items.Add($"{prop.Item1} = '{val}'");
            }

            foreach (var param in additionalParameters)
            {
                var name = ObjectPropertyParser.GetObjectPropertyName(param);

                var val = Parameters.GetCustomParameterInternal(name)?.ToString();

                if (val?.Contains("\n") == true)
                    items.Add($"{param} =\n\n{val}\n\n");
                else
                    items.Add($"{param} = '{val}'");
            }

            return MyInvocation.MyCommand.Name + ": " + string.Join(", ", items);
        }

        internal override string ShouldProcessTarget
        {
            get
            {
                var type = dynamicParams.ActiveSensorType.DestinationType;

                switch (type)
                {
                    case NewSensorDestinationType.DestinationId:
                        return $"Device ID: {DestinationId}";
                    case NewSensorDestinationType.Device:

                        var device = ((Device) MyInvocation.BoundParameters[type.ToString()]);

                        return $"'{device.Name}' (ID: {device.Id})";
                    default:
                        throw new NotImplementedException($"Don't know how to handle destination type '{type}'.");
                }
            }
        }

        internal override string ProgressMessage
        {
            get
            {
                object str = string.Empty;

                if (MyInvocation.BoundParameters.TryGetValue("Name", out str))
                    str = $"'{str}' ";

                switch (dynamicParams.ActiveSensorType.DestinationType)
                {
                    case NewSensorDestinationType.DestinationId:
                        return $"Adding {dynamicParams.ActiveSensorType.Type} sensor {str}to device ID '{DestinationId}'";
                    case NewSensorDestinationType.Device:
                        return $"Adding {dynamicParams.ActiveSensorType.Type} sensor {str}to device '{((Device)MyInvocation.BoundParameters["Device"]).Name}'";
                    default:
                        throw new NotImplementedException($"Don't know how to handle destination type '{type}'.");
                }
            }
        }

        /// <summary>
        /// Resolves the children of the destination object that match the new object's name.
        /// </summary>
        /// <param name="filters">An array of search filters used to retrieve all children of the destination with the specified name.</param>
        /// <returns>All objects under the parent object that match the new object's name.</returns>
        protected override List<Sensor> GetObjects(SearchFilter[] filters) => client.GetSensors(filters);

        #endregion
        #region Dynamic Parameters

        /// <summary>
        /// Retrieves an object that defines the dynamic parameters of this cmdlet.
        /// </summary>
        /// <returns>An object that defines the dynamic parameters of this cmdlet.</returns>
        public object GetDynamicParameters()
        {
            try
            {
                if (dynamicParams == null)
                    dynamicParams = new NewSensorDynamicParameterContainer(this, client);

                return dynamicParams.Parameters;
            }
            catch(Exception ex)
            {
                Debug.Assert(false, $"An exception occurred while trying to create dynamic parameters: {ex.Message}");

                throw;
            }
        }

        #endregion

        private void CreateDeviceSensor(bool endOperation)
        {
            if (IsDestinationIdSensor)
                return;

            SensorParametersInternal parameters;

            switch (dynamicParams.ActiveSensorType.Type)
            {
                case SensorType.ExeXml:
                    parameters = new ExeXmlSensorParameters("FAKE_VALUE");
                    break;
                case SensorType.WmiService:
                    parameters = new WmiServiceSensorParameters(Enumerable.Empty<Targets.WmiServiceTarget>());
                    break;
                case SensorType.Http:
                    parameters = new HttpSensorParameters();
                    break;
                default:
                    throw new NotImplementedException($"Don't know how to create a sensor of type '{dynamicParams.ActiveSensorType.Type}'.");
            }

            BindParametersAndAddSensor(parameters, endOperation);
        }

        private void CreateDestinationIdSensor(bool endOperation)
        {
            if (IsDeviceSensor)
                return;

            SensorParametersInternal parameters;

            switch (dynamicParams.ActiveSensorType.Type)
            {
                case SensorType.Factory:
                    var channelDefinition = dynamicParams.IsInvokableParameterSet ?
                        dynamicParams.ActiveSensorType.GetInvoker(this).Output.Select(o => o.ToString()):
                        new string[] {};

                    parameters = new FactorySensorParameters(channelDefinition);
                    break;
                default:
                    throw new NotImplementedException($"Don't know what needs to be done to process DestinationId sensor type '{dynamicParams.ActiveSensorType.Type}'.");
            }

            BindParametersAndAddSensor(parameters, endOperation);
        }

        private void BindParametersAndAddSensor(SensorParametersInternal parameters, bool endOperation)
        {
            //Bind all parameter properties that were specified

            var properties = ReflectionCacheManager.Get(parameters.GetType()).Properties.Where(p => p.GetAttribute<PropertyParameterAttribute>() != null);

            foreach (var property in properties)
            {
                object propertyValue;

                var propertyName = property.Property.Name;

                string pluralName;

                //Get the de-pluralized name of the property
                if (NewSensorDynamicParameterCategory.pluralMap.TryGetValue(propertyName, out pluralName))
                    propertyName = pluralName;

                //Was the de-pluralized name specified? If not, how about the sensor target replacement for the original pluralized name?
                if (MyInvocation.BoundParameters.TryGetValue(propertyName, out propertyValue))
                {
                    propertyValue = MaybeRewrapList(property, propertyValue);

                    modifiedSensorParameters.Add(Tuple.Create(propertyName, propertyValue));

                    property.SetValue(parameters, propertyValue);
                }
                else
                {
                    var name = property.Property.Name;

                    if (dynamicParams.ActiveSensorType.TryGetSensorTarget(this, client, ref name, ref propertyValue))
                    {
                        propertyValue = MaybeRewrapList(property, propertyValue);

                        modifiedSensorParameters.Add(Tuple.Create(name, propertyValue));

                        propertyValue = CastSensorTarget(property.Property.PropertyType, (List<object>) propertyValue);

                        property.SetValue(parameters, propertyValue);
                    }
                }
            }

            dynamicParams.ActiveSensorType.ValidateParameters(this, parameters);

            Parameters = parameters;
            AddObjectInternal(DestinationId, endOperation);
        }

        internal void AddObjectInternal(int destinationId, bool endOperation)
        {
            if (ShouldProcess(ShouldProcessTarget, ShouldProcessAction))
            {
                ExecuteOperation(() => ExecuteOperationAction(destinationId), ProgressMessage, !endOperation, !endOperation);
            }
        }

        private object MaybeRewrapList(PropertyCache property, object propertyValue)
        {
            if (propertyValue == null)
                return null;

            var type = property.Property.PropertyType;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var propertyType = propertyValue.GetType();

                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
                    return propertyValue;

                var list = propertyValue.ToIEnumerable().ToList();

                return CastSensorTarget(type, list);
            }

            return propertyValue;
        }

        private object CastSensorTarget(Type listType, List<object> propertyValue)
        {
            var type = listType.GetGenericArguments()[0];

            Delegate compiled;

            if (castSensorTargetCache.TryGetValue(type, out compiled))
                return compiled.DynamicInvoke(propertyValue);

            var parameter = Expression.Parameter(typeof(List<object>), "list");

            var castRaw = typeof(Enumerable).GetMethod("Cast", BindingFlags.Static | BindingFlags.Public);
            var toListRaw = typeof(Enumerable).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public);

            var cast = castRaw.MakeGenericMethod(type);
            var toList = toListRaw.MakeGenericMethod(type);

            var result = Expression.Call(null, toList,
                Expression.Call(null, cast, parameter)
            );

            var lambda = Expression.Lambda(result, parameter);

            compiled = lambda.Compile();

            castSensorTargetCache[type] = compiled;

            return compiled.DynamicInvoke(propertyValue);
        }
    }
}
