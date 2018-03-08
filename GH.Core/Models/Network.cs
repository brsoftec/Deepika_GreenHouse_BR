using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class Network
    {
        public Network()
        {
            Friends = new List<MyFriend>();
        }
        public const string TRUSTED = "TRUSTED";
        public const string NORMAL = "NORMAL";
        public const string EMERGENCY = "EMERGENCY";
        public const string TRUSTED_NETWORK = "Trust Network";
        public const string NORMAL_NETWORK = "Normal Network";
        public const string EMERGENCY_NETWORK = "Emergency Network";
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId NetworkOwner { get; set; }
        public string NetworkOwnerAccountId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        [BsonElementAttribute("Friends")]
        public List<MyFriend> Friends { get; set; }
    }

    public class EmergencyContact {

        public string AccountId { get; set; }
        public string DisplayName { get; set; }

    }

    public class MyFriend
    {
        public MyFriend()
        { }
        public MyFriend(ObjectId id)
        {
            Id = id;
        }
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string NetworkName { get; set; }
        public string Relationship { get; set; }
        public bool IsEmergency { get; set; }
        public int Rate { get; set; }
    }

    public class FriendInvitation
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public ObjectId From { get; set; }
        public ObjectId To { get; set; }
        public string NetworkName { get; set; }
        public string Relationship { get; set; }
        public bool IsEmergency { get; set; }
        public int Rate { get; set; }
        public string InviteId{ get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime SentAt { get; set; }
    }

}