using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Web.Areas.User.ViewModels
{
    public class NotificationsResult
    {
        public NotificationsResult()
        {
            Notifications = new List<NotificationInfo>();
        }

        public long Total { get; set; }
        public List<NotificationInfo> Notifications { get; set; }
        public DateTime QueryTime { get; set; }
    }

    public class NotificationInfo
    {
        public string Id { get; set; }
        public string Creator { get; set; }
        public string CreatorAvatar { get; set; }
        public string Url { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Read { get; set; }
        public string TargetType { get; set; }
    }
}