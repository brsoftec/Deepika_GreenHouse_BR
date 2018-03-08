using System;
using GH.Core.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GH.Core.Models
{
    public class MongoEntity : IMongoEntity
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime CreatedOn { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local, Representation = BsonType.String)]
        public DateTime ModifiedOn { get; set; }
        public ObjectId CreatedBy { get; set; }
        public ObjectId ModifiedBy { get; set; }
    }
}