namespace PrtgAPI.PowerShell
{
    static class ParameterSet
    {
        /// <summary>
        /// The default parameter set that is used.
        /// </summary>
        internal const string Default = "DefaultSet";

        /// <summary>
        /// The parameter set that is used when specifying an Object ID manually.
        /// </summary>
        internal const string Manual = "ManualSet";

        /// <summary>
        /// The parameter set that is used when specifying a limit set of values for a specified set of <see cref="Parameters.BaseParameters"/>.
        /// </summary>
        internal const string Basic = "BasicSet";

        /// <summary>
        /// The parameter set that is used when operating in raw values.
        /// </summary>
        internal const string Raw = "RawSet";

        /// <summary>
        /// The parameter set that is used when utilizing dynamically generated parameters.
        /// </summary>
        internal const string Dynamic = "DynamicSet";

        internal const string DynamicManual = "DynamicManualSet";

        /// <summary>
        /// The parameter set that is used when utilizing a well typed property.
        /// </summary>
        internal const string Property = "PropertySet";

        /// <summary>
        /// The parameter set that is used when utilizing a raw property.
        /// </summary>
        internal const string RawProperty = "RawPropertySet";

        internal const string Type = "TypeSet";
        internal const string TypeManual = "TypeManualSet";

        internal const string RecordAge = "RecordAgeSet";
        internal const string DateTime = "DateTimeSet";
        internal const string DateTimeManual = "DateTimeManualSet";

        internal const string Aggregate = "AggregateSet";
        internal const string Summary = "SummarySet";

        internal const string LogicalAndTags = "LogicalAndTagsSet";
        internal const string LogicalOrTags = "LogicalOrTagsSet";

        /// <summary>
        /// The parameter set that is used when performing an action whose effects will last until a specified <see cref="System.DateTime"/>.
        /// </summary>
        internal const string Until = "UntilSet";

        internal const string UntilManual = "UntilManualSet";

        /// <summary>
        /// The parameter set that is used when performing an action whose effects will last forever.
        /// </summary>
        internal const string Forever = "ForeverSet";

        internal const string ForeverManual = "ForeverManualSet";

        internal const string Device = "DeviceSet";
        internal const string Group = "GroupSet";
        internal const string Notification = "NotificationSet";
        internal const string Schedule = "ScheduleSet";

        internal const string Target = "TargetSet";
        internal const string TargetManual = "TargetManualSet";

        internal const string Empty = "EmptySet";

        /// <summary>
        /// The parameter set that is used when adding to an existing object.
        /// </summary>
        internal const string Add = "AddSet";

        /// <summary>
        /// The parameter set that is used when adding a new object specifying an Object ID manually.
        /// </summary>
        internal const string AddManual = "AddManualSet";

        /// <summary>
        /// The parameter set that is used when editing an object specifying the Object ID manually.
        /// </summary>
        internal const string EditManual = "EditManualSet";

        /// <summary>
        /// The parameter set that is used when adding a new object from an existing object.
        /// </summary>
        internal const string AddFrom = "AddFromSet";

        internal const string AddFromManual = "AddFromManualSet";

        /// <summary>
        /// The parameter set that is used when editing an existing object.
        /// </summary>
        internal const string EditFrom = "EditFromSet";

        internal const string Force = "ForceSet";

        internal const string PropertyManual = "PropertyManualSet";
        internal const string RawPropertyManual = "RawPropertyManualSet";
        internal const string RawSubPropertyManual = "RawSubPropertyManualSet";
        internal const string RawManual = "RawManualSet";

        internal const string SensorToDestination = "SensorToDestinationSet";
        internal const string DeviceToDestination = "DeviceToDestinationSet";
        internal const string GroupToDestination = "GroupToDestinationSet";
        internal const string TriggerToDestination = "TriggerToDestinationSet";

        internal const string TargetForSource = "TargetForSourceSet";

        internal const string Deny = "DenySet";
        internal const string DenyManual = "DenyManualSet";
        internal const string AutoDiscover = "AutoDiscoverSet";
        internal const string AutoDiscoverManual = "AutoDiscoverManualSet";

        internal const string ObjectWithManual = "ObjectWithManualSet";
        internal const string ManualWithManual = "ManualWithManualSet";

        internal const string Object = "ObjectSet";

        internal const string HashTable = "HashTableSet";

        internal const string Report = "ReportSet";

        internal const string ReportManual = "ReportManualSet";
    }
}
