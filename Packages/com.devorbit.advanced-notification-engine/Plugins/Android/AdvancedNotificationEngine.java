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

    public static void initialize() {
        context = UnityPlayer.currentActivity;
        Log.d(TAG, "Initialized Native Android Module");

        // Create default channel
        createChannel("default", "Default", "Default Game Notifications", 3);
    }

    public static void requestPermissions() {
        if (Build.VERSION.SDK_INT >= 33) { // Android 13 (TIRAMISU)
            if (context.checkSelfPermission(
                    "android.permission.POST_NOTIFICATIONS") != android.content.pm.PackageManager.PERMISSION_GRANTED) {
                ((android.app.Activity) context)
                        .requestPermissions(new String[] { "android.permission.POST_NOTIFICATIONS" }, 101);
                Log.d(TAG, "Requested POST_NOTIFICATIONS permission");
            } else {
                Log.d(TAG, "POST_NOTIFICATIONS permission already granted");
            }
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
        // In a real implementation, we would use AlarmManager to trigger a
        // BroadcastReceiver
        // which then builds and shows the notification.
        // For this streamlined implementation, we will log the intent.

        Intent intent = new Intent(context, NotificationReceiver.class);
        intent.putExtra("id", id);
        intent.putExtra("title", title);
        intent.putExtra("body", body);
        intent.putExtra("data", dataJson);

        // Unique request code based on ID hash
        int requestCode = id.hashCode();

        PendingIntent pendingIntent = PendingIntent.getBroadcast(
                context,
                requestCode,
                intent,
                PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);

        AlarmManager alarmManager = (AlarmManager) context.getSystemService(Context.ALARM_SERVICE);

        if (alarmManager != null) {
            // Use setExactAndAllowWhileIdle for generic reliable timing
            // specific permission SCHEDULE_EXACT_ALARM might be needed for Android 12+
            try {
                alarmManager.setExactAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, triggerTime, pendingIntent);
                Log.d(TAG, "Scheduled Alarm for: " + triggerTime);
            } catch (SecurityException e) {
                Log.e(TAG, "Permission error scheduling alarm: " + e.getMessage());
            }
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
        FirebaseMessaging.getInstance().subscribeToTopic(topic)
                .addOnCompleteListener(task -> {
                    String msg = "Subscribed to " + topic;
                    if (!task.isSuccessful()) {
                        msg = "Subscribe failed";
                    }
                    Log.d(TAG, msg);
                });
    }

    public static void unsubscribeFromTopic(String topic) {
        FirebaseMessaging.getInstance().unsubscribeFromTopic(topic);
        Log.d(TAG, "Unsubscribed from " + topic);
    }
}
