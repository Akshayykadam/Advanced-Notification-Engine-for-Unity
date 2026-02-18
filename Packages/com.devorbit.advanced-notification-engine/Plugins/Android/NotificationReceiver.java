package com.devorbit.advancednotificationengine;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.util.Log;
import android.app.Notification;
import com.unity3d.player.UnityPlayer;
import org.json.JSONArray;
import org.json.JSONObject;

public class NotificationReceiver extends BroadcastReceiver {

    private void logToUnity(String msg) {
        try {
            UnityPlayer.UnitySendMessage("AdvancedNotificationEngineBridge", "OnNativeLog", msg);
        } catch (Exception e) {
            // Unity might not be running or initialized
        }
    }

    @Override
    public void onReceive(Context context, Intent intent) {
        String id = intent.getStringExtra("id");
        String title = intent.getStringExtra("title");
        String body = intent.getStringExtra("body");
        String data = intent.getStringExtra("data");
        String actions = intent.getStringExtra("actions");

        Log.d("AdvNotifReceiver", "Received Alarm: " + title);
        logToUnity("Receiver: Alarm fired! ID=" + id + " Title=" + title);

        showNotification(context, id, title, body, data, actions);
    }

    private void showNotification(Context context, String id, String title, String body, String data,
            String actionsJson) {
        // Intent to open the game when clicked
        Intent launchIntent = context.getPackageManager().getLaunchIntentForPackage(context.getPackageName());
        if (launchIntent != null) {
            launchIntent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
            launchIntent.putExtra("notification_payload", data);
        }

        PendingIntent pendingIntent = PendingIntent.getActivity(
                context,
                id.hashCode(), // Use unique request code
                launchIntent,
                PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);

        int iconResId = context.getApplicationInfo().icon;
        if (iconResId == 0) {
            // Fallback for Unity apps if default icon is missing from ApplicationInfo
            iconResId = context.getResources().getIdentifier("app_icon", "drawable", context.getPackageName());
        }
        if (iconResId == 0) {
            iconResId = context.getResources().getIdentifier("app_icon", "mipmap", context.getPackageName());
        }
        if (iconResId == 0) {
            // Ultimate fallback to system icon so SOMETHING shows up
            iconResId = android.R.drawable.sym_def_app_icon;
        }

        NotificationManager notificationManager = (NotificationManager) context
                .getSystemService(Context.NOTIFICATION_SERVICE);

        // DEBUG: Check if notifications are enabled globally
        boolean areEnabled = notificationManager.areNotificationsEnabled();
        Log.d("AdvNotifReceiver", "Are Notifications Enabled? " + areEnabled);
        logToUnity("Receiver: Notifications Enabled=" + areEnabled);

        // Ensure channel exists
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
            android.app.NotificationChannel channel = notificationManager.getNotificationChannel("default");
            if (channel == null) {
                Log.d("AdvNotifReceiver", "Channel 'default' not found, creating it...");
                channel = new android.app.NotificationChannel(
                        "default",
                        "Default",
                        android.app.NotificationManager.IMPORTANCE_HIGH);
                channel.setDescription("Default Game Notifications");
                channel.enableVibration(true);
                notificationManager.createNotificationChannel(channel);
            } else {
                Log.d("AdvNotifReceiver", "Channel 'default' exists. Importance: " + channel.getImportance());
            }
        }

        // Force system icon if custom one fails or just to be safe
        if (iconResId == 0) {
            Log.w("AdvNotifReceiver", "Icon ID is 0! Using system fallback.");
            iconResId = android.R.drawable.ic_popup_reminder;
        }

        Notification.Builder builder = new Notification.Builder(context, "default")
                .setSmallIcon(iconResId)
                .setContentTitle(title)
                .setContentText(body)
                .setContentIntent(pendingIntent)
                .setAutoCancel(true);

        // Add action buttons if provided
        int notifId = id.hashCode();
        if (actionsJson != null && !actionsJson.equals("[]")) {
            try {
                JSONArray actionsArray = new JSONArray(actionsJson);
                for (int i = 0; i < actionsArray.length(); i++) {
                    JSONObject actionObj = actionsArray.getJSONObject(i);
                    String actionId = actionObj.getString("id");
                    String actionTitle = actionObj.getString("title");

                    Intent actionIntent = new Intent(context, NotificationActionReceiver.class);
                    actionIntent.setAction("com.devorbit.advancednotificationengine.ACTION_" + actionId.toUpperCase());
                    actionIntent.putExtra("action_id", actionId);
                    actionIntent.putExtra("notification_id", id);
                    actionIntent.putExtra("notification_int_id", notifId);
                    actionIntent.putExtra("data", data);

                    PendingIntent actionPendingIntent = PendingIntent.getBroadcast(
                            context,
                            (id + "_" + actionId).hashCode(),
                            actionIntent,
                            PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);

                    builder.addAction(0, actionTitle, actionPendingIntent);
                    logToUnity("Receiver: Added action button '" + actionTitle + "' (" + actionId + ")");
                }
            } catch (Exception e) {
                Log.w("AdvNotifReceiver", "Failed to parse actions JSON: " + e.getMessage());
                logToUnity("Receiver: Failed to parse actions: " + e.getMessage());
            }
        }

        Log.d("AdvNotifReceiver", "Posting Notification | ID: " + id + " | Icon: " + iconResId);
        logToUnity("Receiver: Posting ID=" + id + " Icon=" + iconResId);

        // ID should be int for notify, using hashcode
        try {
            notificationManager.notify(notifId, builder.build());
            Log.d("AdvNotifReceiver", "Notify called successfully.");
            logToUnity("Receiver: Notify Success");
        } catch (Exception e) {
            Log.e("AdvNotifReceiver", "FAILED to notify: " + e.getMessage());
            logToUnity("Receiver: Notify FAILED: " + e.getMessage());
            e.printStackTrace();
        }
    }
}