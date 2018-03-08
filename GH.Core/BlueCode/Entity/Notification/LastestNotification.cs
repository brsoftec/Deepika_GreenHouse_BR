using System.Collections.Generic;

namespace GH.Core.BlueCode.Entity.Notification
{
    public class LatestNotification
    {
        public bool UnViewed { get; set; }
        public int UnViewedNotifications { get; set; }
        public IEnumerable<NotificationMessage> Notifications { get; set; }
    }
}
