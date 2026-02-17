using System;
using System.Collections.Generic;
using UnityEngine;

namespace DevOrbit.AdvancedNotificationEngine.Runtime.Core
{
    /// <summary>
    /// Registry to track scheduled notification IDs. 
    /// Made public for Editor tools, but intended for internal use.
    /// </summary>
    public static class NotificationRegistry
    {
        private const string PREF_KEY = "DevOrbit_Active_Notifications";
        
        [Serializable]
        private class RegistryData
        {
            public List<string> ActiveIds = new List<string>();
        }

        private static RegistryData _data;

        static NotificationRegistry()
        {
            Load();
        }

        private static void Load()
        {
            if (PlayerPrefs.HasKey(PREF_KEY))
            {
                try 
                {
                    string json = PlayerPrefs.GetString(PREF_KEY);
                    _data = JsonUtility.FromJson<RegistryData>(json);
                }
                catch
                {
                    _data = new RegistryData();
                }
            }
            else
            {
                _data = new RegistryData();
            }
        }

        private static void Save()
        {
            string json = JsonUtility.ToJson(_data);
            PlayerPrefs.SetString(PREF_KEY, json);
            PlayerPrefs.Save();
        }

        public static void Register(string id)
        {
            if (!_data.ActiveIds.Contains(id))
            {
                _data.ActiveIds.Add(id);
                Save();
            }
        }

        public static void Unregister(string id)
        {
            if (_data.ActiveIds.Contains(id))
            {
                _data.ActiveIds.Remove(id);
                Save();
            }
        }

        public static void ClearAll()
        {
            _data.ActiveIds.Clear();
            Save();
        }

        public static List<string> GetActiveIds()
        {
            return new List<string>(_data.ActiveIds);
        }
    }
}
