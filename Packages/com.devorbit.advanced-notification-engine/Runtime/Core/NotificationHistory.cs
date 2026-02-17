using System;
using System.Collections.Generic;
using UnityEngine;
using DevOrbit.AdvancedNotificationEngine.Runtime.Models;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Core
{
    [Serializable]
    public class StoredNotification
    {
        public string Id;
        public string Title;
        public string Body;
        public string Timestamp; // DateTime string
        public bool WasRead;
        public string DataJson; // Serialized dictionary
    }

    /// <summary>
    /// Manages the history/inbox of notifications.
    /// </summary>
    public static class NotificationHistory
    {
        private const string PREF_KEY = "DevOrbit_Notification_History";
        private static List<StoredNotification> _history = new List<StoredNotification>();

        static NotificationHistory()
        {
            Load();
        }

        public static void Add(LocalNotificationRequest request)
        {
            var stored = new StoredNotification
            {
                Id = request.Id,
                Title = request.Title,
                Body = request.Body,
                Timestamp = DateTime.UtcNow.ToString("O"),
                WasRead = false,
                // Serialize request.Data if needed (skipped for brevity)
            };
            
            _history.Insert(0, stored); // Add to top
            Save();
        }

        public static List<StoredNotification> GetHistory()
        {
            return new List<StoredNotification>(_history);
        }

        public static void MarkAsRead(string id)
        {
            var item = _history.Find(x => x.Id == id);
            if (item != null)
            {
                item.WasRead = true;
                Save();
            }
        }

        public static void Clear()
        {
            _history.Clear();
            Save();
        }

        private static void Load()
        {
            if (PlayerPrefs.HasKey(PREF_KEY))
            {
                try
                {
                    string json = PlayerPrefs.GetString(PREF_KEY);
                    Wrapper wrapper = JsonUtility.FromJson<Wrapper>(json);
                    if (wrapper != null && wrapper.Items != null)
                    {
                        _history = wrapper.Items;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[NotificationHistory] Failed to load history: {e.Message}");
                }
            }
        }

        private static void Save()
        {
            Wrapper wrapper = new Wrapper { Items = _history };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(PREF_KEY, json);
            PlayerPrefs.Save();
        }

        [Serializable]
        private class Wrapper
        {
            public List<StoredNotification> Items;
        }
    }
}
