using NUnit.Framework;
using System;
using DevOrbit.AdvancedNotificationEngine.Runtime.Core;
using DevOrbit.AdvancedNotificationEngine.Runtime.Models;
using DevOrbit.AdvancedNotificationEngine.Runtime.Utils;

namespace DevOrbit.AdvancedNotificationEngine.Tests
{
    public class NotificationTests
    {
        [Test]
        public void TestTimezoneConversion()
        {
            DateTime utc = DateTime.UtcNow;
            DateTime local = TimezoneManager.ConvertToLocal(utc);
            Assert.AreEqual(local.Kind, DateTimeKind.Local);
        }

        [Test]
        public void TestRegistryHandlesIds()
        {
            NotificationRegistry.ClearAll();
            string id = "test_id";
            NotificationRegistry.Register(id);
            
            Assert.Contains(id, NotificationRegistry.GetActiveIds());
            
            NotificationRegistry.Unregister(id);
            Assert.False(NotificationRegistry.GetActiveIds().Contains(id));
        }

        [Test]
        public void TestRouterRegistration()
        {
            bool triggered = false;
            NotificationRouter.Register("test_type", (data) =>
            {
                triggered = true;
            });

            NotificationRouter.ProcessOpenedNotification("test_type", new System.Collections.Generic.Dictionary<string, string>());
            
            Assert.IsTrue(triggered);
        }
    }
}
