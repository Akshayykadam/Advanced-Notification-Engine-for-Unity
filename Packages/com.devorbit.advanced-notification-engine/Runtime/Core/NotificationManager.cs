using System;
using System.Collections.Generic;
using UnityEngine;
using DevOrbit.AdvancedNotificationEngine.Runtime.Bridges;
using DevOrbit.AdvancedNotificationEngine.Runtime.Models;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Core
{
    /// <summary>
    /// Main entry point for the Advanced Notification Engine.
    /// </summary>
    public static class NotificationManager
    {
        private static IPlatformBridge _bridge;
        private static bool _isInitialized;
        private static DoNotDisturbSettings _dndSettings = new DoNotDisturbSettings();

        /// <summary>
        /// Event fired when a notification is opened by the user.
        /// </summary>
        public static event Action<string> OnNotificationOpened;

        /// <summary>
        /// Event fired when a notification is received (foreground).
        /// </summary>
        public static event Action<LocalNotificationRequest> OnNotificationReceived;

        /// <summary>
        /// Event fired when a notification action button is clicked.
        /// </summary>
        public static event Action<string, Dictionary<string, string>> OnActionTriggered;

        /// <summary>
        /// Initializes the notification system. Must be called before scheduling.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

#if UNITY_EDITOR
            _bridge = new EditorPlatformBridge(); // We will implement this shortly
#elif UNITY_ANDROID
            _bridge = new AndroidPlatformBridge(); // We will implement this shortly
#elif UNITY_IOS
            _bridge = new iOSPlatformBridge(); // We will implement this shortly
#else
            _bridge = new EditorPlatformBridge(); // Fallback
#endif
            
            _bridge.Initialize();
            _isInitialized = true;
            Debug.Log("[NotificationManager] Initialized.");
        }

        /// <summary>
        /// Schedules a local notification.
        /// </summary>
        /// <param name="request">Request details.</param>
        public static void ScheduleLocal(LocalNotificationRequest request)
        {
            EnsureInitialized();
            
            // Validate and apply logic (e.g. Quiet Hours) via Scheduler
            NotificationScheduler.Schedule(request);
            
            // Scheduler handles Registry registration now, so we don't need to call it here explicitly
            // if we trust Scheduler. But to be safe and clean, let's let Scheduler handle logic
            // and Manager handle orchestration.
            // Current Scheduler implementation does Register(id).
            
            // Pass to bridge
            _bridge.Schedule(request);
            
            // Track in History (Upcoming feature)
            NotificationHistory.Add(request);
        }

        /// <summary>
        /// Cancels a notification by ID.
        /// </summary>
        public static void Cancel(string id)
        {
            EnsureInitialized();
            NotificationRegistry.Unregister(id);
            _bridge.Cancel(id);
        }

        /// <summary>
        /// Cancels all scheduled notifications.
        /// </summary>
        public static void CancelAll()
        {
            EnsureInitialized();
            NotificationRegistry.ClearAll();
            _bridge.CancelAll();
        }

        /// <summary>
        /// Requests permission to send notifications.
        /// </summary>
        public static void RequestPermissions(Action<bool> callback)
        {
            EnsureInitialized();
            _bridge.RequestPermissions(callback);
        }

        // --- Advanced Features ---

        /// <summary>
        /// Creates a notification channel (Android only).
        /// </summary>
        public static void CreateChannel(string id, string name, string description)
        {
            EnsureInitialized();
            _bridge.CreateChannel(id, name, description);
        }

        /// <summary>
        /// Subscribes to a topic for remote push notifications.
        /// </summary>
        public static void SubscribeToTopic(string topic)
        {
            EnsureInitialized();
            _bridge.SubscribeToTopic(topic);
        }

        /// <summary>
        /// Unsubscribes from a topic.
        /// </summary>
        public static void UnsubscribeFromTopic(string topic)
        {
            EnsureInitialized();
            _bridge.UnsubscribeFromTopic(topic);
        }

        /// <summary>
        /// Sets a user property for analytics/segmentation.
        /// </summary>
        public static void SetUserProperty(string key, string value)
        {
            EnsureInitialized();
            _bridge.SetUserProperty(key, value);
        }

        /// <summary>
        /// Configures Quiet Hours / Do Not Disturb.
        /// </summary>
        public static void SetQuietHours(DoNotDisturbSettings settings)
        {
            _dndSettings = settings;
        }

        /// <summary>
        /// Gets the current Quiet Hours settings.
        /// </summary>
        public static DoNotDisturbSettings GetQuietHours()
        {
            return _dndSettings;
        }

        // --- Internal / Bridge Callbacks ---

        public static void HandleNotificationReceived(LocalNotificationRequest request)
        {
            OnNotificationReceived?.Invoke(request);
        }

        public static void HandleNotificationOpened(string payload)
        {
            // Parse payload logic here later
            OnNotificationOpened?.Invoke(payload);
        }

        public static void HandleActionTriggered(string actionId, Dictionary<string, string> payload)
        {
            OnActionTriggered?.Invoke(actionId, payload);
        }

        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[NotificationManager] Auto-initializing. Call Initialize() manually for better control.");
                Initialize();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoStart()
        {
            Initialize();
            RequestPermissions((granted) => {
                Debug.Log($"[NotificationManager] Auto-Start Permission Request: {granted}");
            });
        }
    }
}
