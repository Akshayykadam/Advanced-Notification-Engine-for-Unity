import Foundation
import UserNotifications
import FirebaseMessaging

@objc public class AdvancedNotificationEngine: NSObject, UNUserNotificationCenterDelegate, MessagingDelegate {
    
    @objc public static let shared = AdvancedNotificationEngine()
    
    private override init() {}
    
    @objc public func initialize() {
        UNUserNotificationCenter.current().delegate = self
        Messaging.messaging().delegate = self
        
        print("[AdvNotifEngine] iOS Initialized")
    }
    
    @objc public func requestPermissions() {
        let authOptions: UNAuthorizationOptions = [.alert, .badge, .sound]
        UNUserNotificationCenter.current().requestAuthorization(options: authOptions) { granted, error in
            print("[AdvNotifEngine] Permission Granted: \(granted)")
        }
        
        DispatchQueue.main.async {
            UIApplication.shared.registerForRemoteNotifications()
        }
    }
    
    @objc public func scheduleLocal(id: String, title: String, body: String, triggerTime: Double, dataJson: String) {
        let content = UNMutableNotificationContent()
        content.title = title
        content.body = body
        content.sound = UNNotificationSound.default
        content.userInfo = ["payload": dataJson]
        
        // Calculate time interval
        let date = Date(timeIntervalSince1970: triggerTime)
        let interval = date.timeIntervalSinceNow
        
        if interval > 0 {
            let trigger = UNTimeIntervalNotificationTrigger(timeInterval: interval, repeats: false)
            let request = UNNotificationRequest(identifier: id, content: content, trigger: trigger)
            
            UNUserNotificationCenter.current().add(request) { error in
                if let error = error {
                    print("[AdvNotifEngine] Error scheduling: \(error)")
                } else {
                    print("[AdvNotifEngine] Scheduled: \(id)")
                }
            }
        }
    }
    
    @objc public func cancelLocal(id: String) {
        UNUserNotificationCenter.current().removePendingNotificationRequests(withIdentifiers: [id])
        print("[AdvNotifEngine] Cancelled: \(id)")
    }
    
    @objc public func subscribeToTopic(topic: String) {
        Messaging.messaging().subscribe(toTopic: topic) { error in
            print("[AdvNotifEngine] Subscribed to \(topic)")
        }
    }
    
    @objc public func unsubscribeFromTopic(topic: String) {
        Messaging.messaging().unsubscribe(fromTopic: topic) { error in
            print("[AdvNotifEngine] Unsubscribed from \(topic)")
        }
    }
    
    // MARK: - UNUserNotificationCenterDelegate
    
    public func userNotificationCenter(_ center: UNUserNotificationCenter, willPresent notification: UNNotification, withCompletionHandler completionHandler: @escaping (UNNotificationPresentationOptions) -> Void) {
        // Show banner even when app is in foreground
        if #available(iOS 14.0, *) {
            completionHandler([.banner, .sound])
        } else {
            completionHandler([.alert, .sound])
        }
    }
    
    public func userNotificationCenter(_ center: UNUserNotificationCenter, didReceive response: UNNotificationResponse, withCompletionHandler completionHandler: @escaping () -> Void) {
        let userInfo = response.notification.request.content.userInfo
        if let payload = userInfo["payload"] as? String {
            print("[AdvNotifEngine] Opened with payload: \(payload)")
            // Here you would send a message back to Unity
            // UnitySendMessage("NotificationManager", "HandleNotificationOpened", payload)
        }
        completionHandler()
    }
}
