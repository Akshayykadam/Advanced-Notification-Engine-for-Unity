import Foundation
import UserNotifications

// Firebase is optional — only used if present in the project
#if canImport(FirebaseMessaging)
import FirebaseMessaging
#endif

@objc public class AdvancedNotificationEngine: NSObject, UNUserNotificationCenterDelegate {
    
    @objc public static let shared = AdvancedNotificationEngine()
    
    private override init() {}
    
    // MARK: - Initialize
    
    @objc public func initialize() {
        UNUserNotificationCenter.current().delegate = self
        
        #if canImport(FirebaseMessaging)
        Messaging.messaging().delegate = self
        #endif
        
        print("[AdvNotifEngine] iOS Initialized")
    }
    
    // MARK: - Permissions
    
    @objc public func requestPermissions() {
        let authOptions: UNAuthorizationOptions = [.alert, .badge, .sound]
        UNUserNotificationCenter.current().requestAuthorization(options: authOptions) { granted, error in
            let result = granted ? "true" : "false"
            print("[AdvNotifEngine] Permission Granted: \(granted)")
            // Send result back to Unity
            self.sendToUnity(method: "OnPermissionResult", message: result)
        }
        
        DispatchQueue.main.async {
            UIApplication.shared.registerForRemoteNotifications()
        }
    }
    
    // MARK: - Schedule Local Notification
    
    @objc public func scheduleLocal(id: String, title: String, body: String, triggerTime: Double, dataJson: String, repeatIntervalSeconds: Int, actionsJson: String) {
        let content = UNMutableNotificationContent()
        content.title = title
        content.body = body
        content.sound = UNNotificationSound.default
        content.userInfo = ["id": id, "payload": dataJson]
        
        // Register action category if actions provided
        if !actionsJson.isEmpty, let data = actionsJson.data(using: .utf8),
           let actions = try? JSONSerialization.jsonObject(with: data) as? [[String: String]] {
            var notifActions: [UNNotificationAction] = []
            for action in actions {
                if let actionId = action["id"], let actionTitle = action["title"] {
                    notifActions.append(UNNotificationAction(
                        identifier: actionId,
                        title: actionTitle,
                        options: .foreground
                    ))
                }
            }
            if !notifActions.isEmpty {
                let categoryId = "category_\(id)"
                let category = UNNotificationCategory(
                    identifier: categoryId,
                    actions: notifActions,
                    intentIdentifiers: [],
                    options: []
                )
                UNUserNotificationCenter.current().setNotificationCategories([category])
                content.categoryIdentifier = categoryId
            }
        }
        
        // Build trigger
        var trigger: UNNotificationTrigger?
        let date = Date(timeIntervalSince1970: triggerTime)
        let interval = date.timeIntervalSinceNow
        
        if repeatIntervalSeconds > 0 {
            // Repeating notification
            switch repeatIntervalSeconds {
            case 3600: // Hourly
                let components = Calendar.current.dateComponents([.minute, .second], from: date)
                trigger = UNCalendarNotificationTrigger(dateMatching: components, repeats: true)
            case 86400: // Daily
                let components = Calendar.current.dateComponents([.hour, .minute, .second], from: date)
                trigger = UNCalendarNotificationTrigger(dateMatching: components, repeats: true)
            case 604800: // Weekly
                let components = Calendar.current.dateComponents([.weekday, .hour, .minute, .second], from: date)
                trigger = UNCalendarNotificationTrigger(dateMatching: components, repeats: true)
            default: // Custom interval
                let seconds = max(TimeInterval(repeatIntervalSeconds), 60) // iOS minimum is 60s
                trigger = UNTimeIntervalNotificationTrigger(timeInterval: seconds, repeats: true)
            }
        } else if interval > 0 {
            // One-shot notification
            trigger = UNTimeIntervalNotificationTrigger(timeInterval: max(interval, 1), repeats: false)
        } else {
            // Trigger immediately (1 second)
            trigger = UNTimeIntervalNotificationTrigger(timeInterval: 1, repeats: false)
        }
        
        let request = UNNotificationRequest(identifier: id, content: content, trigger: trigger)
        
        UNUserNotificationCenter.current().add(request) { error in
            if let error = error {
                print("[AdvNotifEngine] Error scheduling: \(error)")
                self.sendToUnity(method: "OnNativeLog", message: "iOS: Error scheduling \(id): \(error.localizedDescription)")
            } else {
                let repeatInfo = repeatIntervalSeconds > 0 ? " (repeating every \(repeatIntervalSeconds)s)" : ""
                print("[AdvNotifEngine] Scheduled: \(id)\(repeatInfo)")
                self.sendToUnity(method: "OnNativeLog", message: "iOS: Scheduled \(id)\(repeatInfo)")
            }
        }
    }
    
    // MARK: - Cancel
    
    @objc public func cancelLocal(id: String) {
        UNUserNotificationCenter.current().removePendingNotificationRequests(withIdentifiers: [id])
        print("[AdvNotifEngine] Cancelled: \(id)")
    }
    
    @objc public func cancelAll() {
        UNUserNotificationCenter.current().removeAllPendingNotificationRequests()
        UNUserNotificationCenter.current().removeAllDeliveredNotifications()
        print("[AdvNotifEngine] Cancelled ALL")
        sendToUnity(method: "OnNativeLog", message: "iOS: Cancelled all notifications")
    }
    
    // MARK: - Firebase Topics (optional)
    
    @objc public func subscribeToTopic(topic: String) {
        #if canImport(FirebaseMessaging)
        Messaging.messaging().subscribe(toTopic: topic) { error in
            if let error = error {
                print("[AdvNotifEngine] Subscribe error: \(error)")
            } else {
                print("[AdvNotifEngine] Subscribed to \(topic)")
            }
        }
        #else
        print("[AdvNotifEngine] Firebase not available — cannot subscribe to \(topic)")
        sendToUnity(method: "OnNativeLog", message: "iOS: Firebase not available for topic subscription")
        #endif
    }
    
    @objc public func unsubscribeFromTopic(topic: String) {
        #if canImport(FirebaseMessaging)
        Messaging.messaging().unsubscribe(fromTopic: topic) { error in
            print("[AdvNotifEngine] Unsubscribed from \(topic)")
        }
        #else
        print("[AdvNotifEngine] Firebase not available — cannot unsubscribe from \(topic)")
        #endif
    }
    
    // MARK: - UNUserNotificationCenterDelegate
    
    public func userNotificationCenter(_ center: UNUserNotificationCenter, willPresent notification: UNNotification, withCompletionHandler completionHandler: @escaping (UNNotificationPresentationOptions) -> Void) {
        // Show banner even when app is in foreground
        let userInfo = notification.request.content.userInfo
        let id = userInfo["id"] as? String ?? notification.request.identifier
        
        // Notify Unity about foreground notification
        sendToUnity(method: "OnNotificationReceived", message: id)
        
        if #available(iOS 14.0, *) {
            completionHandler([.banner, .sound])
        } else {
            completionHandler([.alert, .sound])
        }
    }
    
    public func userNotificationCenter(_ center: UNUserNotificationCenter, didReceive response: UNNotificationResponse, withCompletionHandler completionHandler: @escaping () -> Void) {
        let userInfo = response.notification.request.content.userInfo
        let id = userInfo["id"] as? String ?? response.notification.request.identifier
        let payload = userInfo["payload"] as? String ?? ""
        
        if response.actionIdentifier == UNNotificationDefaultActionIdentifier {
            // User tapped the notification itself
            print("[AdvNotifEngine] Opened notification: \(id)")
            sendToUnity(method: "OnNotificationOpened", message: id)
        } else if response.actionIdentifier != UNNotificationDismissActionIdentifier {
            // User tapped an action button
            print("[AdvNotifEngine] Action: \(response.actionIdentifier) on \(id)")
            sendToUnity(method: "OnActionTriggered", message: response.actionIdentifier)
        }
        
        completionHandler()
    }
    
    // MARK: - Unity Communication
    
    private func sendToUnity(method: String, message: String) {
        // UnitySendMessage sends a message to a GameObject named "AdvancedNotificationEngineBridge"
        // which has the NotificationBridgeReceiver MonoBehaviour attached
        let gameObject = "AdvancedNotificationEngineBridge"
        let cGameObject = strdup(gameObject)
        let cMethod = strdup(method)
        let cMessage = strdup(message)
        UnitySendMessage(cGameObject, cMethod, cMessage)
        free(cGameObject)
        free(cMethod)
        free(cMessage)
    }
}

// MARK: - Firebase Messaging Delegate (optional)

#if canImport(FirebaseMessaging)
extension AdvancedNotificationEngine: MessagingDelegate {
    public func messaging(_ messaging: Messaging, didReceiveRegistrationToken fcmToken: String?) {
        print("[AdvNotifEngine] FCM Token: \(fcmToken ?? "nil")")
    }
}
#endif
