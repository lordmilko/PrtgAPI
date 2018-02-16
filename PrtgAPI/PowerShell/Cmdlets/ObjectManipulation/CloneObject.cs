using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Objects.Shared;
using PrtgAPI.Parameters;
using PrtgAPI.PowerShell.Base;

namespace PrtgAPI.PowerShell.Cmdlets
{
    //todo: is it possible to get a list of all auto discover templates and then auto discover USING one of them

    /// <summary>
    /// <para type="synopsis">Clones a sensor, device, group or notification trigger within PRTG.</para>
    /// 
    /// <para type="description">The Clone-Object cmdlet duplicates a PRTG Sensor, Device, Group or Notification Trigger, including
    /// all objects defined under it (such as all sensors of a device, or all devices and sensors of a group).</para>
    /// <para type="description">Clone-Object can operate in two modes: Clone From and Clone To. In Clone From mode (the default) objects
    /// you wish to clone are piped into Clone-Object, requiring you to specify the Object ID of the parent the cloned object will sit under.
    /// In Clone To mode, objects you wish to clone a single object to are piped to Clone-Object, requiring you to specify the ID of the object you
    /// wish to clone.</para>
    /// <para type="description">Sensors can only be cloned to <see cref="Device"/> objects, whereas devices and groups can both cloned to other groups
    /// or directly under a probe.</para>
    /// 
    /// <para type="description">When cloning sensors, devices and groups objects (in Clone From mode), a name can optionally be specified. When cloning sensors and groups,
    /// if a name is not specified the name of the original sensor or group will be used. When cloning devices, if a name is not
    /// specified PrtgAPI will automatically name the device as "Clone of &lt;device&gt;, where &lt;device&gt;
    /// is the name of the original device. When cloning devices, a Hostname/IP Address can optionally be specified. If a Hostname/IP Address
    /// is ommitted, PrtgAPI will use the name of the device as the hostname. Cloning a notification trigger with Clone-Object is equivalent
    /// to passing a trigger to the New-TriggerParameters cmdlet, followed by Add-NotificationTrigger.</para>
    /// 
    /// <para type="description">To clone a single child object to multiple destination objects, the -SourceId parameter can be specified. When operating
    /// in Clone To mode, Clone-Object will automatically assign the newly created object the same name as the source object. When Clone-Object executes,
    /// it will automatically attempt to resolve the target object specified by the -SourceId parameter. If -SourceId cannot be resolved to a valid
    /// sensor, device or group, Clone-Object will throw an exception specifying that the specified object ID is not valid.</para>
    /// 
    /// <para type="description">When an object has been cloned, by default Clone-Object will attempt to automatically resolve the object
    /// into its resultant <see cref="Sensor"/>, <see cref="Device"/>, <see cref="Group"/> or <see cref="NotificationTrigger"/>  object.
    /// Based on the speed of your PRTG Server, this can sometimes result in a delay of 5-10 seconds due to the delay with which PRTG clones
    /// the object. If Clone-Object cannot resolve the resultant object on the first attempt, PrtgAPI will make a further
    /// 4 retries, pausing for a successively greater duration between each try. After each failed attempt a warning will be displayed indicating
    /// the number of attempts remaining. Object resolution can be aborted at any time by pressing an escape sequence such as Ctrl-C.</para>
    /// 
    /// <para type="description">If you do not wish to resolve the resultant object, you can specify -Resolve:$false, which will
    /// cause Clone-Object to output a clone summary, including the object ID, name and hostname (for devices) of the new object. When
    /// cloning triggers, is -Resolve:$false is specified, no summary will be returned (as PRTG does not automatically return any information
    /// regarding cloned triggers). As PRTG pauses all cloned sensors, devices and groups by default, it is generally recommended to resolve
    /// the new object so that you may unpause the object with Resume-Object.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1234 | Clone-Object 5678</code>
    ///     <para>Clone the sensor with ID 1234 to the device with ID 5678</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1234 | Clone-Object 5678 MyNewSensor</code>
    ///     <para>Clone the sensor with ID 1234 to the device with ID 5678 renamed as "MyNewSensor"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device *exch* | Clone-Object -SourceId 2002</code>
    ///     <para>Clone the object with ID 2002 to all devices whose name contains "exch"</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1234 | Clone-Object 5678 -Resolve:$false</code>
    ///     <para>Clone the sensor with ID 1234 into the device with ID 5678 without resolving the resultant PrtgObject</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device -Id 1234 | Clone-Object 5678 MyNewDevice 192.168.1.1</code>
    ///     <para>Clone the device with ID 1234 into the group or probe with ID 5678 renamed as "MyNewDevice" with IP Address 192.168.1.1</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Probe -Id 1234 | Get-Trigger | Clone-Object 5678</code>
    ///     <para>Clone all notification triggers (both inherited and explicitly defined) on the probe with ID 1234 to the object with ID 5678</para>
    /// </example>
    /// 
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Trigger</para>
    /// <para type="link">Get-NotificationTriggerParameters</para>
    /// <para type="link">Add-NotificationTrigger</para>
    /// <para type="link">Resume-Object</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Copy, "Object", SupportsShouldProcess = true)]
    public class CloneObject : NewObjectCmdlet
    {
        const string SensorToDestination = "SensorToDestination";
        const string DeviceToDestination = "DeviceToDestination";
        const string GroupToDestination = "GroupToDestination";
        const string TriggerToDestination = "TriggerToDestination";

        const string TargetForSource = "TargetForSource";

        /// <summary>
        /// <para type="description">The ID of the device (for sensors), group or probe (for groups and devices) that will hold the cloned object.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = SensorToDestination)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = DeviceToDestination)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = GroupToDestination)]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = TriggerToDestination)]
        public int DestinationId { get; set; }

        /// <summary>
        /// <para type="description">The name to rename the cloned object to.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = SensorToDestination)]
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = DeviceToDestination)]
        [Parameter(Mandatory = false, Position = 1, ParameterSetName = GroupToDestination)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The hostname or IP Address to set on the cloned device.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 2, ParameterSetName = DeviceToDestination)]
        public new string Host { get; set; }

        /// <summary>
        /// <para type="description">The sensor to clone.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = SensorToDestination)]
        public Sensor Sensor { get; set; }

        /// <summary>
        /// <para type="description">The device to clone.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = DeviceToDestination)]
        public Device Device { get; set; }

        /// <summary>
        /// <para type="description">The group to clone.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = GroupToDestination)]
        public Group Group { get; set; }

        /// <summary>
        /// <para type="description">The notification trigger to clone.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = TriggerToDestination)]
        public NotificationTrigger Trigger { get; set; }

        /// <summary>
        /// <para type="description">The ID of the object to clone.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = TargetForSource)]
        public int SourceId { get; set; }

        /// <summary>
        /// <para type="description">The object to clone the object specified by the <see cref="SourceId"/> to.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = TargetForSource)]
        public DeviceOrGroupOrProbe Destination { get; set; }

        private SensorOrDeviceOrGroupOrProbe sourceObj;
        private Func<int, List<PrtgObject>> sourceObjResolver;

        /// <summary>
        /// Provides an enhanced one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessingEx()
        {
            if (ParameterSetName == TargetForSource)
                SetSourceIdObject();
        }

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            var config = GetCmdletConfig();

            if (ShouldProcess($"{config.NameDescrption} ({config.IdDescription}, Destination: {DestinationId})"))
                ExecuteOperation(() => Clone(config), config);
        }

        private void Clone(CloneCmdletConfig config)
        {
            if (config.Object is NotificationTrigger)
            {
                CloneTrigger();
                return;
            }

            var id = config.Cloner(client, DestinationId);

            if (Resolve)
                ResolveObject(id, config.GetObjects, config.Object.GetType());
            else
            {
                if (!config.AllowBasic)
                    return;

                var response = new PSObject();
                response.Properties.Add(new PSNoteProperty("Id", id));
                response.Properties.Add(new PSNoteProperty("Name", config.Name));

                if(config.Host != null)
                    response.Properties.Add(new PSNoteProperty("Host", config.Host));

                WriteObject(response);
            }
        }

        private void CloneTrigger()
        {
            TriggerParameters parameters;

            switch (Trigger.Type)
            {
                case TriggerType.Change:
                    parameters = new ChangeTriggerParameters(DestinationId, Trigger);
                    break;
                case TriggerType.State:
                    parameters = new StateTriggerParameters(DestinationId, Trigger);
                    break;
                case TriggerType.Speed:
                    parameters = new SpeedTriggerParameters(DestinationId, Trigger);
                    break;
                case TriggerType.Threshold:
                    parameters = new ThresholdTriggerParameters(DestinationId, Trigger);
                    break;
                case TriggerType.Volume:
                    parameters = new VolumeTriggerParameters(DestinationId, Trigger);
                    break;
                default:
                    throw new NotImplementedException($"Handler of trigger type '{Trigger.Type}' is not implemented.");
            }

            if (Resolve)
            {
                var cmdlet = new AddNotificationTrigger {Parameters = parameters};
                var triggers = cmdlet.GetResolvedTriggers(() => client.AddNotificationTrigger(parameters));

                foreach (var obj in triggers)
                    WriteObject(obj);
            }
            else
                client.AddNotificationTrigger(parameters);
        }

        private CloneCmdletConfig GetCmdletConfig()
        {
            CloneCmdletConfig parameters;

            switch (ParameterSetName)
            {
                case SensorToDestination:
                    parameters = new CloneCmdletConfig(
                        Sensor,
                        Name ?? Sensor.Name,
                        GetSensors
                    );
                    break;
                case DeviceToDestination:
                    parameters = new CloneCmdletConfig(
                        Device,
                        string.IsNullOrEmpty(Name) ? $"Clone of {Device.Name}" : Name,
                        Host ?? Device.Host,
                        GetDevices
                    );
                    break;
                case GroupToDestination:
                    parameters = new CloneCmdletConfig(
                        Group,
                        Name ?? Group.Name,
                        GetGroups
                    );
                    break;
                case TriggerToDestination:
                    parameters = new CloneCmdletConfig(Trigger, null, null);
                    break;
                case TargetForSource:
                    DestinationId = Destination.Id;
                    parameters = new CloneCmdletConfig(sourceObj, sourceObj.Name, sourceObjResolver);
                    break;
                default:
                    throw new NotImplementedException($"Don't know how to handle parameter set '{ParameterSetName}'");
            }

            if (ParameterSetName != TriggerToDestination)
                parameters.AllowBasic = true;

            return parameters;
        }
        
        private void SetSourceIdObject()
        {
            var sensor = client.GetSensors(Property.Id, SourceId).FirstOrDefault();

            if (sensor != null)
            {
                sourceObj = sensor;
                sourceObjResolver = GetSensors;
                return;
            }

            var device = client.GetDevices(Property.Id, SourceId).FirstOrDefault();

            if (device != null)
            {
                sourceObj = device;
                sourceObjResolver = GetDevices;
                return;
            }

            var group = client.GetGroups(Property.Id, SourceId).FirstOrDefault();

            if (group != null)
            {
                sourceObj = group;
                sourceObjResolver = GetGroups;
                return;
            }
            
            throw new PSArgumentException($"Cannot clone object with ID '{SourceId}' as it is not a sensor, device or group");
        }

        private void ExecuteOperation(Action action, CloneCmdletConfig config)
        {
            ExecuteOperation(action, $"Cloning PRTG {config.TypeDescription}s", $"Cloning {config.TypeDescription.ToLower()} '{config.NameDescrption}' ({config.IdDescription})");
        }

        private void ResolveObject<T>(int id, Func<int, List<T>> getObjects, Type trueType)
        {
            WriteObject(ResolveObject(() => getObjects(id), o => o.Count == 0, "Could not resolve object with ID '{id}'", trueType), true);
        }

        private Func<int, List<PrtgObject>> GetSensors => id => client.GetSensors(Property.Id, id).Cast<PrtgObject>().ToList();
        private Func<int, List<PrtgObject>> GetDevices => id => client.GetDevices(Property.Id, id).Cast<PrtgObject>().ToList();
        private Func<int, List<PrtgObject>> GetGroups => id => client.GetGroups(Property.Id, id).Cast<PrtgObject>().ToList();
    }
}
