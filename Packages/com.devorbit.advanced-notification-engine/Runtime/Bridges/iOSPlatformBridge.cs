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
        private static extern void _AdvNotif_ScheduleLocal(string id, string title, string body, double triggerTime, string dataJson);

        [DllImport("__Internal")]
        private static extern void _AdvNotif_CancelLocal(string id);

        [DllImport("__Internal")]
        private static extern void _AdvNotif_SubscribeToTopic(string topic);

        [DllImport("__Internal")]
        private static extern void _AdvNotif_UnsubscribeFromTopic(string topic);
#endif

        public void Initialize()
        {
#if UNITY_IOS && !UNITY_EDITOR
            _AdvNotif_Initialize();
            Debug.Log("[iOSPlatformBridge] Initialized Native.");
#else
            Debug.Log("[iOSPlatformBridge] Initialize (Editor Stub)");
#endif
        }

        public void RequestPermissions(Action<bool> callback)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _AdvNotif_RequestPermissions();
            // In a real implementation we would register a callback delegate to receive the result async
            callback?.Invoke(true); 
#else
            Debug.Log("[iOSPlatformBridge] RequestPermissions (Editor Stub)");
            callback?.Invoke(true);
#endif
        }

        public void Schedule(LocalNotificationRequest request)
        {
#if UNITY_IOS && !UNITY_EDITOR
            // Calculate Unix timestamp for trigger time
            double triggerTime = new DateTimeOffset(request.TriggerTime).ToUnixTimeSeconds();
            string dataJson = ""; // Serialize payload

            _AdvNotif_ScheduleLocal(request.Id, request.Title, request.Body, triggerTime, dataJson);
#else
            Debug.Log($"[iOSPlatformBridge] Schedule: {request.Id} (Editor Stub)");
#endif
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
            Debug.Log("[iOSPlatformBridge] CancelAll locally/stub.");
        }

        public void CreateChannel(string id, string name, string description)
        {
            // No-op on iOS
        }

        public void SubscribeToTopic(string topic)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _AdvNotif_SubscribeToTopic(topic);
#endif
        }

        public void UnsubscribeFromTopic(string topic)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _AdvNotif_UnsubscribeFromTopic(topic);
#endif
        }

        public void SetUserProperty(string key, string value)
        {
            // Stub for Analytics
        }
    }
}
