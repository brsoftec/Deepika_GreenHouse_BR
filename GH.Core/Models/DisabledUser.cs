using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GH.Core.Models
{
    public class DisabledUser : MongoEntity
    {
        public ObjectId UserId { get; set; }
        public Account User { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime EffectiveDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime? Until { get; set; }
        public string Reason { get; set; }
        public bool IsEnabled { get; set; }
    }
}