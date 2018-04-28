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
        /// The parameter set that is used when specifying a limit set of values for a specified set of <see cref="Parameters.Parameters"/>.
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

        internal const string RecordAge = "RecordAgeSet";
        internal const string DateTime = "DateTimeSet";

        internal const string Aggregate = "AggregateSet";

        /// <summary>
        /// The parameter set that is used when performing an action whose effects will last until a specified <see cref="System.DateTime"/>.
        /// </summary>
        internal const string Until = "UntilSet";

        /// <summary>
        /// The parameter set that is used when performing an action whose effects will last forever.
        /// </summary>
        internal const string Forever = "ForeverSet";

        internal const string Device = "DeviceSet";
        internal const string Group = "GroupSet";
        internal const string Notification = "NotificationSet";

        internal const string Target = "TargetSet";

        internal const string Empty = "EmptySet";

        internal const string Add = "AddSet";
        internal const string Edit = "EditSet";
        internal const string AddFrom = "AddFromSet";
        internal const string EditFrom = "EditFromSet";
    }
}
