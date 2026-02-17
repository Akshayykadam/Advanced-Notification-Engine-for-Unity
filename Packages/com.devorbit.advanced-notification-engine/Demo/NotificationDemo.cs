using System;
using UnityEngine;
using DevOrbit.AdvancedNotificationEngine.Runtime.Core;
using DevOrbit.AdvancedNotificationEngine.Runtime.Models;

namespace DevOrbit.AdvancedNotificationEngine.Demo
{
    public class NotificationDemo : MonoBehaviour
    {
        private void Start()
        {
            NotificationManager.Initialize();
            NotificationManager.OnNotificationOpened += HandleNotificationOpened;
            NotificationManager.OnActionTriggered += HandleActionTriggered;
            
            // Register a deep link route
            NotificationRouter.Register("promo", (data) =>
            {
                Debug.Log($"[Demo] Deep link 'promo' triggered with code: {data.GetValueOrDefault("code")}");
            });
        }

        private void OnDestroy()
        {
            NotificationManager.OnNotificationOpened -= HandleNotificationOpened;
            NotificationManager.OnActionTriggered -= HandleActionTriggered;
        }

        public void ScheduleSimpleNotification()
        {
            var request = new LocalNotificationRequest
            {
                Id = "demo_simple_" + UnityEngine.Random.Range(0, 1000),
                Title = "Hello Unity",
                Body = "This is a test notification.",
                TriggerTime = DateTime.UtcNow.AddSeconds(5) // 5 seconds from now
            };
            
            NotificationManager.ScheduleLocal(request);
            Debug.Log($"[Demo] Scheduled '{request.Title}' for 5 seconds later.");
        }

        public void ScheduleInteractiveNotification()
        {
            var request = new LocalNotificationRequest
            {
                Id = "demo_interactive",
                Title = "Quest Available",
                Body = "Do you want to accept this quest?",
                TriggerTime = DateTime.UtcNow.AddSeconds(5),
                Actions = new[]
                {
                    new NotificationAction("accept", "Accept"),
                    new NotificationAction("decline", "Decline")
                }
            };
            
            NotificationManager.ScheduleLocal(request);
            Debug.Log("[Demo] Scheduled Interactive Notification.");
        }

        public void ToggleQuietHours()
        {
            var dnd = NotificationManager.GetQuietHours();
            dnd.Enabled = !dnd.Enabled;
            NotificationManager.SetQuietHours(dnd);
            Debug.Log($"[Demo] Quiet Hours set to: {dnd.Enabled} (10PM - 8AM)");
        }

        public void SimulateForegroundNotification()
        {
            var request = new LocalNotificationRequest
            {
                Id = "fg_test",
                Title = "In-Game Event",
                Body = "Something happened while you were playing!",
                TriggerTime = DateTime.UtcNow
            };
            
            // Manually trigger for demo purposes to test Overlay
            // In real app, Bridge would call this
            NotificationManager.HandleNotificationReceived(request);
        }

        public void PrintInbox()
        {
            var history = NotificationHistory.GetHistory();
            Debug.Log($"[Demo] Inbox ({history.Count}):");
            foreach (var item in history)
            {
                Debug.Log($"- {item.Title}: {item.Body} (Read: {item.WasRead})");
            }
        }

        public void CancelAll()
        {
            NotificationManager.CancelAll();
            Debug.Log("[Demo] Cancelled all notifications.");
        }

        private void HandleNotificationOpened(string id)
        {
            Debug.Log($"[Demo] UI: User opened notification {id}");
        }

        private void HandleActionTriggered(string action, System.Collections.Generic.Dictionary<string, string> payload)
        {
            Debug.Log($"[Demo] UI: User clicked action '{action}'");
        }
    }
}
