using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GH.Core.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using GH.Core.Extensions;
using GH.Core.BlueCode.Entity.Notification;

namespace GH.Core.Services
{
    public class NotificationService : INotificationService
    {
        IMongoCollection<Account> _accountCollection;
        IMongoCollection<Notification> _notificationCollection;

        public NotificationService()
        {
            var db = MongoContext.Db;
            _accountCollection = db.Accounts;
            _notificationCollection = db.Notifications;
        }

        public void AddNotifications(params Notification[] notifications)
        {
            
            var accountIds = notifications.Select(s => s.ReceiverId);
            var accounts = _accountCollection.Find(s => accountIds.Contains(s.Id)&&s.AccountType==AccountType.Personal).ToList();
            List<Notification> notificationsAddeds = new List<Notification>();
            foreach (var item in accounts)
            {
                if (item.NotificationSettings != null)
                {
                    if (item.NotificationSettings.Workflow)
                    {
                        notificationsAddeds.AddRange(notifications.Where(s => s.ReceiverId == item.Id).ToList());
                    }
                } 
            }
            notificationsAddeds.AddRange(notifications.Where(s => !accounts.Select(a => a.Id).Contains(s.ReceiverId)).ToList());
            if (notificationsAddeds.Count > 0)
            {
                _notificationCollection.InsertMany(notificationsAddeds);
            }
            
        }
        
        public long CountUnreadNotifications(ObjectId receiverId)
        {
            return _notificationCollection.Find(n => n.ReceiverId == receiverId && !n.Read).Count();
        }

        public List<Notification> GetNotifications(ObjectId receiverId, int? start, int? limit, DateTime? toTime, out long total, out DateTime queryTime)
        {
            if (toTime.HasValue)
            {
                var query = _notificationCollection.Find(n => n.ReceiverId == receiverId && n.CreatedAt < toTime);
                total = query.Count();
                queryTime = toTime.Value;
                return query.SortByDescending(n => n.CreatedAt).Skip(start).Limit(limit).ToList();
            }
            else
            {
                var query = _notificationCollection.Find(n => n.ReceiverId == receiverId);
                total = query.Count();
                queryTime = DateTime.Now;
                return query.SortByDescending(n => n.CreatedAt).Skip(start).Limit(limit).ToList();
            }
        }

        public void MarkAllAsRead(ObjectId receiverId)
        {
            _notificationCollection.UpdateMany(n => n.ReceiverId == receiverId, Builders<Notification>.Update.Set(n => n.Read, true));
        }

        public void Read(ObjectId notificationId, ObjectId receiverId)
        {
            _notificationCollection.UpdateOne(n => n.Id == notificationId && n.ReceiverId == receiverId, Builders<Notification>.Update.Set(n => n.Read, true));
        }

        // Blue Code
        public LatestNotification GetLatestNotification(string accountId)
        {
            var account = _accountCollection.Find(a => a.AccountId.Equals(accountId)).SingleOrDefault();
            if(account!= null)
            {
                return account.LastestNotification;
            }

            return null;
        }

    }
}