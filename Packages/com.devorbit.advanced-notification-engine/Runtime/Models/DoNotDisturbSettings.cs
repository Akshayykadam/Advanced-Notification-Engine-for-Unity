using System;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Models
{
    /// <summary>
    /// Configuration for Quiet Hours / Do Not Disturb mode.
    /// </summary>
    [Serializable]
    public class DoNotDisturbSettings
    {
        public bool Enabled;
        
        /// <summary>
        /// Start hour (0-23).
        /// </summary>
        public int StartHour = 22; // 10 PM
        
        /// <summary>
        /// End hour (0-23).
        /// </summary>
        public int EndHour = 8; // 8 AM
        
        /// <summary>
        /// If true, only applies on weekends (Saturday/Sunday).
        /// </summary>
        public bool WeekendOnly;

        public DoNotDisturbSettings(bool enabled = false, int startHour = 22, int endHour = 8)
        {
            Enabled = enabled;
            StartHour = startHour;
            EndHour = endHour;
        }
    }
}
