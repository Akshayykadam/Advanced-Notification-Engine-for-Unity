#import <Foundation/Foundation.h>
#import "UnityFramework/UnityFramework-Swift.h" // Assumes Unity generates this header for Swift interop

extern "C" {
    
    void _AdvNotif_Initialize() {
        [[AdvancedNotificationEngine shared] initialize];
    }

    void _AdvNotif_RequestPermissions() {
        [[AdvancedNotificationEngine shared] requestPermissions];
    }

    void _AdvNotif_ScheduleLocal(const char* id, const char* title, const char* body, double triggerTime, const char* dataJson) {
        NSString *nsId = [NSString stringWithUTF8String:id];
        NSString *nsTitle = [NSString stringWithUTF8String:title];
        NSString *nsBody = [NSString stringWithUTF8String:body];
        NSString *nsDataFn = [NSString stringWithUTF8String:dataJson];
        
        [[AdvancedNotificationEngine shared] scheduleLocalWithId:nsId title:nsTitle body:nsBody triggerTime:triggerTime dataJson:nsDataFn];
    }

    void _AdvNotif_CancelLocal(const char* id) {
        NSString *nsId = [NSString stringWithUTF8String:id];
        [[AdvancedNotificationEngine shared] cancelLocalWithId:nsId];
    }

    void _AdvNotif_SubscribeToTopic(const char* topic) {
        NSString *nsTopic = [NSString stringWithUTF8String:topic];
        [[AdvancedNotificationEngine shared] subscribeToTopicWithTopic:nsTopic];
    }

    void _AdvNotif_UnsubscribeFromTopic(const char* topic) {
        NSString *nsTopic = [NSString stringWithUTF8String:topic];
        [[AdvancedNotificationEngine shared] unsubscribeFromTopicWithTopic:nsTopic];
    }

    // Note: Creating Channels is not applicable on iOS, so no binding needed.
}
