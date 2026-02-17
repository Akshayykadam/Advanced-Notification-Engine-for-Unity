using System;
using System.Collections.Generic;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Models
{
    /// <summary>
    /// Represents a request to schedule a local notification.
    /// </summary>
    [Serializable]
    public class LocalNotificationRequest
    {
        /// <summary>
        /// Unique identifier for this notification.
        /// </summary>
        public string Id;

        /// <summary>
        /// The notification title.
        /// </summary>
        public string Title;

        /// <summary>
        /// The main body text.
        /// </summary>
        public string Body;

        /// <summary>
        /// The time when this notification should be triggered (UTC).
        /// </summary>
        public DateTime TriggerTime;

        /// <summary>
        /// Custom data payload for deep linking or logic.
        /// </summary>
        public Dictionary<string, string> Data;

        /// <summary>
        /// Interactive action buttons.
        /// </summary>
        public NotificationAction[] Actions;

        /// <summary>
        /// Small icon resource name (Android only).
        /// </summary>
        public string SmallIcon;

        /// <summary>
        /// Large icon resource name (Android only).
        /// </summary>
        public string LargeIcon;

        /// <summary>
        /// URL for Big Picture style (Android) or Attachment (iOS).
        /// </summary>
        public string BigPictureUrl;

        /// <summary>
        /// URL for Large Icon (overrides resource if present).
        /// </summary>
        public string LargeIconUrl;

        /// <summary>
        /// Sound resource name (without extension).
        /// </summary>
        public string Sound;

        /// <summary>
        /// Group key for bundling notifications.
        /// </summary>
        public string Group;

        public LocalNotificationRequest()
        {
            Data = new Dictionary<string, string>();
        }
    }
}
