using System;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Bridges
{
    /// <summary>
    /// Interface for platform-specific notification implementation.
    /// </summary>
    public interface IPlatformBridge
    {
        /// <summary>
        /// Initializes the platform-specific notification system.
        /// </summary>
        bool IsInitialized { get; }
        void Initialize();

        /// <summary>
        /// Requests permission to send notifications.
        /// </summary>
        /// <param name="callback">Callback with permission status (true = granted).</param>
        void RequestPermissions(Action<bool> callback);

        /// <summary>
        /// Schedules a local notification.
        /// </summary>
        /// <param name="request">The notification request details.</param>
        void Schedule(Models.LocalNotificationRequest request);

        /// <summary>
        /// Cancels a specific notification by ID.
        /// </summary>
        /// <param name="id">The unique notification ID.</param>
        void Cancel(string id);

        /// <summary>
        /// Cancels all scheduled notifications.
        /// </summary>
        void CancelAll();

        // --- Advanced Features ---

        /// <summary>
        /// Creates a notification channel (Android only).
        /// </summary>
        void CreateChannel(string id, string name, string description);

        /// <summary>
        /// Subscribes the user to a topic (Firebase).
        /// </summary>
        void SubscribeToTopic(string topic);

        /// <summary>
        /// Unsubscribes the user from a topic (Firebase).
        /// </summary>
        void UnsubscribeFromTopic(string topic);

        /// <summary>
        /// Sets a user property for analytics.
        /// </summary>
        void SetUserProperty(string key, string value);
    }
}
