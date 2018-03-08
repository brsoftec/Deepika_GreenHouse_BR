using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GH.Core.Models
{
    public class BusinessAccountRole
    {
        public ObjectId AccountId { get; set; }
        public ObjectId? RoleId { get; set; }
    }

    public class JoiningBusinessInvitation
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public ObjectId From { get; set; }
        public ObjectId To { get; set; }
        public string InviteId { get; set; }
        public List<string> Roles { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime SentAt { get; set; }
    }
}