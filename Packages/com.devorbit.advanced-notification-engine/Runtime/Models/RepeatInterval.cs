namespace DevOrbit.AdvancedNotificationEngine.Runtime.Models
{
    /// <summary>
    /// Defines how a notification repeats.
    /// </summary>
    public enum RepeatInterval
    {
        /// <summary>No repeat — fires once.</summary>
        None = 0,
        /// <summary>Repeats every hour.</summary>
        Hourly = 1,
        /// <summary>Repeats every day at the same time.</summary>
        Daily = 2,
        /// <summary>Repeats every week on the same day/time.</summary>
        Weekly = 3,
        /// <summary>Repeats at a custom interval (see CustomRepeatSeconds).</summary>
        Custom = 4
    }
}
