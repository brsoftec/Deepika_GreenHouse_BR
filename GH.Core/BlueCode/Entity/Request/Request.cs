using GH.Core.BlueCode.Entity.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GH.Core.BlueCode.Entity.Request
{

    [BsonDiscriminator("Request")]
    [BsonIgnoreExtraElements]
    public class Request : IMongoDBEntity
    {
        public ObjectId Id { get; set; }
        public string Type { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public string Message { get; set; }
        public string InteractionId { get; set; }
        public DateTime CreatedDate { get; set; }

    }
    public sealed class EnumRequest
    {
        public static readonly string RequestHandshake = "Request For Handshake";
        public static readonly string StatusPending = "Pending";
        public static readonly string StatusSend = "Send";
        public static readonly string StatusComplete = "Complete";
        public static readonly string StatusDelete = "Delete";
        public static readonly string StatusRead = "Read";
        public static readonly string UserCreatedBusinessType = "ucb";

    }
    
}