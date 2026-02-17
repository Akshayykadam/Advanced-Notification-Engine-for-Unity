package com.devorbit.advancednotificationengine;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.util.Log;
import androidx.core.app.NotificationCompat;
import com.unity3d.player.UnityPlayer;

public class NotificationReceiver extends BroadcastReceiver {

    @Override
    public void onReceive(Context context, Intent intent) {
        String id = intent.getStringExtra("id");
        String title = intent.getStringExtra("title");
        String body = intent.getStringExtra("body");
        String data = intent.getStringExtra("data");

        Log.d("AdvNotifReceiver", "Received Alarm: " + title);

        showNotification(context, id, title, body, data);
    }

    private void showNotification(Context context, String id, String title, String body, String data) {
        // Intent to open the game when clicked
        Intent launchIntent = context.getPackageManager().getLaunchIntentForPackage(context.getPackageName());
        if (launchIntent != null) {
            launchIntent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
            launchIntent.putExtra("notification_payload", data);
        }

        PendingIntent pendingIntent = PendingIntent.getActivity(
                context,
                0,
                launchIntent,
                PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);

        NotificationCompat.Builder builder = new NotificationCompat.Builder(context, "default")
                .setSmallIcon(context.getApplicationInfo().icon) // Uses app icon by default
                .setContentTitle(title)
                .setContentText(body)
                .setPriority(NotificationCompat.PRIORITY_HIGH)
                .setContentIntent(pendingIntent)
                .setAutoCancel(true);

        NotificationManager notificationManager = (NotificationManager) context
                .getSystemService(Context.NOTIFICATION_SERVICE);
        // ID should be int for notify, using hashcode
        notificationManager.notify(id.hashCode(), builder.build());
    }
}
