namespace PrtgAPI
{
    /// <summary>
    /// Specifies the time period for <see cref="TriggerType.Volume"/> triggers.
    /// </summary>
    public enum TriggerPeriod
    {
        /// <summary>
        /// Trigger when a volume limit is reached per hour.
        /// </summary>
        Hour,

        /// <summary>
        /// Trigger when a volume limit is reached per day.
        /// </summary>
        Day,

        /// <summary>
        /// Trigger when a volume limit is reached per week.
        /// </summary>
        Week,

        /// <summary>
        /// Trigger when a volume limit is reached per month.
        /// </summary>
        Month
    }
}
