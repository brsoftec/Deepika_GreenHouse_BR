using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;


namespace GH.Core.BlueCode.Entity.Invite
{

    [BsonDiscriminator("Invite")]
    [BsonIgnoreExtraElements]
    public class Invite : IMongoDBEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Email { get; set; }
//        public string[] ListEmail { get; set; }
        public string FromUserId { get; set; }
        public string FromDisplayName { get; set; }
        public string ToName { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string Options { get; set; }
        public object Payload { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
        public DateTime Sent { get; set; }
        public string Status { get; set; }
        public List<DateTime> SendHistory { get; set; }
    }
}