#import <Foundation/Foundation.h>
#import "UnityFramework/UnityFramework-Swift.h"

extern "C" {
    
    void _AdvNotif_Initialize() {
        [[AdvancedNotificationEngine shared] initialize];
    }

    void _AdvNotif_RequestPermissions() {
        [[AdvancedNotificationEngine shared] requestPermissions];
    }

    void _AdvNotif_ScheduleLocal(const char* id, const char* title, const char* body, double triggerTime, const char* dataJson, int repeatIntervalSeconds, const char* actionsJson) {
        NSString *nsId = [NSString stringWithUTF8String:id];
        NSString *nsTitle = [NSString stringWithUTF8String:title];
        NSString *nsBody = [NSString stringWithUTF8String:body];
        NSString *nsDataJson = [NSString stringWithUTF8String:dataJson];
        NSString *nsActionsJson = [NSString stringWithUTF8String:actionsJson];
        
        [[AdvancedNotificationEngine shared] scheduleLocalWithId:nsId
                                                           title:nsTitle
                                                            body:nsBody
                                                     triggerTime:triggerTime
                                                        dataJson:nsDataJson
                                            repeatIntervalSeconds:repeatIntervalSeconds
                                                     actionsJson:nsActionsJson];
    }

    void _AdvNotif_CancelLocal(const char* id) {
        NSString *nsId = [NSString stringWithUTF8String:id];
        [[AdvancedNotificationEngine shared] cancelLocalWithId:nsId];
    }

    void _AdvNotif_CancelAll() {
        [[AdvancedNotificationEngine shared] cancelAll];
    }

    void _AdvNotif_SubscribeToTopic(const char* topic) {
        NSString *nsTopic = [NSString stringWithUTF8String:topic];
        [[AdvancedNotificationEngine shared] subscribeToTopicWithTopic:nsTopic];
    }

    void _AdvNotif_UnsubscribeFromTopic(const char* topic) {
        NSString *nsTopic = [NSString stringWithUTF8String:topic];
        [[AdvancedNotificationEngine shared] unsubscribeFromTopicWithTopic:nsTopic];
    }
}
