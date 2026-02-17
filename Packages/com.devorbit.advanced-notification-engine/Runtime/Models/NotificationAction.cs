using System;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Models
{
    /// <summary>
    /// Represents an interactive action button on a notification.
    /// </summary>
    [Serializable]
    public class NotificationAction
    {
        /// <summary>
        /// Unique ID for the action (e.g., "accept", "reply").
        /// </summary>
        public string Id;

        /// <summary>
        /// Display title for the button.
        /// </summary>
        public string Title;

        /// <summary>
        /// Whether this action requires the app to open in foreground.
        /// </summary>
        public bool Foreground;

        public NotificationAction(string id, string title, bool foreground = true)
        {
            Id = id;
            Title = title;
            Foreground = foreground;
        }
    }
}
