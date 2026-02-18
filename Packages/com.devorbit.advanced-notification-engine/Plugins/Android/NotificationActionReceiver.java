package com.devorbit.advancednotificationengine;

import android.app.NotificationManager;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;
import com.unity3d.player.UnityPlayer;

/**
 * Handles notification action button clicks.
 * Sends the action ID back to Unity and dismisses the notification.
 */
public class NotificationActionReceiver extends BroadcastReceiver {

    private static final String TAG = "AdvNotifAction";

    @Override
    public void onReceive(Context context, Intent intent) {
        String actionId = intent.getStringExtra("action_id");
        String notificationId = intent.getStringExtra("notification_id");
        int notifIntId = intent.getIntExtra("notification_int_id", 0);
        String data = intent.getStringExtra("data");

        Log.d(TAG, "Action clicked: " + actionId + " on notification: " + notificationId);

        // Dismiss the notification
        NotificationManager notificationManager = (NotificationManager) context
                .getSystemService(Context.NOTIFICATION_SERVICE);
        if (notificationManager != null) {
            notificationManager.cancel(notifIntId);
        }

        // Send action back to Unity
        // Format: actionId|dataJson
        String payload = actionId + "|" + (data != null ? data : "{}");
        try {
            UnityPlayer.UnitySendMessage("AdvancedNotificationEngineBridge", "OnActionTriggered", payload);
            Log.d(TAG, "Sent action to Unity: " + payload);
        } catch (Exception e) {
            Log.w(TAG, "Unity not available to receive action: " + e.getMessage());
        }
    }
}
