using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using GH.Core.BlueCode.Entity.Common;
using System;

namespace GH.Core.BlueCode.Entity.ActivityLog
{
    [BsonDiscriminator("ActivityLog")]
    public class ActivityLog: IMongoDBEntity
    {
        public ObjectId Id { get; set; }   
        public string ActivityId { get; set; }
        public DateTime DateTime { get; set; }
        public string ActivityType { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string FromUserId { get; set; }
        public string FromUserName { get; set; }
        public string FromUserLink { get; set; }
        public string ToUserId { get; set; }
        public string ToUserEmail { get; set; }
        public string ToUserName { get; set; }
        public string ToUserProfileLink { get; set; }
        public string TargetOjectId { get; set; }
        public string TargetObjectName { get; set; }
        public string TargetObjectLink { get; set; }
    }
}
