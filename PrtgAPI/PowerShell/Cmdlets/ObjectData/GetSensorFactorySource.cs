using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using PrtgAPI.Objects.Shared;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves source sensors and channels from a PRTG Sensor Factory.</para>
    /// 
    /// <para type="description">The Get-SensorFactorySource cmdlet retrieves the source sensors and channels
    /// of PRTG Sensor Factory objects. By default Get-SensorFactorySource retrieves the sensors of the channel definition.
    /// If the -Channels parameter is specified, the cmdlet will retrieve the source channels instead. If a sensor is passed
    /// to Get-SensorFactorySource that isn't a Sensor Factory, an ParameterBindingException will be thrown.</para>
    /// 
    /// <example>
    ///     <code>flt type eq "sensor factory" | Get-Sensor -Count 1 | Get-SensorFactorySource</code>
    ///     <para>Retrieve all source sensors of a single sensor factory sensor</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>flt type eq "sensor factory" | Get-Sensor -Count 1 | Get-SensorFactorySource -Channels</code>
    ///     <para>Retrieve all source channels of a single sensor factory sensor</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">New-SearchFilter</para>
    /// 
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "SensorFactorySource")]
    public class GetSensorFactorySource : PrtgProgressCmdlet
    {
        /// <summary>
        /// <para type="description">The sensor factory sensor to retrieve source sensors or channels of.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public Sensor Sensor { get; set; }

        /// <summary>
        /// <para type="description">Whether to retrieve the source <see cref="Channel"/> objects of a sensor factory, instead of the source sensors.</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public SwitchParameter Channels { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSensorFactorySource"/> class.
        /// </summary>
        public GetSensorFactorySource() : base(string.Empty)
        {
        }

        /// <summary>
        /// Performs record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            if (Sensor.Type != "Sensor Factory")
                throw new ParameterBindingException($"Cannot bind sensor '{Sensor.Name}' of type '{Sensor.Type}'. Only Sensor Factory objects may be specified.");

            var description = "Sensor Factory " + (Channels.IsPresent ? "Channel" : "Sensor");

            First(GetSensorIds, description, "Sensor Factory Properties")
                .Finally(GetObjects, description)
                .Write();
        }

        List<FactoryIds> GetSensorIds()
        {
            var properties = client.GetSensorProperties(Sensor.Id);

            if (properties.ChannelDefinition == null)
                return new List<FactoryIds>();

            var regex = new Regex("(channel\\()(.+?)(,)(.+?)(\\))", RegexOptions.IgnoreCase);

            var ids = properties.ChannelDefinition.SelectMany(def => regex.Matches(def).Cast<Match>().Select(m => new FactoryIds
            {
                SensorId = Convert.ToInt32(regex.Replace(m.Value, "$2")),
                ChannelId = Convert.ToInt32(regex.Replace(m.Value, "$4"))
            })).ToList();

            return ids;
        }

        List<PrtgObject> GetObjects(List<FactoryIds> ids)
        {
            return Channels.IsPresent ? GetChannels(ids) : GetSensors(ids);
        }

        List<PrtgObject> GetSensors(List<FactoryIds> ids)
        {
            var filters = ids.Select(id => new SearchFilter(Property.Id, id.SensorId)).ToArray();

            return client.GetSensors(filters).Cast<PrtgObject>().ToList();
        }

        List<PrtgObject> GetChannels(List<FactoryIds> ids)
        {
            var channels = ids.SelectMany(id => client.GetChannels(id.SensorId).Where(c => c.Id == id.ChannelId)).ToList();

            return channels.Cast<PrtgObject>().ToList();
        }

        class FactoryIds
        {
            public int SensorId { get; set; }

            public int ChannelId { get; set; }
        }
    }
}
