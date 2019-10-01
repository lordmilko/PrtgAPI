using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PrtgAPI.Attributes;
using PrtgAPI.PowerShell.Base;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Utilities;
using IDynamicParameters = System.Management.Automation.IDynamicParameters;

namespace PrtgAPI.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Retrieves notification triggers from a PRTG Server.</para>
    /// 
    /// <para type="description">The Get-NotificationTrigger cmdlet retrieves notification triggers that are defined on a PRTG Object
    /// as well as the types of triggers an object supports. Notification triggers define conditions that when met by a sensor or one
    /// of its channels, should result in the firing of a notification action. When notification triggers are defined on a device,
    /// group, or probe, the triggers are inherited by all nodes under the object. Individual objects can choose to block inheritance
    /// of notification triggers, preventing those triggers from trickling down.</para>
    /// 
    /// <para type="description">When looking at notification triggers defined on a single object, Get-NotificationTrigger can be invoked with no arguments.
    /// When looking at notification triggers across multiple objects, it is often useful to filter out notification triggers inherited from a parent object via
    /// the -Inherited parameter.</para>
    /// 
    /// <para type="description"><see cref="NotificationTrigger"/> objects returned from Get-NotificationTrigger can be passed to Set-TriggerProperty
    /// or New-TriggerParameters, to allow cloning or editing the trigger's properties.</para>
    /// 
    /// <para type="description">Notification trigger types that are supported by a specified object can be determined using the -Types parameter.
    /// While there is no restriction on the types of triggers assignable to container-like objects (including devices, groups and probes)
    /// each sensor can only be assigned specific types based on the types of channels it contains. When adding a new trigger,
    /// Add-NotificationTrigger will automatically validate whether the specified TriggerParameters are assignable to the target object.
    /// If the new trigger's type is incompatible with the target object, PrtgAPI will throw an exception alerting you to this error.</para>
    /// 
    /// <example>
    ///     <code>C:\> Get-Probe | Get-NotificationTrigger</code>
    ///     <para>Get all notification triggers defined on all probes</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Device | Get-NotificationTrigger *pager* -Inherited $false</code>
    ///     <para>Get all notification triggers from all devices whose OnNotificationAction action name contains "pager"
    /// in the name and are not inherited from any parent objects.</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Probe -Id 2001 | Get-NotificationTrigger -Type State</code>
    ///     <para>Get all State notification triggers from the sensor with ID 2001</para>
    ///     <para/>
    /// </example>
    /// <example>
    ///     <code>C:\> Get-Sensor -Id 1001 | Get-NotificationTrigger -Types</code>
    ///     <para>Get all notification trigger types supported by the object with ID 1001.</para>
    /// </example>
    ///
    /// <para type="link" uri="https://github.com/lordmilko/PrtgAPI/wiki/Notification-Triggers#get-1">Online version:</para>
    /// <para type="link">Get-Sensor</para>
    /// <para type="link">Get-Device</para>
    /// <para type="link">Get-Group</para>
    /// <para type="link">Get-Probe</para>
    /// <para type="link">Add-NotificationTrigger</para>
    /// <para type="link">New-NotificationTriggerParameters</para>
    /// <para type="link">Set-NotificationTrigger</para>
    /// <para type="link">Set-NotificationTriggerProperty</para>
    /// </summary>
    [OutputType(typeof(NotificationTrigger))]
    [Cmdlet(VerbsCommon.Get, "NotificationTrigger", DefaultParameterSetName = ParameterSet.Dynamic)]
    public class GetNotificationTrigger : PrtgObjectCmdlet<NotificationTrigger>, IDynamicParameters
    {
        /// <summary>
        /// <para type="description">The object to retrieve notification triggers for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Type, HelpMessage = "The object to retrieve notification triggers for.")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = ParameterSet.Dynamic, HelpMessage = "The object to retrieve notification triggers for.")]
        public SensorOrDeviceOrGroupOrProbe Object { get; set; }

        /// <summary>
        /// <para type="description">The ID of the object to retrieve notification triggers for.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.TypeManual)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.DynamicManual)]
        public int ObjectId { get; set; }

        /// <summary>
        /// <para type="description">Filter the response to objects with a certain <see cref="TriggerProperty.OnNotificationAction"/>. Can include wildcards.</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 0, ParameterSetName = ParameterSet.Dynamic)]
        [Parameter(Mandatory = false, Position = 0, ParameterSetName = ParameterSet.DynamicManual)]
        public NameOrObject<NotificationAction>[] OnNotificationAction { get; set; }

        /// <summary>
        /// <para type="description">Filter the response to objects with a certain <see cref="TriggerProperty.OffNotificationAction"/>. Can include wildcards.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Dynamic)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DynamicManual)]
        public NameOrObject<NotificationAction>[] OffNotificationAction { get; set; }

        /// <summary>
        /// <para type="description">Filter the response to objects with a certain <see cref="TriggerProperty.EscalationNotificationAction"/>. Can include wildcards.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Dynamic)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DynamicManual)]
        public NameOrObject<NotificationAction>[] EscalationNotificationAction { get; set; }

        /// <summary>
        /// <para type="description">Filter the response to objects of a certain type.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Dynamic)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DynamicManual)]
        public TriggerType[] Type { get; set; }

        /// <summary>
        /// <para type="description">Filter the reponse to objects with a specified SubId.</para>
        /// </summary>
        [Alias("Id")]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Dynamic)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DynamicManual)]
        public int[] SubId { get; set; }

        /// <summary>
        /// <para type="description">Filter the reponse to objects with a specified ParentId.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Dynamic)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DynamicManual)]
        public int[] ParentId { get; set; }

        /// <summary>
        /// <para type="description">Filter the reponse to objects with a specified Channel.
        /// Can refer to an object capable of being used as the source of a <see cref="TriggerChannel"/> or a wildcard indicating the name of the channel.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Dynamic)]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DynamicManual)]
        public object[] Channel { get; set; }

        /// <summary>
        /// <para type="description">List all notification trigger types compatible with the specified object.</para> 
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.Type)]
        [Parameter(Mandatory = true, ParameterSetName = ParameterSet.TypeManual)] //todo: unit test type manual
        public SwitchParameter Types { get; set; }

        /// <summary>
        /// <para type="description">Indicates whether to include inherited triggers in the response.</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.Dynamic, HelpMessage = "Indicates whether to include inherited triggers in the response. If this value is not specified, inherited triggers are included.")]
        [Parameter(Mandatory = false, ParameterSetName = ParameterSet.DynamicManual, HelpMessage = "Indicates whether to include inherited triggers in the response. If this value is not specified, inherited triggers are included.")]
        public bool? Inherited { get; set; }

        private PropertyDynamicParameterSet<TriggerProperty> dynamicParameterSet;

        /// <summary>
        /// Performs enhanced record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecordEx()
        {
            switch (ParameterSetName)
            {
                case ParameterSet.Dynamic:
                case ParameterSet.DynamicManual:
                    base.ProcessRecordEx();
                    break;
                case ParameterSet.Type:
                case ParameterSet.TypeManual:
                    TypeDescription = "Notification Trigger Type";
                    WriteObjectWithProgress(GetNotificationTriggerTypes);
                    break;
                default:
                    throw new UnknownParameterSetException(ParameterSetName);
            }
        }

        private PSObject GetNotificationTriggerTypes()
        {
            PrtgObject prtgObject = Object;

            if (prtgObject == null)
                prtgObject = client.GetObject(ObjectId);

            var types = client.GetNotificationTriggerTypes(prtgObject);

            var names = Enum.GetValues(typeof(TriggerType)).Cast<TriggerType>().ToList();

            var obj = new PSObject();
            obj.TypeNames.Insert(0, "PrtgAPI.TriggerTypePSObject");

            obj.Properties.Add(new PSNoteProperty("Name", prtgObject.Name));
            obj.Properties.Add(new PSNoteProperty("ObjectId", prtgObject.Id));

            foreach (var name in names)
            {
                obj.Properties.Add(new PSNoteProperty(name.ToString(), types.Contains(name)));
            }

            return obj;
        }

        /// <summary>
        /// Retrieves all notification triggers from a PRTG Server.
        /// </summary>
        /// <returns>A list of all notification triggers.</returns>
        protected override IEnumerable<NotificationTrigger> GetRecords()
        {
            IEnumerable<NotificationTrigger> triggers = client.GetNotificationTriggers(Object?.Id ?? ObjectId);

            if (Inherited == false)
                triggers = triggers.Where(a => a.Inherited == false);

            triggers = FilterResponseRecordsByPropertyNameOrObjectId(OnNotificationAction, t => t.OnNotificationAction, triggers);
            triggers = FilterResponseRecordsByPropertyNameOrObjectId(OffNotificationAction, t => t.OffNotificationAction, triggers);
            triggers = FilterResponseRecordsByPropertyNameOrObjectId(EscalationNotificationAction, t => t.EscalationNotificationAction, triggers);

            triggers = FilterResponseRecords(ParentId, t => t.ParentId, triggers);
            triggers = FilterResponseRecords(SubId, t => t.SubId, triggers);
            triggers = FilterResponseRecords(Type, t => t.Type, triggers);

            if (Channel != null)
                triggers = FilterResponseRecordsByChannel(Channel, t => t.Channel, triggers);

            triggers = FilterByDynamicParameters(triggers);

            return triggers;
        }

        private IEnumerable<NotificationTrigger> FilterResponseRecordsByChannel(object[] channel, Func<NotificationTrigger, object> func, IEnumerable<NotificationTrigger> triggers)
        {
            foreach (var trigger in triggers)
            {
                if (trigger.Channel != null)
                {
                    foreach (var obj in channel)
                    {
                        TriggerChannel result;

                        if (TriggerChannel.TryParse(obj, out result) && trigger.Channel.Equals(result))
                        {
                            yield return trigger;
                            break;
                        }
                        else
                        {
                            if (Object == null)
                                Object = client.GetObject(ObjectId, true) as SensorOrDeviceOrGroupOrProbe;

                            if (Object is Sensor)
                            {
                                var channels = client.GetChannels(Object.Id);

                                var wildcard = new WildcardPattern(obj?.ToString(), WildcardOptions.IgnoreCase);

                                var match = channels.Any(c => wildcard.IsMatch(trigger.Channel.ToString()));

                                if (match)
                                {
                                    yield return trigger;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private IEnumerable<NotificationTrigger> FilterByDynamicParameters(IEnumerable<NotificationTrigger> triggers)
        {
            if (dynamicParameterSet != null)
            {
                //Our NotificationAction parameters are defined as "real" parameters, so we will filter on them elsewhere,
                //not that it matters however, as they too don't support filtering where their values are null
                var boundParameters = dynamicParameterSet.GetBoundParameters(this, Tuple.Create).Where(t => t.Item2 != null);

                foreach (var parameter in boundParameters)
                {
                    //Get the PropertyInfo the filter Property corresponds to.
                    var property = ReflectionCacheManager.Get(typeof(NotificationTrigger)).Properties.First(p => p.GetAttribute<PropertyParameterAttribute>()?.Property.Equals(parameter.Item1) == true);

                    if (property.Property.PropertyType.IsArray)
                        throw new NotImplementedException("Cannot filter array properties dynamically.");

                    var items = parameter.Item2.ToIEnumerable().ToList();

                    triggers = triggers.Where(t => items.Any(i => Equals(property.GetValue(t), i)));
                }
            }

            return triggers;
        }

        /// <summary>
        /// Retrieves an object that defines the dynamic parameters of this cmdlet.
        /// </summary>
        /// <returns>An object that defines the dynamic parameters of this cmdlet.</returns>
        public object GetDynamicParameters()
        {
            if (dynamicParameterSet == null)
            {
                var properties = ReflectionCacheManager.Get(typeof(NotificationTrigger)).Properties.
                    Where(p => p.GetAttribute<PropertyParameterAttribute>() != null).
                    Select(p => Tuple.Create((TriggerProperty)p.GetAttribute<PropertyParameterAttribute>().Property, p)).ToList();

                dynamicParameterSet = new PropertyDynamicParameterSet<TriggerProperty>(
                    new[] {ParameterSet.Dynamic,ParameterSet.DynamicManual},
                    e => ReflectionCacheManager.GetArrayPropertyType(properties.FirstOrDefault(p => p.Item1 == e)?.Item2.Property.PropertyType),
                    this
                );
            }

            return dynamicParameterSet.Parameters;
        }
    }
}
