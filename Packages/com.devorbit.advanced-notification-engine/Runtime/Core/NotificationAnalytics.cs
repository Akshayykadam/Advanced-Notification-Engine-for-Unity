using System;
using UnityEngine;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Core
{
    /// <summary>
    /// Handles tracking of notification lifecycle events.
    /// </summary>
    public static class NotificationAnalytics
    {
        public static event Action<string> OnNotificationReceived;
        public static event Action<string> OnNotificationOpened;

        internal static void TrackReceived(string id)
        {
            Debug.Log($"[NotificationAnalytics] Received: {id}");
            OnNotificationReceived?.Invoke(id);
        }

        internal static void TrackOpened(string id)
        {
            Debug.Log($"[NotificationAnalytics] Opened: {id}");
            OnNotificationOpened?.Invoke(id);
        }
    }
}
