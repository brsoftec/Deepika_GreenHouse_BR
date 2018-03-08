using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.Models
{
    public class VerifyToken
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string RequestId { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int TryNumber { get; set; }
        public int RefreshExpiredTimeNumber { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime CreatedAt { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime ExpiredTime { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime? VerifiedAt { get; set; }
        public TokenStatus Status { get; set; }
        public VerifyPurpose Purpose { get; set; }
    }

    public enum TokenStatus
    {
        CREATED = 0,
        SENT = 1,
        VERIFIED = 2,
        CANCELED = 3,
        EXPIRED = 4,
        INVALID = 5,
        FAILED = 6,
        FINISHED = 7
    }

    public enum VerifyPurpose
    {
        RESET_PASSWORD = 0,
        AUTHENTICATION = 1,
        VerifyPhone = 2
    }
}