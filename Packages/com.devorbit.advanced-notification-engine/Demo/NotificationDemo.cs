using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DevOrbit.AdvancedNotificationEngine.Runtime.Core;
using DevOrbit.AdvancedNotificationEngine.Runtime.Models;
using DevOrbit.AdvancedNotificationEngine.Runtime.UI;

namespace DevOrbit.AdvancedNotificationEngine.Demo
{
    public class NotificationDemo : MonoBehaviour
    {
        private string _consoleLog = "";
        private Vector2 _scrollPos;
        private bool _showConsole = true;

        private void Start()
        {
            NotificationManager.Initialize();
            NotificationManager.OnNotificationOpened += HandleNotificationOpened;
            NotificationManager.OnActionTriggered += HandleActionTriggered;
            
            // Register a deep link route
            NotificationRouter.Register("promo", (data) =>
            {
                Log($"[Demo] Deep link 'promo' triggered with code: {data.GetValueOrDefault("code")}");
            });

            // Auto-create overlay if missing
            if (FindObjectOfType<NotificationOverlayController>() == null)
            {
                CreateOverlayUI();
            }

            Log("Demo Initialized. Press buttons to test.");
        }

        private void OnDestroy()
        {
            NotificationManager.OnNotificationOpened -= HandleNotificationOpened;
            NotificationManager.OnActionTriggered -= HandleActionTriggered;
        }

        private void OnGUI()
        {
            // Styles
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 24, padding = new RectOffset(10, 10, 10, 10) };
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, normal = { textColor = Color.yellow } };

            GUILayout.BeginArea(new Rect(20, 20, 400, Screen.height - 40));
            GUILayout.Label("Advanced Notification Engine", labelStyle);
            GUILayout.Space(20);

            if (GUILayout.Button("Schedule Simple (5s)", buttonStyle)) ScheduleSimpleNotification();
            GUILayout.Space(10);
            if (GUILayout.Button("Schedule Interactive", buttonStyle)) ScheduleInteractiveNotification();
            GUILayout.Space(10);
            if (GUILayout.Button("Simulate Foreground", buttonStyle)) SimulateForegroundNotification();
            GUILayout.Space(10);
            
            // Quiet Hours
            var dnd = NotificationManager.GetQuietHours();
            string dndStatus = dnd.Enabled ? "ON" : "OFF";
            if (GUILayout.Button($"Toggle Quiet Hours ({dndStatus})", buttonStyle)) ToggleQuietHours();
            GUILayout.Space(10);

            if (GUILayout.Button("Subscribe 'news'", buttonStyle)) NotificationManager.SubscribeToTopic("news");
            GUILayout.Space(10);
            if (GUILayout.Button("Print Inbox to Console", buttonStyle)) PrintInbox();
            GUILayout.Space(10);
            if (GUILayout.Button("Cancel All", buttonStyle)) CancelAll();
            
            GUILayout.EndArea();

            // Console Area
            if (_showConsole)
            {
                GUILayout.BeginArea(new Rect(440, 20, Screen.width - 460, Screen.height - 40));
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, "box");
                GUILayout.Label(_consoleLog);
                GUILayout.EndScrollView();
                if (GUILayout.Button("Clear Log", GUILayout.Height(40))) _consoleLog = "";
                GUILayout.EndArea();
            }
        }

        public void ScheduleSimpleNotification()
        {
            var request = new LocalNotificationRequest
            {
                Id = "demo_simple_" + UnityEngine.Random.Range(0, 1000),
                Title = "Hello Unity",
                Body = "This is a test notification.",
                TriggerTime = DateTime.UtcNow.AddSeconds(5)
            };
            
            NotificationManager.ScheduleLocal(request);
            Log($"Scheduled '{request.Title}' for 5s later.");
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
            Log("Scheduled Interactive Notification.");
        }

        public void ToggleQuietHours()
        {
            var dnd = NotificationManager.GetQuietHours();
            dnd.Enabled = !dnd.Enabled;
            // Default 10 PM to 8 AM
            if (dnd.Enabled && dnd.StartHour == 0 && dnd.EndHour == 0) {
                dnd.StartHour = 22; 
                dnd.EndHour = 8;
            }
            NotificationManager.SetQuietHours(dnd);
            Log($"Quiet Hours set to: {dnd.Enabled} ({dnd.StartHour}:00 - {dnd.EndHour}:00)");
        }

        public void SimulateForegroundNotification()
        {
            var request = new LocalNotificationRequest
            {
                Id = "fg_test_" + UnityEngine.Random.Range(0,100),
                Title = "In-Game Event",
                Body = "Something happened while you were playing!",
                TriggerTime = DateTime.UtcNow
            };
            
            // Manually trigger for demo purposes to test Overlay
            NotificationManager.HandleNotificationReceived(request);
            Log("Simulated Foreground Notification");
        }

        public void PrintInbox()
        {
            var history = NotificationHistory.GetHistory();
            Log($"Inbox Count: {history.Count}");
            foreach (var item in history)
            {
                Log($"- {item.Title}");
            }
        }

        public void CancelAll()
        {
            NotificationManager.CancelAll();
            Log("Cancelled all notifications.");
        }

        private void HandleNotificationOpened(string id) => Log($"UI: User opened notification {id}");
        private void HandleActionTriggered(string action, Dictionary<string, string> payload) => Log($"UI: User clicked action '{action}'");

        private void Log(string msg)
        {
            Debug.Log("[Demo] " + msg);
            _consoleLog += DateTime.Now.ToString("HH:mm:ss") + ": " + msg + "\n";
            _scrollPos.y = float.MaxValue; // Auto scroll
        }

        // --- Helper to create UI ---
        private void CreateOverlayUI()
        {
            GameObject canvasGo = new GameObject("NotificationCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // Panel
            GameObject panelGo = new GameObject("OverlayPanel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            Image panelImage = panelGo.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            RectTransform rect = panelGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(0, 150); // Height 150

            // Title
            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(panelGo.transform, false);
            Text titleText = titleGo.AddComponent<Text>();
            titleText.text = "Title";
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 28;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleLeft;
            RectTransform titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero; titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(20, 50); titleRect.offsetMax = new Vector2(-20, -10);

            // Body
            GameObject bodyGo = new GameObject("Body");
            bodyGo.transform.SetParent(panelGo.transform, false);
            Text bodyText = bodyGo.AddComponent<Text>();
            bodyText.text = "Body text goes here...";
            bodyText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            bodyText.fontSize = 20;
            bodyText.color = Color.gray;
            bodyText.alignment = TextAnchor.MiddleLeft;
            RectTransform bodyRect = bodyGo.GetComponent<RectTransform>();
            bodyRect.anchorMin = Vector2.zero; bodyRect.anchorMax = Vector2.one;
            bodyRect.offsetMin = new Vector2(20, 10); bodyRect.offsetMax = new Vector2(-20, -80);

            // Controller
            NotificationOverlayController ctrl = canvasGo.AddComponent<NotificationOverlayController>();
            ctrl.Panel = panelGo;
            ctrl.TitleText = titleText;
            ctrl.BodyText = bodyText;
            
            Log("Auto-created Overlay UI.");
        }
    }
}
