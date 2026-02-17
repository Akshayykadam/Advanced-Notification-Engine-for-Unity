package com.devorbit.advancednotificationengine;

import android.app.AlarmManager;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.util.Log;
import com.unity3d.player.UnityPlayer;
import com.google.firebase.messaging.FirebaseMessaging;

import java.util.Calendar;

public class AdvancedNotificationEngine {

    private static final String TAG = "AdvNotifEngine";
    private static Context context;

    private static void logToUnity(String msg) {
        try {
            UnityPlayer.UnitySendMessage("AdvancedNotificationEngineBridge", "OnNativeLog", msg);
        } catch (Exception e) {
            // Unity not ready?
        }
    }

    public static void initialize() {
        context = UnityPlayer.currentActivity;
        logToUnity("Java: Initializing Engine...");
        Log.d(TAG, "Initialized Native Android Module");

        // Create default channel with HIGH importance (heads-up)
        // 4 = NotificationManager.IMPORTANCE_HIGH
        createChannel("default", "Default", "Default Game Notifications", 4);
        logToUnity("Java: Default Channel Created");
    }

    public static void requestPermissions() {
        if (Build.VERSION.SDK_INT >= 33) { // Android 13 (TIRAMISU)
            if (context.checkSelfPermission(
                    "android.permission.POST_NOTIFICATIONS") != android.content.pm.PackageManager.PERMISSION_GRANTED) {
                ((android.app.Activity) context)
                        .requestPermissions(new String[] { "android.permission.POST_NOTIFICATIONS" }, 101);
                Log.d(TAG, "Requested POST_NOTIFICATIONS permission");
                logToUnity("Java: Requested POST_NOTIFICATIONS");
            } else {
                Log.d(TAG, "POST_NOTIFICATIONS permission already granted");
                logToUnity("Java: Permissions Already Granted");
            }
        } else {
            logToUnity("Java: Android < 13, no runtime perm needed.");
        }
    }

    public static void createChannel(String id, String name, String description, int importance) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(id, name, importance);
            channel.setDescription(description);

            NotificationManager notificationManager = context.getSystemService(NotificationManager.class);
            notificationManager.createNotificationChannel(channel);
            Log.d(TAG, "Channel Created: " + id);
        }
    }

    public static void scheduleLocal(String id, String title, String body, long triggerTime, String dataJson) {
        long nowMs = System.currentTimeMillis();
        long delayMs = triggerTime - nowMs;
        logToUnity(
                "Java: Scheduling ID=" + id + " triggerTime=" + triggerTime + " now=" + nowMs + " delayMs=" + delayMs);

        if (delayMs < 0) {
            delayMs = 0;
            logToUnity("Java: WARNING - trigger time is in the past, firing immediately");
        }

        // For short delays (< 60s), use Handler.postDelayed — bypasses AlarmManager
        // entirely
        // This is more reliable and avoids SCHEDULE_EXACT_ALARM permission issues
        if (delayMs < 60000) {
            logToUnity("Java: Using Handler.postDelayed for short delay (" + delayMs + "ms)");
            final Context ctx = context;
            final String fId = id, fTitle = title, fBody = body, fData = dataJson;
            new android.os.Handler(android.os.Looper.getMainLooper()).postDelayed(() -> {
                logToUnity("Java: Handler fired! Showing notification now...");
                NotificationReceiver receiver = new NotificationReceiver();
                Intent intent = new Intent();
                intent.putExtra("id", fId);
                intent.putExtra("title", fTitle);
                intent.putExtra("body", fBody);
                intent.putExtra("data", fData);
                receiver.onReceive(ctx, intent);
            }, delayMs);
            return;
        }

        // For longer delays, use AlarmManager
        logToUnity("Java: Using AlarmManager for long delay (" + delayMs + "ms)");

        Intent intent = new Intent(context, NotificationReceiver.class);
        intent.putExtra("id", id);
        intent.putExtra("title", title);
        intent.putExtra("body", body);
        intent.putExtra("data", dataJson);

        int requestCode = id.hashCode();

        PendingIntent pendingIntent = PendingIntent.getBroadcast(
                context,
                requestCode,
                intent,
                PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);

        AlarmManager alarmManager = (AlarmManager) context.getSystemService(Context.ALARM_SERVICE);

        if (alarmManager != null) {
            try {
                // Try exact alarm first
                if (Build.VERSION.SDK_INT >= 31 && !alarmManager.canScheduleExactAlarms()) {
                    logToUnity("Java: Exact alarm NOT permitted, using inexact set()");
                    alarmManager.set(AlarmManager.RTC_WAKEUP, triggerTime, pendingIntent);
                } else {
                    alarmManager.setExactAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, triggerTime, pendingIntent);
                    logToUnity("Java: Exact alarm scheduled OK");
                }
                Log.d(TAG, "Scheduled Alarm for: " + triggerTime);
            } catch (SecurityException e) {
                logToUnity("Java: SECURITY ERROR: " + e.getMessage() + " — falling back to inexact alarm");
                Log.e(TAG, "Permission error scheduling alarm: " + e.getMessage());
                try {
                    alarmManager.set(AlarmManager.RTC_WAKEUP, triggerTime, pendingIntent);
                    logToUnity("Java: Inexact alarm fallback scheduled OK");
                } catch (Exception e2) {
                    logToUnity("Java: ALL alarm methods FAILED: " + e2.getMessage());
                    Log.e(TAG, "All alarm methods failed: " + e2.getMessage());
                }
            }
        } else {
            logToUnity("Java: ERROR - AlarmManager is null!");
        }
    }

    public static void cancelLocal(String id) {
        Intent intent = new Intent(context, NotificationReceiver.class);
        int requestCode = id.hashCode();
        PendingIntent pendingIntent = PendingIntent.getBroadcast(
                context,
                requestCode,
                intent,
                PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);

        AlarmManager alarmManager = (AlarmManager) context.getSystemService(Context.ALARM_SERVICE);
        if (alarmManager != null) {
            alarmManager.cancel(pendingIntent);
            Log.d(TAG, "Cancelled Alarm: " + id);
        }
    }

    // --- Firebase Integration ---

    public static void subscribeToTopic(String topic) {
        logToUnity("Java: Subscribing to topic '" + topic + "'...");
        try {
            FirebaseMessaging.getInstance().subscribeToTopic(topic)
                    .addOnCompleteListener(task -> {
                        if (task.isSuccessful()) {
                            logToUnity("Java: Subscribed to '" + topic + "' SUCCESS");
                        } else {
                            logToUnity("Java: Subscribe to '" + topic + "' FAILED");
                        }
                        Log.d(TAG, task.isSuccessful() ? "Subscribed to " + topic : "Subscribe failed");
                    });
        } catch (Exception e) {
            logToUnity("Java: Firebase ERROR: " + e.getMessage());
            Log.e(TAG, "Firebase subscribe error: " + e.getMessage());
        }
    }

    public static void unsubscribeFromTopic(String topic) {
        logToUnity("Java: Unsubscribing from topic '" + topic + "'...");
        try {
            FirebaseMessaging.getInstance().unsubscribeFromTopic(topic);
            logToUnity("Java: Unsubscribed from '" + topic + "'");
        } catch (Exception e) {
            logToUnity("Java: Firebase ERROR: " + e.getMessage());
        }
        Log.d(TAG, "Unsubscribed from " + topic);
    }
}
