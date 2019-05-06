using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using PrtgAPI.Attributes;
using PrtgAPI.Parameters;
using PrtgAPI.Reflection;
using PrtgAPI.Utilities;

namespace PrtgAPI.Request
{
    static class RequestParser
    {
        #region Notifications

        internal static TriggerChannel GetTriggerChannel(TriggerParameters parameters)
        {
            TriggerChannel channel = null;

            switch (parameters.Type)
            {
                case TriggerType.Speed:
                    channel = ((SpeedTriggerParameters)parameters).Channel;
                    break;
                case TriggerType.Volume:
                    channel = ((VolumeTriggerParameters)parameters).Channel;
                    break;
                case TriggerType.Threshold:
                    channel = ((ThresholdTriggerParameters)parameters).Channel;
                    break;
            }

            return channel;
        }

        /// <summary>
        /// Extracts the XML of a single notification action from an XML document containing many notification actions.
        /// </summary>
        /// <param name="normal">The XML to extract the notification action from</param>
        /// <param name="properties">The properties to extract</param>
        /// <param name="id">The ID of the notification action to extract the properties for.</param>
        /// <returns>A XML document containing the specified properties of the specified object.</returns>
        internal static XDocument ExtractActionXml(XDocument normal, XElement properties, int id)
        {
            var thisDoc = new XDocument(normal);
            var items = thisDoc.Descendants("item");
            items.Where(i => i.Element("objid").Value != id.ToString()).Remove();

            var item = items.SingleOrDefault();

            //No item when notification triggers tell us about a notification
            //action we don't have permission to access
            if (item == null)
                return null;
            else
                item.Add(properties.Nodes());
            
            return thisDoc;
        }

        #endregion
        #region Add Objects

        internal static bool IsAddSensor(ICommandParameters parameters) => parameters.Function == CommandFunction.AddSensor5;

        internal static SearchFilter[] GetFilters(Either<IPrtgObject, int> parent, NewObjectParameters parameters)
        {
            var filters = new List<SearchFilter>()
            {
                new SearchFilter(Property.ParentId, parent.GetId())
            };

            if (parameters is NewSensorParameters)
            {
                //When creating new sensors, PRTG may dynamically assign a name based on the sensor's parameters.
                //As such, we instead filter for sensors of the newly created type
                var sensorType = parameters[Parameter.SensorType];

                var str = sensorType is SensorType ? ((Enum)sensorType).EnumToXml() : sensorType?.ToString();

                if (!((NewSensorParameters)parameters).DynamicType)
                    filters.Add(new SearchFilter(Property.Type, str?.ToLower() ?? string.Empty));
            }
            else
                filters.Add(new SearchFilter(Property.Name, parameters.Name));

            return filters.ToArray();
        }

        internal static List<KeyValuePair<Parameter, object>> ValidateObjectParameters(NewObjectParameters parameters)
        {
            var propertyCaches = parameters.GetType().GetNormalProperties().ToList();

            foreach (var cache in propertyCaches)
            {
                var requireValue = cache.GetAttribute<RequireValueAttribute>();

                if (requireValue != null && requireValue.ValueRequired)
                    ValidateRequiredValue(cache.Property, parameters);

                var dependency = cache.GetAttribute<DependentPropertyAttribute>();

                if (dependency != null)
                    ValidateDependentProperty(dependency, cache.Property, parameters);
            }

            var lengthLimit = parameters.GetParameters().Where(p => p.Key.GetEnumAttribute<LengthLimitAttribute>() != null).ToList();

            return lengthLimit;
        }

        private static void ValidateRequiredValue(PropertyInfo property, NewObjectParameters parameters, DependentPropertyAttribute attrib = null)
        {
            var val = property.GetValue(parameters);

            var dependentStr = attrib != null ? $" when property '{attrib.Property}' is value '{attrib.RequiredValue}'" : "";

            if (string.IsNullOrWhiteSpace(val?.ToString()))
            {
                throw new InvalidOperationException($"Property '{property.Name}' requires a value{dependentStr}, however the value was null, empty or whitespace.");
            }

            var list = val as IEnumerable;

            if (list != null && !(val is string))
            {
                var casted = list.Cast<object>().ToList();

                if (!casted.Any())
                    throw new InvalidOperationException($"Property '{property.Name}' requires a value, however an empty collection was specified.");

                if (casted.All(c => string.IsNullOrWhiteSpace(c?.ToString())))
                    throw new InvalidOperationException($"Property '{property.Name}' requires a value, however a collection consisting of null, empty or whitespace values was specified.");
            }
        }

        private static void ValidateDependentProperty(DependentPropertyAttribute attrib, PropertyInfo property, NewObjectParameters parameters)
        {
            var target = parameters.GetType().GetProperty(attrib.Property.ToString()).GetValue(parameters);

            if (target.ToString() == attrib.RequiredValue.ToString())
                ValidateRequiredValue(property, parameters, attrib);
        }

        internal static void ValidateAddSensorQueryTarget(List<SensorTypeDescriptor> types, BeginAddSensorQueryParameters parameters)
        {
            var thisType = types.FirstOrDefault(t => string.Equals(t.Id, parameters.OriginalType, StringComparison.OrdinalIgnoreCase));

            if (thisType == null)
            {
                if (parameters.QueryTarget == null)
                    throw new InvalidOperationException($"Cannot process query for sensor type '{parameters.OriginalType}': sensor type '{parameters.OriginalType}' is not valid.");

                throw new InvalidOperationException($"Failed to validate query target '{parameters.QueryTarget}' on sensor type '{parameters.OriginalType}': sensor type '{parameters.OriginalType}' is not valid.");
            }

            if (parameters.QueryTarget == null)
            {
                if (thisType.QueryTargets != null && thisType.QueryTargets.Count > 0)
                    throw new InvalidOperationException($"Failed to process query for sensor type '{parameters.OriginalType}': a sensor query target is required, however none was specified. Please specify one of the following targets: {thisType.QueryTargets.ToQuotedList()}.");
                else
                    return;
            }

            if (thisType.QueryTargets == null || thisType.QueryTargets.Count == 0)
                throw new InvalidOperationException($"Cannot specify query target '{parameters.QueryTarget}' on sensor type '{parameters.OriginalType}': type does not support query targets.");

            var matchingArgument = thisType.QueryTargets.FirstOrDefault(a => string.Equals(a.Value, parameters.QueryTarget.Value, StringComparison.OrdinalIgnoreCase));

            if (matchingArgument == null)
            {
                throw new InvalidOperationException($"Query target '{parameters.QueryTarget}' is not a valid target for sensor type '{parameters.OriginalType}' on device ID {parameters.ObjectId}. Please specify one of the following targets: {thisType.QueryTargets.ToQuotedList()}.");
            }

            parameters.QueryTarget = matchingArgument;
        }

        internal static ICommandParameters GetInternalNewObjectParameters(Either<IPrtgObject, int> deviceOrId, NewObjectParameters parameters)
        {
            var newParams = new CommandFunctionParameters(parameters.Function);

            foreach (var param in parameters.GetParameters())
            {
                newParams[param.Key] = param.Value;
            }

            newParams[Parameter.Id] = deviceOrId.GetId();

            return newParams;
        }

        #endregion
        #region System Administration

        [ExcludeFromCodeCoverage]
        internal static CommandFunction GetLoadSystemFilesFunction(ConfigFileType fileType)
        {
            if (fileType == ConfigFileType.General)
                return CommandFunction.ReloadFileLists;
            if (fileType == ConfigFileType.Lookups)
                return CommandFunction.LoadLookups;

            throw new NotImplementedException($"Don't know how to handle file type '{fileType}'.");
        }

        #endregion

        internal static bool IsLongLat(string address, out Location location) =>
            IsLongLat(address, false, out location);

        private static bool IsLongLat(string address, bool findLabel, out Location location)
        {
            var label = findLabel ? GetLocationLabel(ref address) : null;

            var str = Regex.Replace(address, "\\r\\n|\\r|\\n", " ", RegexOptions.Singleline);
            str = Regex.Replace(str, "\\{(.*)\\}", string.Empty);

            var matches = Regex.Matches(str, "-?\\d+\\.\\d+");

            if (matches.Count == 2)
            {
                double latitude;
                double longitude;

                if (double.TryParse(matches[0].Value, out latitude) && double.TryParse(matches[1].Value, out longitude))
                {
                    if (!findLabel && (address.Contains('\n') || address.Contains('\r')))
                    {
                        //There exists a set of coordinates in this address. Could we have acquired these same coordinates if we were looking
                        //for labels?
                        if (IsLongLat(address, true, out location))
                            return true;
                    }

                    //Either there exists a set of coordinates in this address even when searching for a label (findLabel = true),
                    //or we tried searching for a label but didn't find one (findLabel = false)
                    location = new GpsLocation(latitude, longitude);

                    //Apply the label (if one was found)
                    if (!string.IsNullOrWhiteSpace(label))
                        location.Label = label;

                    return true;
                }
            }

            location = null;
            return false;
        }

        internal static string GetLocationLabel(ref string address)
        {
            var primary = '\n';
            var secondary = '\r';

            var index = address.IndexOf(primary);

            if (index == -1)
            {
                //If we're on OSX maybe only a carriage return was used

                var tmp = primary;
                primary = secondary;
                secondary = tmp;

                index = address.IndexOf(primary);
            }

            if (index != -1)
            {
                var label = address.Substring(0, index);

                var proposedAddress = address.Substring(index + 1);

                if (string.IsNullOrWhiteSpace(proposedAddress))
                    return null;
                else
                    address = proposedAddress;

                return label.Replace(secondary.ToString(), "");
            }

            return null;
        }

        internal static Tuple<PropertyParameter[], PropertyParameter[]> GetSetObjectPropertyParamLists(PropertyParameter[] @params)
        {
            var normalParams = new List<PropertyParameter>();
            var mergeableParams = new List<PropertyParameter>();

            foreach (var param in @params)
            {
                var attrib = param.Property.GetEnumAttribute<MergeableAttribute>();

                if (attrib == null)
                    normalParams.Add(param);
                else
                    mergeableParams.Add(param);
            }

            return Tuple.Create(normalParams.ToArray(), mergeableParams.ToArray());
        }

        internal static PropertyParameter[] MergeParameters(Tuple<PropertyParameter[], PropertyParameter[]> paramLists)
        {
            var originalParams = paramLists.Item1;
            var mergeableParams = paramLists.Item2;

            if (mergeableParams.Length > 0)
            {
                var newParams = originalParams.ToList();

                foreach (var mergee in mergeableParams)
                {
                    var attrib = mergee.Property.GetEnumAttribute<MergeableAttribute>();

                    var dependency = originalParams.FirstOrDefault(p => p.Property == attrib.Dependency);

                    if (dependency == null)
                        throw new InvalidOperationException($"{nameof(ObjectProperty)} '{mergee.Property}' must be used in conjunction with property '{attrib.Dependency}', however a value for property '{attrib.Dependency}' was not specified.");

                    var replacement = attrib.Merge(mergee, dependency);

                    var index = newParams.IndexOf(dependency);
                    newParams[index] = replacement;
                }

                return newParams.ToArray();
            }
            else
                return originalParams;
        }
    }
}
