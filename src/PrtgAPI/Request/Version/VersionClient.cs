using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Linq;
using PrtgAPI.Parameters;
using PrtgAPI.Utilities;

namespace PrtgAPI.Request
{
    internal partial class VersionClient
    {
        internal RequestVersion Version { get; set; }

        protected PrtgClient client;

        internal VersionClient(RequestVersion version, PrtgClient client)
        {
            Version = version;
            this.client = client;
        }

        #region SetChannelProperty

        protected ICollection<Channel> GetChannels(ICollection<Channel> channels, int[] sensorIds, int channelId, CancellationToken token)
        {
            Func<int, List<Channel>> getChannels = (s) => client.GetChannelsInternal(s, idFilter: i => i == channelId, token: token);

            if (channels == null)
            {
                channels = new List<Channel>();

                foreach (var id in sensorIds.Distinct())
                {
                    var result = getChannels(id);

                    if (result.Count == 0)
                        throw new InvalidOperationException($"Channel ID '{channelId}' does not exist on sensor ID '{id}'.");

                    ((List<Channel>) channels).AddRange(result);
                }
            }

            return channels;
        }

        protected async Task<ICollection<Channel>> GetChannelsAsync(ICollection<Channel> channels, int[] sensorIds, int channelId, CancellationToken token)
        {
            Func<int, Task<List<Channel>>> getChannels = async (s) => await client.GetChannelsInternalAsync(s, idFilter: i => i == channelId, token: token).ConfigureAwait(false);

            if (channels == null)
            {
                channels = new List<Channel>();

                foreach (var id in sensorIds.Distinct())
                {
                    var result = await getChannels(id).ConfigureAwait(false);

                    if (result.Count == 0)
                        throw new InvalidOperationException($"Channel ID '{channelId}' does not exist on sensor ID '{id}'.");

                    ((List<Channel>)channels).AddRange(result);
                }
            }

            return channels;
        }

        #endregion
        #region Factor

        private List<SetChannelPropertyGrouping> GroupChannels(ICollection<Channel> channels, ChannelParameter[] channelParameters)
        {
            /* | Scenario | Category       | Channels | Sensors | Result        | Requests |
             * | -------- | -------------- | -------- | ------- | ------------- | -------- |
             * |    1m    | Manual         |    1     |    1    | One channel   |    1     |
             * |    2m    | Manual         |    2     |    1    | IMPOSSIBLE    |    0     |
             * |    3m    | Manual         |    1     |    2    | Same channel  |    1     |
             * |    4m    | Manual         |    2     |    2    | IMPOSSIBLE    |    0     |
             * |    5m    | ManualSpecial  |  Empty Parameters  | Don't resolve channels   |
             * | -------- | -------------- | -------- | ------- | ------------- | -------- |
             * |    1mf   | ManualFactor   |    1     |    1    | One channel   |    1     |
             * |    2mf   | ManualFactor   |    2     |    1    | IMPOSSIBLE    |    0     |
             * |    3mf   | ManualFactor   |    1     |    2    | Many channels |    F#    | F# = Number of Factors. 3 unique factors, 3 requests
             * |    4mf   | ManualFactor   |    2     |    2    | IMPOSSIBLE    |    0     |
             * | -------- | -------------- | -------- | ------- | ------------- | -------- |
             * |    1c    | Channel        |    1     |    1    | One channel   |    1     |
             * |    2c    | Channel        |    2     |    1    | Two channels  |    1     |
             * |    3c    | Channel        |    1     |    2    | Same channel  |    1     |
             * |    4c    | Channel        |    2     |    2    | Many channels |    C#    | C# = Number of Channels. 3 unique channels, 3 unique requests
             * | -------- | -------------- | -------- | ------- | ------------- | -------- |
             * |    1cf   | ChannelFactor  |    1     |    1    | One channel   |    1     |
             * |    2cf   | ChannelFactor  |    2     |    1    | Two channels  |    1     |
             * |    3cf   | ChannelFactor  |    1     |    2    | Many channels |    F#    |
             * |    4cf   | ChannelFactor  |    2     |    2    | Many channels |    F#    |
             * |    5cf   | ChannelSpecial | Empty Parameters   | Treat as 3c   |    1     |
             * | -------- | -------------- | -------- | ------- | ------------- | -------- |
             */

            var groupedBySensor = channels.GroupBy(c => c.SensorId).ToList();

            if (groupedBySensor.Count == 1)
                return GroupChannelsForOneSensor(channelParameters, groupedBySensor.Single());
            else
                return GroupChannelsForMultipleSensors(channelParameters, groupedBySensor);
        }

        #region One Sensor

        private List<SetChannelPropertyGrouping> GroupChannelsForOneSensor(ChannelParameter[] channelParameters, IGrouping<int, Channel> sensorsChannels)
        {
            //1c, 2c, 1cf, 2cf, 5cf

            var groupedByChannel = sensorsChannels.GroupBy(c => c.Id).ToList();

            if (groupedByChannel.Count == 1)
                return GroupChannelsForOneSensorAndChannel(channelParameters, sensorsChannels, groupedByChannel.Single());
            else
                return GroupChannelsForOneSensorAndMultipleChannels(channelParameters, sensorsChannels, groupedByChannel);
        }

        private List<SetChannelPropertyGrouping> GroupChannelsForOneSensorAndChannel(ChannelParameter[] channelParameters, IGrouping<int, Channel> sensorsChannels, IGrouping<int, Channel> channelGroup)
        {
            //1c, 1cf, 5cf

            //At this point, regardless of whether or not all factor properties are null or not (if we even need factors at all) we're going to be executing a single request
            var list = new List<SetChannelPropertyGrouping>();

            //The C# compiler automatically overrides Equals and GetHashCode for anonymous types, so doing Distinct/DistinctBy is a coherent operation
            foreach (var task in GetSetChannelPropertyTasks(channelGroup.DistinctBy(c => new {  c.Id, c.SensorId }).ToList(), channelParameters))
            {
                var parameters = new SetChannelPropertyParameters(new[] { sensorsChannels.Key }, channelGroup.Key, task.Parameters);

                //5cf. If we don't need factors, we're equal to 1c
                if (NeedFactors(parameters, task.Parameters))
                {
                    //Apply the factors to the parameters

                    //We could have the same sensor twice with multiple factors; only accept the first one
                    var factor = task.Channels.GroupBy(c => c.Factor).First();

                    AddFactorProperties(parameters, parameters, factor);
                }

                //One channel on one sensor
                list.Add(new SetChannelPropertyGrouping(parameters, sensorsChannels.ToReadOnly()));
            }

            return list;
        }

        private List<SetChannelPropertyGrouping> GroupChannelsForOneSensorAndMultipleChannels(ChannelParameter[] channelParameters, IGrouping<int, Channel> sensorsChannels, List<IGrouping<int, Channel>> groupedByChannel)
        {
            //2c, 2cf, 5cf

            //One sensor, multiple channels

            SetChannelPropertyParameters masterParameters = null;

            foreach (var channelGroup in groupedByChannel)
            {
                foreach (var task in GetSetChannelPropertyTasks(channelGroup.DistinctBy(c => new { c.Id, c.SensorId }).ToList(), channelParameters))
                {
                    var localParameters = new SetChannelPropertyParameters(new[] { sensorsChannels.Key }, channelGroup.Key, task.Parameters);

                    //5cf
                    if (NeedFactors(localParameters, task.Parameters))
                    {
                        var factors = task.Channels.GroupBy(c => c.Factor);

                        foreach (var factor in factors)
                        {
                            AddFactorProperties(localParameters, localParameters, factor);
                        }
                    }

                    if (masterParameters == null)
                        masterParameters = localParameters;
                    else
                        masterParameters.CustomParameters.AddRange(localParameters.CustomParameters);
                }
            }

            masterParameters.RemoveDuplicateParameters();

            return new List<SetChannelPropertyGrouping>
            {
                new SetChannelPropertyGrouping(masterParameters, groupedByChannel.SelectMany(c => c).ToReadOnly())
            };
        }

        #endregion
        #region Multiple Sensors

        private List<SetChannelPropertyGrouping> GroupChannelsForMultipleSensors(ChannelParameter[] channelParameters, List<IGrouping<int, Channel>> groupedBySensor)
        {
            //3c, 4c, 3cf, 4cf, 5cf

            //Flatten the group back down to an enumeration of channels and see how many distinct IDs we have
            var groupedByChannel = groupedBySensor.SelectMany(g => g).GroupBy(c => c.Id).ToList();

            if (groupedByChannel.Count == 1)
                return GroupChannelsForMultipleSensorsAndOneChannel(channelParameters, groupedByChannel.Single());
            else
                return GroupChannelsForMultipleSensorsAndChannels(channelParameters, groupedByChannel);
        }

        private List<SetChannelPropertyGrouping> GroupChannelsForMultipleSensorsAndOneChannel(ChannelParameter[] channelParameters, IGrouping<int, Channel> channelGroup)
        {
            //3c, 3cf, 5cf

            var list = new List<SetChannelPropertyGrouping>();

            foreach (var task in GetSetChannelPropertyTasks(channelGroup.DistinctBy(c => new { c.Id, c.SensorId }).ToList(), channelParameters))
            {
                var sensorIds = task.Channels.Select(c => c.SensorId).ToArray();

                var tempParameters = new SetChannelPropertyParameters(sensorIds, channelGroup.Key, task.Parameters);

                //5cf
                if (NeedFactors(tempParameters, task.Parameters))
                {
                    //3cf
                    list.AddRange(GetFactorParameters(tempParameters, task.Channels));
                }
                else
                {
                    //3c
                    list.Add(new SetChannelPropertyGrouping(tempParameters, task.Channels));
                }
            }

            return list;
        }

        protected virtual List<SetChannelPropertyTask> GetSetChannelPropertyTasks(List<Channel> channels, ChannelParameter[] channelParameters)
        {
            return new List<SetChannelPropertyTask>
            {
                new SetChannelPropertyTask(channels, channelParameters)
            };
        }

        private List<SetChannelPropertyGrouping> GroupChannelsForMultipleSensorsAndChannels(ChannelParameter[] channelParameters, List<IGrouping<int, Channel>> groupedByChannel)
        {
            //4c, 4cf, 5cf

            var list = new List<SetChannelPropertyGrouping>();

            //Multiple channels across multiple sensors. For small numbers of channels it's concievable it would be better to group per-sensor
            //however generally we will be cutting across multiple sensors using the same channel ID
            foreach (var channelGroup in groupedByChannel)
            {
                foreach (var task in GetSetChannelPropertyTasks(channelGroup.DistinctBy(c => new {c.Id, c.SensorId}).ToList(), channelParameters))
                {
                    //Let's see whether we'll need any factors
                    var tempParameters = new SetChannelPropertyParameters(task.Channels.Select(c => c.SensorId).Distinct().ToArray(), channelGroup.Key, task.Parameters);

                    //5cf
                    if (NeedFactors(tempParameters, task.Parameters))
                    {
                        //4cf
                        list.AddRange(GetFactorParameters(tempParameters, task.Channels));
                    }
                    else
                    {
                        //4c
                        list.Add(new SetChannelPropertyGrouping(tempParameters, task.Channels.ToReadOnly()));
                    }
                }
            }

            return list;
        }

        protected IEnumerable<SetChannelPropertyGrouping> GetFactorParameters(SetChannelPropertyParameters tempParameters, IEnumerable<Channel> channels)
        {
            //Get the factors for all channels with the given ID
            var factors = channels.Select(g => g).DistinctBy(c => new { c.Id, c.SensorId }).GroupBy(c => c.Factor);

            foreach (var factor in factors)
            {
                //4cf
                var factorParameters = tempParameters.ShallowClone();
                factorParameters.SensorIds = factor.Select(c => c.SensorId).ToArray();

                AddFactorProperties(tempParameters, factorParameters, factor);

                yield return new SetChannelPropertyGrouping(factorParameters, factor.ToReadOnly());
            }
        }

        #endregion

        protected virtual bool NeedChannels(SetChannelPropertyParameters parameters, ChannelParameter[] channelParameters)
        {
            return NeedFactors(parameters, channelParameters);
        }

        private bool NeedFactors(SetChannelPropertyParameters parameters, ChannelParameter[] channelParameters)
        {
            if (parameters.FactorParameters.Count > 0)
            {
                var factorProperties = parameters.FactorParameters.Select(p => p.Item1).OfType<ChannelProperty>().ToList();

                //We are inclined to say we need channels. However, if all of the factor parameters found were explicitly
                //specified, and all have a null or empty value, we don't need to include channels
                if (factorProperties.All(p =>
                {
                    var parameter = channelParameters.FirstOrDefault(pa => pa.Property == p);

                    //The parameter was specified and its value was null or empty. Don't need channels for that!
                    if (parameter != null && string.IsNullOrEmpty(parameter.Value?.ToString()))
                        return true;

                    //Either the parameter wasn't explicitly specified (meaning it's someones dependent property) or
                    //its value wasn't null or empty; we'll need to retrieve channels to specify the factor value
                    return false;
                }))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private void AddFactorProperties(SetChannelPropertyParameters outerParameters, SetChannelPropertyParameters innerParameters, IGrouping<double?, Channel> factor)
        {
            if (factor.Key == null)
                return;

            //For each ChannelProperty that has a factor
            foreach (var obj in outerParameters.FactorParameters)
            {
                var prop = obj.Item2.Property;

                //Get the factor property that also needs to be included (e.g. UpperErrorLimitFactor)
                var cache = outerParameters.GetPropertyCache(prop);

                /* If you set a channel like "Available Memory" on a WMI Memory sensor to be a PercentValue
                 * (which doesn't really make sense) then the Factor won't really match. This is kind of an issue
                 * when it comes to deciding how to split our our factor requests (we assume that everyone in a given request has the same factor for all properties)
                 * however this is really a super rare issue so we are not handling it for now */
                //Debug.Assert(factor.All(g => (double?)cache.Property.GetValue(g) == factor.Key), "Generic and specific Factor were not equal for at least one channel");

                //Generate the factor's property name
                var name = outerParameters.GetFactorParameterName(prop, cache);

                //And add it
                innerParameters.CustomParameters.Add(new CustomParameter(name, factor.Key));
            }
        }

        #endregion
        #region ResolveAddressInternal

        internal virtual List<Location> ResolveAddressInternal(string address, CancellationToken token, bool lastAttempt)
        {
            var parameters = new ResolveAddressParameters(address, true);

            var response = client.ObjectEngine.GetObject<GoogleGeoResult>(parameters, ResponseParser.ResolveParser, token);

            if (lastAttempt)
                HandleLastAttempt(response, address);

            return response.Results.Cast<Location>().ToList();
        }

        internal async virtual Task<List<Location>> ResolveAddressInternalAsync(string address, CancellationToken token, bool lastAttempt)
        {
            var parameters = new ResolveAddressParameters(address, true);

            var response = await client.ObjectEngine.GetObjectAsync<GoogleGeoResult>(parameters, m => Task.FromResult(ResponseParser.ResolveParser(m)), token).ConfigureAwait(false);

            if (lastAttempt)
                HandleLastAttempt(response, address);

            return response.Results.Cast<Location>().ToList();
        }

        private void HandleLastAttempt(GoogleGeoResult response, string address)
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                throw new PrtgRequestException($"Could not resolve '{address}' to an actual address: server responded with '{response.ErrorMessage?.EnsurePeriod()} {response.Status}'.");
            }
        }

        #endregion
        #region AddSensorInternal

        //AddSensorInternal

        internal virtual void AddSensorInternal(ICommandParameters internalParams, int index, CancellationToken token)
        {
            if (index == 0)
            {
                //Validate the sensor type
                GetTmpId((int) internalParams[Parameter.Id], internalParams, token);
            }

            client.AddObjectInternalDefault(internalParams, token);
        }

        internal virtual async Task AddSensorInternalAsync(ICommandParameters internalParams, int index, CancellationToken token)
        {
            if (index == 0)
            {
                //Validate the sensor type
                await GetTmpIdAsync((int) internalParams[Parameter.Id], internalParams, token).ConfigureAwait(false);
            }

            await client.AddObjectInternalDefaultAsync(internalParams, token).ConfigureAwait(false);
        }

        #endregion

        private ISensorQueryTargetParameters SynthesizeParameters(ICommandParameters parameters)
        {
            var customParameters = parameters[Parameter.Custom];

            if (customParameters.IsIEnumerable())
            {
                var queryParameters = new SensorQueryTargetParameters();

                var any = false;

                foreach (CustomParameter parameter in customParameters.ToIEnumerable())
                {
                    any = true;

                    queryParameters[parameter.Name] = parameter.Value;
                }

                if (any)
                    return queryParameters;
            }

            return null;
        }
    }
}
