using GH.Core.BlueCode.Entity.Notification;
using GH.Core.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Services
{
    public interface INotificationService
    {
        void AddNotifications(params Notification[] notifications);
        long CountUnreadNotifications(ObjectId receiverId);
        void Read(ObjectId notificationId, ObjectId receiverId);
        void MarkAllAsRead(ObjectId receiverId);

        List<Notification> GetNotifications(ObjectId receiverId, int? start, int? limit, DateTime? toTime, out long total, out DateTime queryTime);

        // Blue Code
        LatestNotification GetLatestNotification(string accountId);
    }
}