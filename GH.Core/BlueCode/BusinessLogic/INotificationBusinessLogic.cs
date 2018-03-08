using GH.Core.BlueCode.Entity.Notification;
using System.Collections.Generic;

namespace RegitSocial.Business.Notification
{
    public interface INotificationBusinessLogic
    {
        void SendNotification(NotificationMessage notificationMessage);
        IEnumerable<NotificationMessage> ViewNotification(string accountId);
        IEnumerable<NotificationMessage> GetNotificationList(string userId, int pageIndex = 1, int pageSize = 10);
        List<NotificationMessage> GetNotificationByAccountId(string accountId, string notificationType = null, int start = 0, int take = 10);
        List<UserNotification> GetNotifications(string accountId, string notificationType = null, int start = 0, int take = 10);
        void MarkRead(string notifId);
    }
}
