using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.PowerShell.Cmdlets;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Request;
using PrtgAPI.Utilities;

namespace PrtgAPI.PowerShell
{
    /// <summary>
    /// Represents one or more parameter sets that pertain to a given sensor type.
    /// </summary>
    class NewSensorDynamicParameterCategory
    {
        public SensorType Type { get; }

        public NewSensorDestinationType DestinationType { get; }

        private PSCmdletInvoker invoker;

        private Func<PSCmdlet> makeCmdlet;
        private Action<dynamic, Dictionary<string, object>> valueFromPipeline;

        private AlternateParameterSet alternateSet;

        internal static Dictionary<string, string> pluralMap;

        private Lazy<ParameterSetDescriptor[]> parameterSets;

        private ParameterSetDescriptor[] ParameterSets => parameterSets.Value;

        static NewSensorDynamicParameterCategory()
        {
            pluralMap = new Dictionary<string, string>();

            pluralMap["Services"] = "Service";
            pluralMap["Tags"] = "Tags";
        }

        public NewSensorDynamicParameterCategory(SensorType type, NewSensorDestinationType destinationType,
            Func<PSCmdlet> makeCmdlet = null, Action<dynamic, Dictionary<string, object>> valueFromPipeline = null)
        {
            parameterSets = new Lazy<ParameterSetDescriptor[]>(GetParameterSets);

            Type = type;
            DestinationType = destinationType;
            this.makeCmdlet = makeCmdlet;
            this.valueFromPipeline = valueFromPipeline;
        }

        public NewSensorDynamicParameterCategory(SensorType type, NewSensorDestinationType destinationType,
            AlternateParameterSet alternateSet)
        {
            parameterSets = new Lazy<ParameterSetDescriptor[]>(GetParameterSets);

            Type = type;
            DestinationType = destinationType;

            this.alternateSet = alternateSet;
        }

        public PSCmdletInvoker GetInvoker(NewSensor newSensorCmdlet)
        {
            if (invoker == null && makeCmdlet != null)
                invoker = new PSCmdletInvoker(newSensorCmdlet, makeCmdlet(), new Lazy<string>(() => QualifiedToUnqualifiedSetName(newSensorCmdlet.ParameterSetName)), valueFromPipeline);

            return invoker;
        }

        /// <summary>
        /// Indicates whether this category contains the parameter set that is active in a given cmdlet.
        /// </summary>
        /// <param name="newSensorCmdlet">The cmdlet to check parameter sets against.</param>
        /// <returns>If any of the parameter sets in this category matched the cmdlet's parameter set, true. Otherwise, false.</returns>
        public bool HasParameterSet(NewSensor newSensorCmdlet)
        {
            GetInvoker(newSensorCmdlet); //Force initialize the invoker for the parameter sets

            return ParameterSets.Any(s => newSensorCmdlet.ParameterSetName == $"{Type}{s.Name}");
        }

        public ParameterSetDescriptor GetParameterSet(NewSensor newSensorCmdlet)
        {
            GetInvoker(newSensorCmdlet); //Force initialize the invoker for the parameter sets

            return ParameterSets.First(s => $"{Type}{s.Name}" == newSensorCmdlet.ParameterSetName);
        }

        public void AddDynamicParameters(NewSensor newSensorCmdlet, RuntimeDefinedParameterDictionaryEx dictionary)
        {
            GetInvoker(newSensorCmdlet); //Force initialize the invoker for the parameter sets

            var parametersType = Type.GetEnumAttribute<TypeAttribute>(true);

            //Don't add parameters for types that don't have them
            if (parametersType.Class == null)
                return;

            AddParametersObjectDynamicParameters(dictionary);

            if (makeCmdlet != null)
                AddCmdletDynamicParameters(newSensorCmdlet, dictionary);

            AddSensorTypeDynamicParameter(dictionary);
            AddDestinationParameter(dictionary);
        }

        /// <summary>
        /// Creates dynamic parameters for all parameters defined on a <see cref="NewSensorParameters"/> object.
        /// </summary>
        /// <param name="dictionary">The dynamic parameters dictionary to add parameters to.</param>
        private void AddParametersObjectDynamicParameters(RuntimeDefinedParameterDictionaryEx dictionary)
        {
            //If TypeAttribute.Class is null (i.e. creating the parameters are not actually supported) a SwitchParameter wouldn't have been created
            //and we won't actually get to this point
            var parametersType = Type.GetEnumAttribute<TypeAttribute>(true);

            var properties = ReflectionCacheManager.Get(parametersType.Class).Properties.Where(p => p.GetAttribute<PropertyParameterAttribute>() != null);

            var parameterConfig = Type.GetEnumAttribute<NewSensorAttribute>() ?? new NewSensorAttribute();

            int? position = null;

            foreach (var property in properties)
            {
                var parameterAttributes = new List<Attribute>();

                var isNameParameter = property.Property.Name == nameof(NewObjectParameters.Name);

                position = GetPosition(property, isNameParameter, parameterConfig, position);

                foreach (var set in ParameterSets.Where(s => PropertyIsAllowedInSet(property, s)))
                {
                    parameterAttributes.Add(new ParameterAttribute
                    {
                        Mandatory = GetMandatory(property, isNameParameter, parameterConfig),
                        Position = position.Value,
                        ParameterSetName = $"{Type}{set.Name}"
                    });
                }

                var name = GetParameterName(property);

                var type = property.Property.PropertyType;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var underlying = type.GetGenericArguments()[0];
                    type = underlying.MakeArrayType();
                }

                dictionary.AddOrMerge(name, type, parameterAttributes.ToArray());

                if (alternateSet != null)
                {
                    IAlternateParameter alternateParameter;

                    if (TryGetAlternateProperty(property, alternateSet.Name, out alternateParameter))
                    {
                        var parameterAttribute = new ParameterAttribute
                        {
                            Mandatory = HasRequireValueTrue(property),
                            Position = position.Value,
                            ParameterSetName = $"{Type}{alternateSet.Name}"
                        };

                        dictionary.AddOrMerge(alternateParameter.Name, alternateParameter.Type, parameterAttribute);
                    }
                }
            }
        }

        private int GetPosition(PropertyCache property, bool isNameParameter, NewSensorAttribute parameterConfig, int? position)
        {
            if (isNameParameter)
            {
                if (parameterConfig.DynamicName)
                    return int.MinValue;

                return 1;
            }

            if (property.GetAttribute<RequireValueAttribute>()?.ValueRequired == true)
            {
                //If we have an invoker, don't give positions to required values in case the type of parameter
                //at this position conflicts with the parameter that might be used in the cmdlet parameter set
                if (makeCmdlet != null)
                    return int.MinValue;

                if (parameterConfig.DynamicName)
                {
                    if (position == null)
                        position = 1; //Set it to the first position after the SensorType SwitchParameter
                    else
                        position++;
                }
                else
                {
                    if (position == null)
                        position = 2; //Set it to be the first position after the SensorType SwitchParameter and Name
                    else
                        position++;
                }

                return position.Value;
            }

            return int.MinValue;
        }

        private bool GetMandatory(PropertyCache property, bool isNameParameter, NewSensorAttribute parameterConfig)
        {
            if (parameterConfig.ConfigOptional)
                return false;

            if (isNameParameter)
            {
                if (parameterConfig.DynamicName)
                    return false;

                return true;
            }

            return HasRequireValueTrue(property);
        }

        private bool TryGetAlternateProperty(PropertyCache property, string parameterSet, out IAlternateParameter parameter)
        {
            if (alternateSet != null && alternateSet.Name == parameterSet)
            {
                var matchingParameter = alternateSet.Parameters.FirstOrDefault(p => p.OriginalName == property.Property.Name);

                if (matchingParameter != null)
                {
                    parameter = matchingParameter;
                    return true;
                }
            }

            parameter = null;
            return false;
        }

        private string GetParameterName(PropertyCache property)
        {
            if (property.Property.Name.EndsWith("s") && typeof(IEnumerable).IsAssignableFrom(property.Property.PropertyType) && property.Property.PropertyType != typeof(string))
            {
                string value;

                if (pluralMap.TryGetValue(property.Property.Name, out value))
                    return value;

                throw new NotImplementedException($"Don't know how to de-pluralize '{property.Property.Name}'.");
            }

            return property.Property.Name;
        }

        private void AddCmdletDynamicParameters(NewSensor newSensorCmdlet, RuntimeDefinedParameterDictionaryEx dictionary)
        {
            var properties = ReflectionCacheManager.Get(GetInvoker(newSensorCmdlet).CmdletToInvoke.GetType()).Properties
                .Where(p => p.GetAttribute<ParameterAttribute>() != null).ToList();

            var highestPosition = GetHighestParameterPosition(dictionary);

            foreach (var property in properties)
            {
                var description = property.GetAttribute<DescriptionAttribute>();

                var name = description?.Description ?? property.Property.Name;

                //Not cached so we can modify it
                var attributes = Attribute.GetCustomAttributes(property.Property, typeof(ParameterAttribute)).ToList();

                foreach (ParameterAttribute attribute in attributes)
                {
                    attribute.ParameterSetName = $"{Type}{attribute.ParameterSetName}";

                    if (attribute.Position >= 0 && highestPosition != null)
                        attribute.Position += highestPosition.Value + 1;
                }

                var aliases = property.GetAttribute<AliasAttribute>();

                if (aliases != null && !aliases.AliasNames.Contains(name))
                    attributes.Add(aliases);

                dictionary.AddOrMerge(name, property.Property.PropertyType, attributes.ToArray());
            }
        }

        private int? GetHighestParameterPosition(RuntimeDefinedParameterDictionaryEx dictionary)
        {
            var invocationSets = ParameterSets.Where(set => set.Invoke).Select(set => set.Name).ToList();

            var specifiedPositions = dictionary.SelectMany(p => p.Value.Attributes.OfType<ParameterAttribute>())
                .Where(a => invocationSets.Contains(QualifiedToUnqualifiedSetName(a.ParameterSetName)) && a.Position >= 0).ToArray();

            if (specifiedPositions.Length == 0)
                return null;
                
            return specifiedPositions.Max(a => a.Position);
        }

        private string QualifiedToUnqualifiedSetName(string name)
        {
            return name.Substring(Type.ToString().Length);
        }

        private void AddSensorTypeDynamicParameter(RuntimeDefinedParameterDictionaryEx dictionary)
        {
            var attributes = ParameterSets.Select(set => new ParameterAttribute
            {
                Mandatory = true,
                ParameterSetName = $"{Type}{set.Name}",
                Position = 0 //Force it to the front so it displays first in PowerShell help
            }).ToArray();

            dictionary.AddOrMerge(Type.ToString(), typeof(SwitchParameter), attributes);
        }

        private void AddDestinationParameter(RuntimeDefinedParameterDictionaryEx dictionary)
        {
            var type = DestinationType == NewSensorDestinationType.DestinationId ? typeof(int) : typeof(Device);

            var attributes = ParameterSets.Select(set => new ParameterAttribute
            {
                Mandatory = true,
                ValueFromPipeline = DestinationType == NewSensorDestinationType.Device,
                ParameterSetName = $"{Type}{set.Name}"
            }).ToArray();

            dictionary.AddOrMerge(DestinationType.ToString(), type, attributes);
        }

        private bool HasRequireValueTrue(PropertyCache property)
        {
            var attrib = property.GetAttribute<RequireValueAttribute>();

            if (attrib != null)
                return attrib.ValueRequired;

            return false;
        }

        private bool PropertyIsAllowedInSet(PropertyCache property, ParameterSetDescriptor set)
        {
            if (set.ExcludedParameters.Length == 0)
                return true;

            var attrib = property.GetAttribute<PropertyParameterAttribute>();

            if (set.ExcludedParameters.Contains((attrib.Property)))
                return false;

            return true;
        }

        private ParameterSetDescriptor[] GetParameterSets()
        {
            if (makeCmdlet == null)
            {
                var setDescriptors = new List<ParameterSetDescriptor>();
                setDescriptors.Add(new ParameterSetDescriptor(ParameterSet.PropertyManual));

                if (alternateSet != null)
                    setDescriptors.Add(new ParameterSetDescriptor(alternateSet.Name, false, GetAlternateSetExcludedParameters()));

                return setDescriptors.ToArray();
            }

            if (invoker == null)
                Debug.Fail($"Invoker cannot be null in {nameof(GetParameterSets)} when {nameof(makeCmdlet)} is not null");

            //Get all the parameter sets defined on the cmdlet that will be invoked
            var cmdletProperties = ReflectionCacheManager.Get(invoker.CmdletToInvoke.GetType()).Properties
                .Where(p => p.GetAttribute<ParameterAttribute>() != null).ToList();

            var sets = cmdletProperties
                .SelectMany(p => p.GetAttributes<ParameterAttribute>())
                .Select(a => a.ParameterSetName)
                .Where(n => n != ParameterAttribute.AllParameterSets)
                .Distinct()
                .Select(s =>
                {
                    if (Type == SensorType.Factory)
                        return new ParameterSetDescriptor(s, true, ObjectProperty.ChannelDefinition);
                    else
                        throw new NotImplementedException($"Don't know what cmdlet based parameter set descriptor to use for sensor type '{Type}'.");
                })
                .ToList();

            if (alternateSet != null)
                sets.Add(new ParameterSetDescriptor(alternateSet.Name));

            //Normal set not utilizing any cmdlet parameters
            sets.Add(new ParameterSetDescriptor(ParameterSet.PropertyManual));

            return sets.ToArray();
        }

        private Enum[] GetAlternateSetExcludedParameters()
        {
            if (Type == SensorType.WmiService)
                return new Enum[] { Parameter.Service };

            throw new NotImplementedException($"Don't know what alternate set to use for sensor type '{Type}'.");
        }

        public bool TryGetSensorTarget(NewSensor newSensorCmdlet, PrtgClient client, ref string propertyName, ref object propertyValue)
        {
            if (alternateSet != null)
            {
                var name = propertyName;

                var matchingParameter = alternateSet.Parameters.OfType<AlternateSensorTargetParameter>().FirstOrDefault(p => p.OriginalName == name);

                //If this is true, we are processing a sensor type that had an unbound parameter ("Services") that also had a replacement
                //sensor target parameter ("ServiceName"). Invoke Get-SensorTarget using the device the sensor will be created on,
                //the type of sensor we're creating and the value that was specified for the alternate target parameter (the specified ServiceName wildcards)
                if (matchingParameter != null)
                {
                    var invoker = new PSCmdletInvoker(
                        newSensorCmdlet,
                        new GetSensorTarget(),
                        new Lazy<string>(() => ParameterSet.Default),
                        (c, b) => c.Device = (Device)b[nameof(GetSensorTarget.Device)]
                    );

                    var boundParameters = new Dictionary<string, object>();
                    boundParameters[nameof(GetSensorTarget.Type)] = Type;
                    boundParameters[nameof(GetSensorTarget.Name)] = newSensorCmdlet.MyInvocation.BoundParameters[matchingParameter.Name];

                    invoker.BindParameters(boundParameters);
                    invoker.BeginProcessing(boundParameters);
                    invoker.ProcessRecord(newSensorCmdlet.MyInvocation.BoundParameters);

                    propertyName = matchingParameter.Name;
                    propertyValue = invoker.Output;

                    return true;
                }
            }

            return false;
        }

        internal void ValidateParameters(NewSensor newSensorCmdlet, NewSensorParameters parameters)
        {
            try
            {
                RequestParser.ValidateObjectParameters(parameters);
            }
            catch (InvalidOperationException ex)
            {
                var property = Regex.Replace(ex.Message, ".*[Pp]roperty '(.+?)'.+", "$1");

                var candidates = new List<string>();

                string singular;

                //Build a list of possible parameter values that could have been invoked
                if (pluralMap.TryGetValue(property, out singular))
                    candidates.Add(singular);

                if (alternateSet != null)
                    candidates.AddRange(alternateSet.Parameters.Where(p => p.OriginalName == property).Select(p => p.Name));

                //Filter the list to the parameter that was actually specified
                candidates = candidates.Where(c => newSensorCmdlet.MyInvocation.BoundParameters.ContainsKey(c)).ToList();

                if (candidates.Count == 1)
                {
                    var newMessage = ex.Message.Replace("Property", "Parameter").Replace($"'{property}'", $"'-{candidates[0]}'");

                    throw new InvalidOperationException(newMessage, ex);
                }
                else
                    throw;
            }
        }
    }
}