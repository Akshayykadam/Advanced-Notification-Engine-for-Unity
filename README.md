# Advanced Notification Engine for Unity

A production-ready, cross-platform notification system for Unity — schedule local & push notifications with deep linking, quiet hours, inbox history, and in-app overlays.

**Version**: 1.0.0 · **Unity**: 2021.3+ · **Platforms**: Android, iOS, Editor

---

## ✨ Features

| Feature | Description |
|---|---|
| **Local Notifications** | Schedule with exact timing, auto-fallback for restricted devices |
| **Push Notifications** | Firebase Cloud Messaging topic subscription |
| **Interactive Actions** | Add action buttons (Accept/Decline) to notifications |
| **Deep Linking** | Route users to specific screens via `NotificationRouter` |
| **Quiet Hours** | Auto-reschedule notifications outside DND windows |
| **Notification Inbox** | Persist and query notification history |
| **In-App Overlays** | Show banners while the app is in foreground |
| **Timezone Safe** | Automatic UTC ↔ Local conversion |
| **Robust Delivery** | `Handler.postDelayed` scheduling — no extra permissions required |

---

## 🚀 Quick Start

### Installation

**Via UPM** — Add git URL in Package Manager:
```
https://github.com/Akshayykadam/Advanced-Notification-Engine-for-Unity.git?path=Packages/com.devorbit.advanced-notification-engine
```

**Manual** — Copy `Packages/com.devorbit.advanced-notification-engine` into your project's `Packages/` folder.

### Initialize

```csharp
void Start()
{
    NotificationManager.Initialize();
    NotificationManager.RequestPermissions((granted) => {
        if (granted) Debug.Log("Notifications enabled!");
    });
}
```

---

## 📖 Usage

### Schedule a Notification

```csharp
var request = new LocalNotificationRequest
{
    Id = "daily_reward",
    Title = "Daily Reward Ready!",
    Body = "Claim your 100 Gems now.",
    TriggerTime = DateTime.UtcNow.AddHours(24),
    SmallIcon = "icon_small",
    LargeIcon = "icon_large"
};

NotificationManager.ScheduleLocal(request);
```

### Interactive Actions

```csharp
request.Actions = new[]
{
    new NotificationAction("accept", "Accept Quest"),
    new NotificationAction("decline", "Ignore")
};

NotificationManager.OnActionTriggered += (actionId, payload) =>
{
    if (actionId == "accept") StartQuest();
};
```

### Deep Linking

```csharp
// Register routes at startup
NotificationRouter.Register("promo", (data) =>
{
    ShopManager.OpenPromoPage(data["code"]);
});

// Schedule with data payload
var request = new LocalNotificationRequest
{
    Id = "promo_123",
    Title = "Flash Sale!",
    Data = new Dictionary<string, string> {
        { "type", "promo" },
        { "code", "SUMMER2026" }
    }
};
```

### Quiet Hours

```csharp
NotificationManager.SetQuietHours(new DoNotDisturbSettings
{
    Enabled = true,
    StartHour = 22, // 10 PM
    EndHour = 8     // 8 AM
});
```

### Notification Inbox

```csharp
var history = NotificationHistory.GetHistory();
foreach (var item in history)
{
    Debug.Log($"{item.Title}: {item.Body} (Read: {item.WasRead})");
}

NotificationHistory.MarkAsRead(item.Id);
```

### Firebase Push Topics

```csharp
NotificationManager.SubscribeToTopic("news");
```

> **Note**: Requires Firebase Unity SDK with `google-services.json` configured.

### In-App Overlays

1. Add `NotificationOverlayController` to a UI Panel
2. Assign Title and Body text references
3. The controller automatically listens to `OnNotificationReceived`

The demo scene auto-creates the overlay if none exists.

---

## 🔧 Platform Setup

### Android

- **Permissions**: Only `POST_NOTIFICATIONS` is required (Android 13+), no alarm permissions needed
- **Icons**: Place drawable resources in `Plugins/Android/res/drawable/`
- **Delivery**: Uses `Handler.postDelayed` for all scheduling — permission-free, no `AlarmManager` dependency
- **Firebase**: Import Firebase Unity SDK (Messaging) and add `google-services.json`

### iOS

- Enable **Push Notifications** capability in Xcode
- Enable **Background Modes > Remote notifications**
- Add `GoogleService-Info.plist` for Firebase

---

## 📁 Package Structure

```
com.devorbit.advanced-notification-engine/
├── Demo/                    # Demo scene with interactive test UI
├── Plugins/
│   ├── Android/             # Java native code + AndroidManifest
│   └── iOS/                 # Swift native code
├── Runtime/
│   ├── Core/                # NotificationManager, Scheduler, History, Router
│   ├── Bridges/             # Platform-specific bridges (Android/iOS/Editor)
│   ├── Models/              # Data models (Request, Action, Settings)
│   └── UI/                  # Overlay controller
└── Tests/                   # Unit tests
```

---

## 🎮 Demo Scene

The included demo provides a full test UI with buttons for:
- **Schedule (5s)** — Local notification in 5 seconds
- **Interactive** — Notification with Accept/Decline actions
- **Foreground** — In-app overlay banner
- **Quiet Hours** — Toggle DND mode
- **Subscribe** — Firebase topic subscription
- **Inbox** — Print notification history
- **Cancel All** — Clear pending notifications

A built-in console log displays all native and C# events in real-time.

---

## Support

**DevOrbit Studios** — contact@devorbit.com
