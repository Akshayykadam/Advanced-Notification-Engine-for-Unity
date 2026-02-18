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
            NotificationManager.OnNativeLog += HandleNativeLog;
            
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
            NotificationManager.OnNativeLog -= HandleNativeLog;
        }

        private void OnGUI()
        {
            float sw = Screen.width;
            float sh = Screen.height;
            float pad = sw * 0.03f;
            float spacing = sh * 0.008f;
            float btnH = sh * 0.07f;
            int fontSize = Mathf.Clamp((int)(sh * 0.025f), 16, 36);

            // --- Styles ---
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = (int)(fontSize * 1.4f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.85f, 0.1f) }
            };

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(6, 6, 6, 6),
                wordWrap = true
            };

            GUIStyle logHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = (int)(fontSize * 0.9f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.6f, 0.8f, 1f) }
            };

            GUIStyle logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = (int)(fontSize * 0.72f),
                wordWrap = true,
                richText = true,
                normal = { textColor = new Color(0.85f, 0.85f, 0.85f) }
            };

            GUIStyle clearBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = (int)(fontSize * 0.8f),
                alignment = TextAnchor.MiddleCenter
            };

            // === TOP SECTION: Title + Buttons ===
            float titleH = sh * 0.05f;
            float topSectionH = titleH + spacing + (btnH + spacing) * 5 + spacing;
            float btnAreaW = sw - pad * 2;

            GUILayout.BeginArea(new Rect(pad, pad, btnAreaW, topSectionH));

            // Title
            GUILayout.Label("Advanced Notification Engine", titleStyle, GUILayout.Height(titleH));
            GUILayout.Space(spacing);

            // Row 1
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Schedule (5s)", btnStyle, GUILayout.Height(btnH)))
                ScheduleSimpleNotification();
            GUILayout.Space(spacing);
            if (GUILayout.Button("Interactive", btnStyle, GUILayout.Height(btnH)))
                ScheduleInteractiveNotification();
            GUILayout.EndHorizontal();
            GUILayout.Space(spacing);

            // Row 2
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Foreground", btnStyle, GUILayout.Height(btnH)))
                SimulateForegroundNotification();
            GUILayout.Space(spacing);
            var dnd = NotificationManager.GetQuietHours();
            string dndLabel = dnd.Enabled ? "Quiet Hours (ON)" : "Quiet Hours (OFF)";
            if (GUILayout.Button(dndLabel, btnStyle, GUILayout.Height(btnH)))
                ToggleQuietHours();
            GUILayout.EndHorizontal();
            GUILayout.Space(spacing);

            // Row 3
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Subscribe", btnStyle, GUILayout.Height(btnH)))
                NotificationManager.SubscribeToTopic("news");
            GUILayout.Space(spacing);
            if (GUILayout.Button("Inbox", btnStyle, GUILayout.Height(btnH)))
                PrintInbox();
            GUILayout.EndHorizontal();
            GUILayout.Space(spacing);

            // Row 4
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel All", btnStyle, GUILayout.Height(btnH)))
                CancelAll();
            GUILayout.Space(spacing);
            if (GUILayout.Button("Check Init", btnStyle, GUILayout.Height(btnH)))
                Log($"Is Initialized: {NotificationManager.IsInitialized}");
            GUILayout.EndHorizontal();
            GUILayout.Space(spacing);

            // Row 5
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Repeat Daily", btnStyle, GUILayout.Height(btnH)))
                ScheduleRepeatingNotification();
            GUILayout.Space(spacing);
            if (GUILayout.Button("Repeat Hourly", btnStyle, GUILayout.Height(btnH)))
                ScheduleHourlyNotification();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            // === BOTTOM SECTION: Log Console ===
            if (_showConsole)
            {
                float logTop = pad + topSectionH + spacing * 2;
                float logH = sh - logTop - pad;
                float clearH = btnH * 0.6f;

                GUILayout.BeginArea(new Rect(pad, logTop, btnAreaW, logH));

                // Header row
                GUILayout.BeginHorizontal();
                GUILayout.Label("Console Log", logHeaderStyle, GUILayout.Height(clearH));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Clear", clearBtnStyle, GUILayout.Width(sw * 0.18f), GUILayout.Height(clearH)))
                    _consoleLog = "";
                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                // Log scroll area
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, "box");
                GUILayout.Label(_consoleLog, logStyle);
                GUILayout.EndScrollView();

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

        public void ScheduleRepeatingNotification()
        {
            var request = new LocalNotificationRequest
            {
                Id = "demo_daily",
                Title = "Daily Reward",
                Body = "Your daily reward is ready! Tap to collect.",
                TriggerTime = DateTime.UtcNow.AddSeconds(10),
                Repeat = RepeatInterval.Daily
            };
            
            NotificationManager.ScheduleLocal(request);
            Log($"Scheduled Daily Repeating Notification (first in 10s).");
        }

        public void ScheduleHourlyNotification()
        {
            var request = new LocalNotificationRequest
            {
                Id = "demo_hourly",
                Title = "Hourly Check-In",
                Body = "Come back and play!",
                TriggerTime = DateTime.UtcNow.AddSeconds(10),
                Repeat = RepeatInterval.Hourly
            };
            
            NotificationManager.ScheduleLocal(request);
            Log($"Scheduled Hourly Repeating Notification (first in 10s).");
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
        private void HandleNativeLog(string msg) => Log($"[Native] {msg}");

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
            rect.sizeDelta = new Vector2(0, 150);

            // Title
            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(panelGo.transform, false);
            Text titleText = titleGo.AddComponent<Text>();
            titleText.text = "Title";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            bodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
