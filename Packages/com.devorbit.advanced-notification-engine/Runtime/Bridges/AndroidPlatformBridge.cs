using System;
using UnityEngine;
using DevOrbit.AdvancedNotificationEngine.Runtime.Models;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Bridges
{
    /// <summary>
    /// Android implementation of the notification bridge.
    /// Uses AndroidJavaClass/Object to talk to native Java plugin.
    /// </summary>
    public class AndroidPlatformBridge : IPlatformBridge
    {
        private AndroidJavaClass _nativeClass;
        private const string JAVA_CLASS_NAME = "com.devorbit.advancednotificationengine.AdvancedNotificationEngine";

        public bool IsInitialized => _nativeClass != null;

        public void Initialize()
        {
            try
            {
                _nativeClass = new AndroidJavaClass(JAVA_CLASS_NAME);
                _nativeClass.CallStatic("initialize");
                Debug.Log("[AndroidPlatformBridge] Initialized Native Java Class SUCCESS.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AndroidPlatformBridge] FAILED to initialize: {e.Message}\nStack: {e.StackTrace}");
                _nativeClass = null; 
            }
        }

        public void RequestPermissions(Action<bool> callback)
        {
            if (_nativeClass != null)
            {
                _nativeClass.CallStatic("requestPermissions");
                Debug.Log("[AndroidPlatformBridge] Requested Native Permissions");
            }
            callback?.Invoke(true);
        }

        public void Schedule(LocalNotificationRequest request)
        {
            if (_nativeClass == null) return;
            
            // Ensure UTC interpretation regardless of DateTime.Kind
            DateTime utcTime = request.TriggerTime.Kind == DateTimeKind.Utc
                ? request.TriggerTime
                : DateTime.SpecifyKind(request.TriggerTime, DateTimeKind.Utc);
            long triggerTimeMs = new DateTimeOffset(utcTime).ToUnixTimeMilliseconds();
            long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Debug.Log($"[AndroidPlatformBridge] Schedule: trigger={triggerTimeMs} now={nowMs} delta={triggerTimeMs - nowMs}ms repeat={request.Repeat}");
            // Serialize data dictionary to JSON
            string dataJson = "{}";
            if (request.Data != null && request.Data.Count > 0)
            {
                var dataPairs = new System.Collections.Generic.List<string>();
                foreach (var kvp in request.Data)
                    dataPairs.Add($"\"{kvp.Key}\":\"{kvp.Value}\"");
                dataJson = "{" + string.Join(",", dataPairs) + "}";
            }

            // Serialize actions to JSON array
            string actionsJson = "[]";
            if (request.Actions != null && request.Actions.Length > 0)
            {
                var actionItems = new System.Collections.Generic.List<string>();
                foreach (var action in request.Actions)
                    actionItems.Add($"{{\"id\":\"{action.Id}\",\"title\":\"{action.Title}\"}}");
                actionsJson = "[" + string.Join(",", actionItems) + "]";
            }

            // Calculate repeat interval in milliseconds
            long repeatIntervalMs = GetRepeatIntervalMs(request);

            _nativeClass.CallStatic("scheduleLocal", 
                request.Id, 
                request.Title, 
                request.Body, 
                triggerTimeMs,
                dataJson,
                repeatIntervalMs,
                actionsJson
            );
        }

        private long GetRepeatIntervalMs(LocalNotificationRequest request)
        {
            switch (request.Repeat)
            {
                case RepeatInterval.Hourly: return 3600L * 1000L;
                case RepeatInterval.Daily: return 24L * 3600L * 1000L;
                case RepeatInterval.Weekly: return 7L * 24L * 3600L * 1000L;
                case RepeatInterval.Custom: return (long)request.CustomRepeatSeconds * 1000L;
                default: return 0L;
            }
        }

        public void Cancel(string id)
        {
            if (_nativeClass == null) return;
            _nativeClass.CallStatic("cancelLocal", id);
        }

        public void CancelAll()
        {
            if (_nativeClass == null) return;
            _nativeClass.CallStatic("cancelAll");
            Debug.Log("[AndroidPlatformBridge] CancelAll via native.");
        }

        public void CreateChannel(string id, string name, string description)
        {
            if (_nativeClass == null) return;
            // Importance: 3 = Default, 4 = High
            _nativeClass.CallStatic("createChannel", id, name, description, 4);
        }

        public void SubscribeToTopic(string topic)
        {
            if (_nativeClass == null) return;
            _nativeClass.CallStatic("subscribeToTopic", topic);
        }

        public void UnsubscribeFromTopic(string topic)
        {
            if (_nativeClass == null) return;
            _nativeClass.CallStatic("unsubscribeFromTopic", topic);
        }

        public void SetUserProperty(string key, string value)
        {
            // Access Firebase Analytics if integrated
            Debug.Log("[AndroidPlatformBridge] SetUserProperty (Stub - needs Analytics SDK)");
        }
    }
}
