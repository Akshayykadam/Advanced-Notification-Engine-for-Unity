# Advanced Notification Engine for Unity

A production-ready, highly extensible notification system for Unity, supporting Android, iOS, and Editor testing.

**Version**: 1.0.0
**Unity Version**: 2021.3+

## Features
- **Cross-Platform**: Unified API for Android and iOS.
- **Deep Linking**: Route users to specific scenes or logic with `NotificationRouter`.
- **Advanced Scheduling**: Exact alarms, repeating intervals (Daily/Weekly).
- **Timezone Safe**: Automatically handles UTC <-> Local conversions.
- **Quiet Hours**: Define "Do Not Disturb" windows (e.g., 10 PM - 8 AM).
- **Inbox / History**: Automatically save and retrieve past notifications.
- **In-App Overlays**: Display notification banners while the game is running.
- **Rich Media**: Support for Big Picture and Large Icons.
- **Channels & Topics**: Prepare for Android 8.0+ channels and Firebase topics.

## Installation

### Via Unity Package Manager (UPM)
Add the package from disk or git URL:
`https://github.com/your-repo/com.devorbit.advanced-notification-engine.git`

### Manual Installation
Copy the `Packages/com.devorbit.advanced-notification-engine` folder into your project's `Packages` directory.

## Setup

1.  **Initialize**: Call `NotificationManager.Initialize()` early in your game (e.g., splash screen).
2.  **Permissions**: Request notification permissions (Android 13+ / iOS).

```csharp
void Start()
{
    NotificationManager.Initialize();
    NotificationManager.RequestPermissions((granted) => {
        if (granted) Debug.Log("Notifications enabled!");
    });
}
```

## Usage Guide

### 1. Scheduling a Local Notification
```csharp
var request = new LocalNotificationRequest
{
    Id = "daily_reward",
    Title = "Daily Reward Ready!",
    Body = "Claim your 100 Gems now.",
    TriggerTime = DateTime.UtcNow.AddHours(24),
    SmallIcon = "icon_small", // Android resource name
    LargeIcon = "icon_large", // Android resource name
    Sound = "custom_sound"    // Resource name (no extension)
};

NotificationManager.ScheduleLocal(request);
```

### 2. Interactive Actions (Buttons)
```csharp
request.Actions = new[]
{
    new NotificationAction("accept", "Accept Quest"),
    new NotificationAction("decline", "Ignore")
};

// Handle clicks
NotificationManager.OnActionTriggered += (actionId, payload) =>
{
    if (actionId == "accept") StartQuest();
};
```

### 3. Deep Linking (Routing)
Route users to specific screens even if the app was closed.

```csharp
// Register routes once at startup
NotificationRouter.Register("promo", (data) =>
{
    string code = data["code"];
    ShopManager.OpenPromoPage(code);
});

// Schedule with data
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

### 4. Quiet Hours (Do Not Disturb)
Automatically reschedule notifications that fall within quiet hours (e.g., 10 PM - 8 AM) to the next valid time.

```csharp
var dnd = new DoNotDisturbSettings
{
    Enabled = true,
    StartHour = 22, // 10 PM
    EndHour = 8,    // 8 AM
    WeekendOnly = false
};

NotificationManager.SetQuietHours(dnd);
```

### 5. Notification Inbox (History)
Retrieve a list of past notifications to display in a "Messages" UI.

```csharp
var history = NotificationHistory.GetHistory();
foreach (var item in history)
{
    Debug.Log($"{item.Title}: {item.Body} (Read: {item.WasRead})");
}

// Mark as read
NotificationHistory.MarkAsRead(item.Id);
```

### 6. In-App Overlays
Show a notification banner if the user is playing the game when it arrives.

1.  Create a UI Panel with the `NotificationOverlayController` component.
2.  Assign your Text and Image references.
3.  The controller automatically listens to `OnNotificationReceived`.

## Platform Specifics

### Android
- **Icons**: Place icon assets in `Plugins/Android/res/drawable/`.
- **Manifest**: The plugin automatically handles standard permissions, but ensure your custom manifest includes `POST_NOTIFICATIONS` for Android 13+.
- **Firebase**: The plugin assumes the Firebase SDK is present in the project. Ensure you have imported the Firebase Unity SDK (Messaging).

### iOS
- Enable "Push Notifications" capability in Xcode.
- Enable "Background Modes > Remote notifications" for silent updates.
- **Firebase**: Ensure `GoogleService-Info.plist` is in your project and Firebase SDK is initialized.

## Native Implementation Details
This package includes native source code:
- **Android**: `Plugins/Android/AdvancedNotificationEngine.java` handles AlarmManager and FirebaseMessaging.
- **iOS**: `Plugins/iOS/AdvancedNotificationEngine.swift` handles UNUserNotificationCenter.

## Support
Contact: support@devorbit.com
