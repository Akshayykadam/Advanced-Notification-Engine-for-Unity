using System;
using UnityEngine;
using DevOrbit.AdvancedNotificationEngine.Runtime.Models;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Core
{
    /// <summary>
    /// Handles the logic for scheduling notifications, including validation and repeating intervals.
    /// </summary>
    public static class NotificationScheduler
    {
        /// <summary>
        /// Validates and ensures the request is ready for the bridge.
        /// </summary>
        public static void Schedule(LocalNotificationRequest request)
        {
            if (request == null)
            {
                Debug.LogError("[NotificationScheduler] Request is null.");
                return;
            }

            if (string.IsNullOrEmpty(request.Id))
            {
                Debug.LogError("[NotificationScheduler] Request ID is missing.");
                return;
            }

            if (request.TriggerTime <= DateTime.UtcNow)
            {
                Debug.LogWarning($"[NotificationScheduler] Trigger time {request.TriggerTime} is in the past. Notification might fire immediately or be ignored depending on platform.");
            }

            // Apply Quiet Hours logic
            var dnd = NotificationManager.GetQuietHours();
            if (dnd != null && dnd.Enabled)
            {
                // Convert trigger time to local to check hours
                DateTime localTrigger = Utils.TimezoneManager.ConvertToLocal(request.TriggerTime);
                bool isWeekend = localTrigger.DayOfWeek == DayOfWeek.Saturday || localTrigger.DayOfWeek == DayOfWeek.Sunday;

                if (!dnd.WeekendOnly || isWeekend)
                {
                    // Check if time falls within DND range
                    // Handle crossing midnight (e.g. 22 to 8) vs same day (e.g. 13 to 15)
                    bool inRange = false;
                    int hour = localTrigger.Hour;
                    
                    if (dnd.StartHour > dnd.EndHour) // Crosses midnight
                    {
                        if (hour >= dnd.StartHour || hour < dnd.EndHour) inRange = true;
                    }
                    else // Same day
                    {
                        if (hour >= dnd.StartHour && hour < dnd.EndHour) inRange = true;
                    }

                    if (inRange)
                    {
                        // Reschedule to EndHour of the relevant day (today or tomorrow)
                        DateTime nextValidTime = localTrigger.Date.AddHours(dnd.EndHour);
                        if (nextValidTime <= localTrigger)
                        {
                            nextValidTime = nextValidTime.AddDays(1);
                        }
                        
                        // Add a small buffer (e.g., 1 minute) to avoid edge cases
                        nextValidTime = nextValidTime.AddMinutes(1);

                        // Convert back to UTC
                        DateTime newUtc = Utils.TimezoneManager.ConvertToUtc(nextValidTime);
                        Debug.Log($"[NotificationScheduler] Rescheduling '{request.Id}' from {request.TriggerTime} to {newUtc} due to Quiet Hours.");
                        request.TriggerTime = newUtc;
                    }
                }
            }
            
            // Register internally
            NotificationRegistry.Register(request.Id);
            
            // Pass to bridge (Accessing bridge via Manager is tricky if Scheduler is static util, 
            // but Manager calls Scheduler usually. Here we modified Scheduler to just validate/modify request 
            // before Manager sends it. Wait, Manager calls ScheduleLocal -> attempts to pass to bridge.
            // Ideally Manager should call Scheduler.Process(request) and then pass to bridge.)
            
            // CORRECTION: In current architecture, Manager calls NotificationRegistry then Bridge.
            // Scheduler was just a validation helper. 
            // We need to inject this logic into Manager or make Manager call Scheduler to *modify* the request.
            // Let's assume Manager calls Scheduler.Process(request) before sending.
            // But wait, Manager.ScheduleLocal currently calls:
            // 1. EnsureInitialized
            // 2. NotificationRegistry.Register
            // 3. _bridge.Schedule
            
            // Use this tool to Update SCHEDULER, but I need to update MANAGER to use Scheduler properly.
            // Currently Scheduler.Schedule(request) is a void that does validation/logging but returns nothing.
            // I will change it to `public static void ProcessRequest(LocalNotificationRequest request)` and modify the request object directly.
        }
    }
}
