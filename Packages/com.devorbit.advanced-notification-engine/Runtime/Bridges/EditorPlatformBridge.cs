using System;
using UnityEngine;
using DevOrbit.AdvancedNotificationEngine.Runtime.Models;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Bridges
{
    /// <summary>
    /// Editor implementation of the notification bridge.
    /// Logs notifications to the console instead of sending them.
    /// </summary>
    public class EditorPlatformBridge : IPlatformBridge
    {
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            IsInitialized = true;
            Debug.Log("[EditorPlatformBridge] Initialized.");
        }

        public void RequestPermissions(Action<bool> callback)
        {
            Debug.Log("[EditorPlatformBridge] RequestPermissions: Simulating 'Granted'.");
            callback?.Invoke(true);
        }

        public void Schedule(LocalNotificationRequest request)
        {
            Debug.Log($"[EditorPlatformBridge] Scheduled Notification: ID={request.Id}, Title='{request.Title}', Trigger={request.TriggerTime}");
        }

        public void Cancel(string id)
        {
            Debug.Log($"[EditorPlatformBridge] Cancelled Notification: ID={id}");
        }

        public void CancelAll()
        {
            Debug.Log("[EditorPlatformBridge] Cancelled ALL Notifications.");
        }

        public void CreateChannel(string id, string name, string description)
        {
            Debug.Log($"[EditorPlatformBridge] Created Channel: {id} ({name})");
        }

        public void SubscribeToTopic(string topic)
        {
            Debug.Log($"[EditorPlatformBridge] Subscribed to topic: {topic}");
        }

        public void UnsubscribeFromTopic(string topic)
        {
            Debug.Log($"[EditorPlatformBridge] Unsubscribed from topic: {topic}");
        }

        public void SetUserProperty(string key, string value)
        {
            Debug.Log($"[EditorPlatformBridge] Set user property: {key}={value}");
        }
    }
}
