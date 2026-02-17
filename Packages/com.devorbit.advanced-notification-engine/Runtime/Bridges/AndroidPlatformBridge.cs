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

        public void Initialize()
        {
            try
            {
                _nativeClass = new AndroidJavaClass(JAVA_CLASS_NAME);
                _nativeClass.CallStatic("initialize");
                Debug.Log("[AndroidPlatformBridge] Initialized Native Java Class.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AndroidPlatformBridge] Failed to initialize: {e.Message}");
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
            
            long triggerTimeMs = new DateTimeOffset(request.TriggerTime).ToUnixTimeMilliseconds();
            string dataJson = ""; // Serialize data if needed

            _nativeClass.CallStatic("scheduleLocal", 
                request.Id, 
                request.Title, 
                request.Body, 
                triggerTimeMs,
                dataJson
            );
        }

        public void Cancel(string id)
        {
            if (_nativeClass == null) return;
            _nativeClass.CallStatic("cancelLocal", id);
        }

        public void CancelAll()
        {
            // Native side cancellation of all isn't implemented in Java yet, 
            // but we can just clear registry on C# side or impl specific logic.
            Debug.Log("[AndroidPlatformBridge] CancelAll locally.");
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
