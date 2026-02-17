using System;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Utils
{
    /// <summary>
    /// Utility for handling timezone conversions to ensure safe delivery.
    /// </summary>
    public static class TimezoneManager
    {
        public static DateTime ConvertToLocal(DateTime utcTime)
        {
            return utcTime.ToLocalTime();
        }

        public static DateTime ConvertToUtc(DateTime localTime)
        {
            return localTime.ToUniversalTime();
        }

        public static double GetCurrentUtcOffsetHours()
        {
            return TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalHours;
        }
    }
}
