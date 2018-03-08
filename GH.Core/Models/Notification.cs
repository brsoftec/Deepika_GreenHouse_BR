using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GH.Core.Models
{
    public class Notification
    {
        public Notification()
        {
            CreatedAt = DateTime.Now;
        }

        [BsonId]
        public ObjectId Id { get; set; }

        public ObjectId? Creator { get; set; }

        public ObjectId ReceiverId {get;set;}
        
        public ObjectId? TargetId { get; set; }

        public NotificationTargetType TargetType { get; set; }
        //public NotificationType Type { get; set; }

        public bool Read { get; set; }
        
        public string Message { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime CreatedAt { get; set; }
    }

    public enum NotificationTargetType
    {
        ReviewPost, RejectPost, ApprovePost
    }
    public enum NotificationType
    {
        Workflow
    }
}