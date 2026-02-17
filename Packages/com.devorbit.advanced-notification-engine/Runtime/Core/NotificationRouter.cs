using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Core
{
    /// <summary>
    /// Handles routing of notifications to specific game logic or scenes.
    /// </summary>
    public static class NotificationRouter
    {
        private static Dictionary<string, Action<Dictionary<string, string>>> _routes = new Dictionary<string, Action<Dictionary<string, string>>>();
        private static Dictionary<string, string> _pendingPayload; // For cold start

        /// <summary>
        /// Registers a handler for a specific notification type.
        /// </summary>
        /// <param name="type">The 'type' field in the notification data.</param>
        /// <param name="callback">Action to execute.</param>
        public static void Register(string type, Action<Dictionary<string, string>> callback)
        {
            if (_routes.ContainsKey(type))
            {
                _routes[type] = callback;
            }
            else
            {
                _routes.Add(type, callback);
            }

            // Check if we have a pending payload for this type
            CheckPendingPayload();
        }

        /// <summary>
        /// Called platform-side when a notification is opened.
        /// </summary>
        public static void ProcessOpenedNotification(string type, Dictionary<string, string> data)
        {
            if (_routes.ContainsKey(type))
            {
                Debug.Log($"[NotificationRouter] Routing type: {type}");
                try
                {
                    _routes[type]?.Invoke(data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[NotificationRouter] Error in route handler: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"[NotificationRouter] No route registered for type: {type}. Caching as pending.");
                _pendingPayload = data; // Cache as pending if not registered yet (e.g., early init)
            }
        }

        private static void CheckPendingPayload()
        {
            if (_pendingPayload != null && _pendingPayload.ContainsKey("type"))
            {
                string type = _pendingPayload["type"];
                if (_routes.ContainsKey(type))
                {
                    Debug.Log($"[NotificationRouter] Processing pending payload for: {type}");
                    var payload = _pendingPayload;
                    _pendingPayload = null;
                    _routes[type]?.Invoke(payload);
                }
            }
        }
    }
}
