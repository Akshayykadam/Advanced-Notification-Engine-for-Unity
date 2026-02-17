using UnityEngine;
using UnityEditor;
using DevOrbit.AdvancedNotificationEngine.Runtime.Core;

namespace DevOrbit.AdvancedNotificationEngine.Editor
{
    public class NotificationSettings : EditorWindow
    {
        [MenuItem("Tools/Advanced Notification Engine/Settings")]
        public static void ShowWindow()
        {
            GetWindow<NotificationSettings>("Notification Settings");
        }

        private void OnGUI()
        {
            GUILayout.Label("Advanced Notification Engine", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Clear All Scheduled Notifications (Editor)"))
            {
                NotificationManager.CancelAll();
                Debug.Log("[NotificationSettings] Cleared all notifications.");
            }

            if (GUILayout.Button("Reset ID Registry"))
            {
                NotificationRegistry.ClearAll();
                Debug.Log("[NotificationSettings] ID Registry reset.");
            }
            
            EditorGUILayout.Space();
            GUILayout.Label("Version 1.0", EditorStyles.miniLabel);
        }
    }
}
