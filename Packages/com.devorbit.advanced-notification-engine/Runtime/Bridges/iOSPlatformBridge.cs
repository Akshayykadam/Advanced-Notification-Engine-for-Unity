using System;
using System.Runtime.InteropServices;
using UnityEngine;
using DevOrbit.AdvancedNotificationEngine.Runtime.Models;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Bridges
{
    /// <summary>
    /// iOS implementation of the notification bridge.
    /// Uses P/Invoke to talk to native Objective-C++ interface.
    /// </summary>
    public class iOSPlatformBridge : IPlatformBridge
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _AdvNotif_Initialize();

        [DllImport("__Internal")]
        private static extern void _AdvNotif_RequestPermissions();

        [DllImport("__Internal")]
        private static extern void _AdvNotif_ScheduleLocal(string id, string title, string body, double triggerTime, string dataJson, int repeatIntervalSeconds, string actionsJson);

        [DllImport("__Internal")]
        private static extern void _AdvNotif_CancelLocal(string id);

        [DllImport("__Internal")]
        private static extern void _AdvNotif_CancelAll();

        [DllImport("__Internal")]
        private static extern void _AdvNotif_SubscribeToTopic(string topic);

        [DllImport("__Internal")]
        private static extern void _AdvNotif_UnsubscribeFromTopic(string topic);
#endif

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _AdvNotif_Initialize();
            Debug.Log("[iOSPlatformBridge] Initialized Native.");
            IsInitialized = true;
#else
            Debug.Log("[iOSPlatformBridge] Initialize (Editor Stub)");
            IsInitialized = true;
#endif
        }

        public void RequestPermissions(Action<bool> callback)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _AdvNotif_RequestPermissions();
            callback?.Invoke(true); 
#else
            Debug.Log("[iOSPlatformBridge] RequestPermissions (Editor Stub)");
            callback?.Invoke(true);
#endif
        }

        public void Schedule(LocalNotificationRequest request)
        {
#if UNITY_IOS && !UNITY_EDITOR
            // Ensure UTC
            DateTime utcTime = request.TriggerTime.Kind == DateTimeKind.Utc
                ? request.TriggerTime
                : DateTime.SpecifyKind(request.TriggerTime, DateTimeKind.Utc);
            double triggerTime = new DateTimeOffset(utcTime).ToUnixTimeSeconds();
            
            // Serialize data
            string dataJson = "";
            if (request.Data != null && request.Data.Count > 0)
            {
                dataJson = JsonUtility.ToJson(request.Data);
            }

            // Calculate repeat interval in seconds
            int repeatIntervalSeconds = GetRepeatIntervalSeconds(request);

            // Serialize actions
            string actionsJson = "";
            if (request.Actions != null && request.Actions.Length > 0)
            {
                var sb = new System.Text.StringBuilder("[");
                for (int i = 0; i < request.Actions.Length; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append($"{{\"id\":\"{request.Actions[i].Id}\",\"title\":\"{request.Actions[i].Title}\"}}");
                }
                sb.Append("]");
                actionsJson = sb.ToString();
            }

            _AdvNotif_ScheduleLocal(request.Id, request.Title, request.Body, triggerTime, dataJson, repeatIntervalSeconds, actionsJson);
#else
            Debug.Log($"[iOSPlatformBridge] Schedule: {request.Id} Repeat={request.Repeat} (Editor Stub)");
#endif
        }

        private int GetRepeatIntervalSeconds(LocalNotificationRequest request)
        {
            switch (request.Repeat)
            {
                case RepeatInterval.Hourly: return 3600;
                case RepeatInterval.Daily: return 86400;
                case RepeatInterval.Weekly: return 604800;
                case RepeatInterval.Custom: return Mathf.Max(request.CustomRepeatSeconds, 60); // iOS minimum 60s
                default: return 0;
            }
        }

        public void Cancel(string id)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _AdvNotif_CancelLocal(id);
#else
            Debug.Log($"[iOSPlatformBridge] Cancel: {id} (Editor Stub)");
#endif
        }

        public void CancelAll()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _AdvNotif_CancelAll();
#else
            Debug.Log("[iOSPlatformBridge] CancelAll (Editor Stub)");
#endif
        }

        public void CreateChannel(string id, string name, string description)
        {
            // No-op on iOS — channels are Android-only
        }

        public void SubscribeToTopic(string topic)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _AdvNotif_SubscribeToTopic(topic);
#else
            Debug.Log($"[iOSPlatformBridge] SubscribeToTopic: {topic} (Editor Stub)");
#endif
        }

        public void UnsubscribeFromTopic(string topic)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _AdvNotif_UnsubscribeFromTopic(topic);
#else
            Debug.Log($"[iOSPlatformBridge] UnsubscribeFromTopic: {topic} (Editor Stub)");
#endif
        }

        public void SetUserProperty(string key, string value)
        {
            // Stub for Analytics
        }
    }
}
