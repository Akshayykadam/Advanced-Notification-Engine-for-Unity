package com.devorbit.advancednotificationengine;

import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.util.Log;
import com.unity3d.player.UnityPlayer;
import java.util.HashMap;

public class AdvancedNotificationEngine {

    private static final String TAG = "AdvNotifEngine";
    private static Context context;
    private static final HashMap<String, Runnable> scheduledTasks = new HashMap<>();
    private static final android.os.Handler mainHandler = new android.os.Handler(android.os.Looper.getMainLooper());

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
        if (Build.VERSION.SDK_INT >= 33) {
            if (context.checkSelfPermission(
                    "android.permission.POST_NOTIFICATIONS") != android.content.pm.PackageManager.PERMISSION_GRANTED) {
                ((android.app.Activity) context)
                        .requestPermissions(new String[] { "android.permission.POST_NOTIFICATIONS" }, 101);
                Log.d(TAG, "Requested POST_NOTIFICATIONS permission");
                logToUnity("Java: Requested POST_NOTIFICATIONS");
            } else {
                Log.d(TAG, "POST_NOTIFICATIONS permission already granted");
                logToUnity("Java: POST_NOTIFICATIONS Already Granted");
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

    public static void scheduleLocal(String id, String title, String body, long triggerTime, String dataJson,
            long repeatIntervalMs) {
        long nowMs = System.currentTimeMillis();
        long delayMs = triggerTime - nowMs;
        logToUnity(
                "Java: Scheduling ID=" + id + " delayMs=" + delayMs + " repeatMs=" + repeatIntervalMs);

        if (delayMs < 0) {
            delayMs = 0;
            logToUnity("Java: Trigger time is in the past, firing immediately");
        }

        // Cancel any existing schedule with the same ID
        cancelLocal(id);

        final Context ctx = context;
        final String fId = id, fTitle = title, fBody = body, fData = dataJson;
        final long fRepeatMs = repeatIntervalMs;

        Runnable fireNotification = new Runnable() {
            @Override
            public void run() {
                logToUnity("Java: Handler fired! Showing notification: " + fId);
                NotificationReceiver receiver = new NotificationReceiver();
                Intent intent = new Intent();
                intent.putExtra("id", fId);
                intent.putExtra("title", fTitle);
                intent.putExtra("body", fBody);
                intent.putExtra("data", fData);
                receiver.onReceive(ctx, intent);

                // Re-schedule if repeating
                if (fRepeatMs > 0) {
                    logToUnity("Java: Re-scheduling repeating notification in " + fRepeatMs + "ms");
                    mainHandler.postDelayed(this, fRepeatMs);
                } else {
                    scheduledTasks.remove(fId);
                }
            }
        };

        scheduledTasks.put(id, fireNotification);
        mainHandler.postDelayed(fireNotification, delayMs);
        logToUnity("Java: Notification scheduled via Handler (delay=" + delayMs + "ms)");
        Log.d(TAG, "Scheduled via Handler: " + id + " delay=" + delayMs);
    }

    public static void cancelLocal(String id) {
        Runnable task = scheduledTasks.remove(id);
        if (task != null) {
            mainHandler.removeCallbacks(task);
            Log.d(TAG, "Cancelled scheduled notification: " + id);
            logToUnity("Java: Cancelled notification: " + id);
        } else {
            Log.d(TAG, "No scheduled notification found for: " + id);
        }
    }

    public static void cancelAll() {
        // Cancel all scheduled handler tasks
        for (Runnable task : scheduledTasks.values()) {
            mainHandler.removeCallbacks(task);
        }
        scheduledTasks.clear();

        // Also dismiss any already-shown notifications
        NotificationManager notifManager = (NotificationManager) context.getSystemService(Context.NOTIFICATION_SERVICE);
        if (notifManager != null) {
            notifManager.cancelAll();
        }
        logToUnity("Java: Cancelled all notifications");
        Log.d(TAG, "Cancelled all notifications");
    }

    // --- Firebase Integration (optional — works only if Firebase SDK is present)
    // ---

    public static void subscribeToTopic(String topic) {
        try {
            Class<?> fbClass = Class.forName("com.google.firebase.messaging.FirebaseMessaging");
            Object instance = fbClass.getMethod("getInstance").invoke(null);
            java.lang.reflect.Method subscribe = fbClass.getMethod("subscribeToTopic", String.class);
            subscribe.invoke(instance, topic);
            Log.d(TAG, "Subscribed to " + topic);
            logToUnity("Java: Subscribing to topic '" + topic + "'");
        } catch (Exception e) {
            Log.w(TAG, "Firebase not available — cannot subscribe to " + topic);
            logToUnity("Java: Firebase not available for topic subscription");
        }
    }

    public static void unsubscribeFromTopic(String topic) {
        try {
            Class<?> fbClass = Class.forName("com.google.firebase.messaging.FirebaseMessaging");
            Object instance = fbClass.getMethod("getInstance").invoke(null);
            java.lang.reflect.Method unsubscribe = fbClass.getMethod("unsubscribeFromTopic", String.class);
            unsubscribe.invoke(instance, topic);
            Log.d(TAG, "Unsubscribed from " + topic);
            logToUnity("Java: Unsubscribed from '" + topic + "'");
        } catch (Exception e) {
            Log.w(TAG, "Firebase not available — cannot unsubscribe from " + topic);
            logToUnity("Java: Firebase not available for topic unsubscription");
        }
    }

}